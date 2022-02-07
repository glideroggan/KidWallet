using server.Data.Task;
using Microsoft.EntityFrameworkCore;
using server.Data.DTOs;
using server.Data.User;
using server.Repositories;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using server.Data;
using server.Extensions;
using server.Services.Exceptions;
using server.Services.Models;

[assembly: InternalsVisibleTo("tests")]
namespace server.Services;



/*
 * [DONE] - remove clean room for all
 * [DONE] - the once bool wasn't honored when task was approved
 * [DONE] - tasks are returned that are already reserved by other child
 * [DONE] - When updating from db, it can sometimes be a bit slow and it doesn't look right when deleting tasks
 *      We can test this by slowing down the connection and try deleting some tasks
 */


public class TaskService
{
    private IRepo<TaskDto> _repo;
    private readonly IRepo<UserDto> _userRepo;
    private readonly NotifyService _notifyService;
    private readonly IRepo<SpendingAccountDto> _spendingRepo;
    private readonly AppState _state;
    private readonly IRepo<StatDto> _statRepo;
    private readonly IDbContextFactory<WalletContext> _dbContextFactory;
    private readonly IRepo<AccountHistoryDto> _accountHistoryRepo;

    public TaskService(IRepo<TaskDto> repo, IRepo<StatDto> statRepo, IRepo<UserDto> userRepo, 
        IRepo<SpendingAccountDto> spendingRepo, IRepo<AccountHistoryDto> accountHistoryRepo, 
        NotifyService notifyService,
        IDbContextFactory<WalletContext> dbContextFactory,
        AppState state)
    {
        _dbContextFactory = dbContextFactory;
        _repo = repo;
        _accountHistoryRepo = accountHistoryRepo;
        _userRepo = userRepo;
        _notifyService = notifyService;
        _spendingRepo = spendingRepo;
        _state = state;
        _statRepo = statRepo;
    }

    

    private class StatusComparer : IComparer<StatusEnum>
    {
        public int Compare(StatusEnum a, StatusEnum b)
        {
            return a > b ? -1 : 1;
        }
    }
    internal async Task<List<TaskModel>> GetTasksAsync(Expression<Func<TaskDto, bool>> WhereClause)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var dtos = await _repo.GetAll(dbContext).Include("User")
            .Include("TargetUser")
            .Where(WhereClause)
            // TODO: add the other filter here also
            .ToListAsync();
        
        return dtos
            .Where(GetValidTasksBasedOnUserAndRole)
            .Select(d => d.ToModel())
            .OrderBy(d => d.Status, new StatusComparer())
            .ToList();
    }

    internal async Task<TaskModel> SetAsDone(int taskId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var taskDto = await _repo.GetByIdAsync(dbContext, taskId);
        taskDto.Status = StatusEnum.WaitingForApproval;
        await _repo.UpdateAsync(dbContext, taskDto);
        await _repo.SaveAsync(dbContext);
        // TODO: we should emit event, so that notification service can notify on them
        await _notifyService.TaskDoneAsync(taskDto.Description, taskId);
        return taskDto.ToModel();
    }

    internal async Task Approve(int taskId)
    {
        // CLEAN: refactor this please
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var trans = _repo.CreateTransaction(dbContext);
        string? taskName = string.Empty;
        int money = 0;
        string taskOwner = string.Empty;
        try
        {
            var taskDto = await _repo.GetAll(dbContext)
                .Where(x => x.TaskId == taskId)
                .Include("User")
                .Include("TargetUser")
                .FirstAsync();
            Debug.Assert(taskDto != null);
            Debug.Assert(taskDto.UserId != null);
            taskName = taskDto.Description;
            money = taskDto.Payout;
            
            // create stat of the transaction
            // workaround until we change the setup in db, by adding migrations to this project
            // NOTE: transaction is needed for this to be persistent to the same session
            _statRepo.ExecuteSqlCommand(dbContext, "SET IDENTITY_INSERT [dbo].[doneTasks] ON");
            await AccountActions.CreateStat(dbContext, _statRepo, taskDto);

            // Transfer fund 
            
            taskOwner = await AccountActions.PayoutOfTask(dbContext, _userRepo, _accountHistoryRepo, taskDto);

            // Clone task with a new NotBefore
            var date = DateTime.UtcNow;
            var anyDay = taskDto.DayInTheWeek;
            var cloneTask = false;
            if (anyDay != 0)
            {
                date = DayAndWeekHelper.ComputeNextDate((uint)anyDay, DateTime.UtcNow);
                cloneTask = true;
            }
            if (taskDto.Week)
            {
                date = DayAndWeekHelper.ComputeNextDate(taskDto.EveryOtherWeek, DateTime.UtcNow);
                cloneTask = true;
            }
            if (cloneTask && !taskDto.Once)
            {
                var newTaskDto = new TaskDto
                {
                    Description = taskDto.Description,
                    ImgUrl = taskDto.ImgUrl,
                    Payout = taskDto.Payout,
                    Status = StatusEnum.Available,
                    Week = taskDto.Week,
                    EveryOtherWeek = taskDto.EveryOtherWeek,
                    DayInTheWeek = taskDto.DayInTheWeek,
                    NotBefore = date,
                    Created = date,
                };
                TaskActions.CreateTask(dbContext, _repo, newTaskDto);
            }

            // remove old task
            _repo.Remove(dbContext, taskDto);

            
            await _repo.SaveAsync(dbContext);

            await trans.CommitAsync();
            
            // notify
            // BUG: no notifications
            // await _notifyService.TaskApproved(taskId, _state.User.Name, taskName, money, taskOwner);
        }
        catch (Exception ex)
        {
            // TODO: log error
            await trans.RollbackAsync();
        }

        
    }

    internal async Task<TaskModel> ReserveAsync(int taskId, int userId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var taskDto = await _repo.GetAll(dbContext)
            .Where(x => x.TaskId == taskId)
            .Include("User")
            .FirstAsync();
        /* TODO: not sure I want it like this, problem is that I need to reload from db as we don't have any notification between
         * users when they update the db.
         * we could add something to dbcontext, so that when one user is doing a save, other dbcontexts would reload these entities
         * 
         * Or we would do everything without tracking with entity framework, and then build a caching system that would span over
         * all contexts for all users
         */
        taskDto = _repo.Reload(dbContext, taskDto);

        if (taskDto.Status != StatusEnum.Available)
        {
            // TODO: what is the best plan to show errors?
            throw new ServiceException("Denna uppgift är inte ledig längre");
        }
        
        taskDto.UserId = userId;
        taskDto.Status = StatusEnum.OnGoing;
        await _repo.UpdateAsync(dbContext, taskDto);
        await _repo.SaveAsync(dbContext);
        // PERF: shouldn't be needed, we should just update the user prop also, by getting the user
        taskDto.User = _userRepo
            .GetAll(dbContext)
            .First(u => u.UserId == userId);
        return taskDto.ToModel();
    }

    internal async Task<TaskModel> DisApprove(int taskId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var taskDto = await _repo.GetByIdAsync(dbContext, taskId);
        taskDto.Status = StatusEnum.OnGoing;
        await _repo.UpdateAsync(dbContext, taskDto);
        await _repo.SaveAsync(dbContext);
        return taskDto.ToModel();
    }

    internal async Task<TaskModel> UnReserveAsync(int taskId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var taskDto = await _repo.GetByIdAsync(dbContext, taskId);
        taskDto.UserId = null;
        taskDto.User = null;
        taskDto.Status = StatusEnum.Available;
        await _repo.UpdateAsync(dbContext, taskDto);
        await _repo.SaveAsync(dbContext);
        return taskDto.ToModel();
    }

    /// <summary>
    /// Parent
    ///     Get all tasks, even inactive ones
    /// Child
    ///     Get all free tasks
    /// </summary>
    /// <param name="taskDto"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private bool GetValidTasksBasedOnUserAndRole(TaskDto taskDto)
    {
        return (_state.User?.Role, taskDto) switch
        {
            (RoleEnum.Parent, _) => true,
            (RoleEnum.Child, { Status: StatusEnum.Available, NotBefore: var notBefore}) 
                when notBefore > DateTime.UtcNow  => false,
            (RoleEnum.Child, { Status: StatusEnum.Available, TargetUser: null }) => true,
            (RoleEnum.Child, { Status: StatusEnum.Available } dto) 
                when dto.TargetUser.UserId == _state.User.Id =>  true,
            (RoleEnum.Child, { Status: StatusEnum.OnGoing, UserId: not null }) => taskDto.UserId == _state.User.Id,
            _ => false,
        };
    }

    public async Task CreateNewAsync(CreateTask model)
    {
        var notBefore = model.Daily
            ? DayAndWeekHelper.ComputeNextDate(model.DayOfTheWeek, DateTime.Today.Subtract(TimeSpan.FromDays(1)))
            : DayAndWeekHelper.ComputeNextDate((int)0, DateTime.UtcNow);
        var taskDto = new TaskDto
        {
            Created = DateTime.UtcNow,
            Description = model.Description,
            Payout = model.Payout,
            Week = !model.Daily,
            ImgUrl = model.ImageUrl,
            DayInTheWeek = model.DayOfTheWeek,
            SpecificUserId = model.SpecificUserId,
            NotBefore = notBefore,
            Once = model.Once,
            Status = StatusEnum.Available
        };
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        _repo.Add(dbContext, taskDto);
        await _repo.SaveAsync(dbContext);
    }

    public async Task RemoveAsync(int taskId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var t = await _repo.GetByIdAsync(dbContext, taskId);
        _repo.Remove(dbContext, t);
        await _repo.SaveAsync(dbContext);
    }
}

internal static class StatHelper
{
    internal static StatDto CreateStat(TaskDto taskDto)
    {
        var stat = new StatDto
        {
            Count = 1,
            TaskId = taskDto.TaskId,
#pragma warning disable CS8629
            UserId = taskDto.UserId.Value,
#pragma warning restore CS8629
            Description = taskDto.Description
        };
        return stat;
    }
}

[Flags]
public enum DaysEnum : uint
{
    None = 0U,
    Monday = 2,
    Tuesday = 4,
    Wednesday = 8,
    Thursday = 16,
    Friday = 32,
    Saturday = 64,
    Sunday = 128
}

public static class DayAndWeekHelper
{
    public static uint Encoder(DaysEnum days)
    {
        return (uint)days;
    }

    public static uint Encoder(params bool[] arr)
    {
        var sb = new StringBuilder(arr.Length);

        for (var index = arr.Length - 1; index >= 0; index--)
        {
            sb.Append(arr[index] ? "1" : "0");
        }

        var s = sb.ToString().PadLeft(7, '0').PadRight(8, '0');
        var val = Convert.ToUInt32(s, 2);
        return val;
    }

    public static DaysEnum GetDays(uint anyday)
    {
        return (DaysEnum)anyday;
    }

    public static DateTime ComputeNextDate(uint anyday, DateTime utcNow)
    {
        // TODO: god damn mess, could probably be some small function....
        if (anyday == 0) throw new ArgumentNullException();

        var dayOfTheWeek = GetDays(anyday);
        var days = Enum.GetNames(typeof(DaysEnum));
        DayOfWeek? firstDay = null;
        DayOfWeek? nextDay = null;
        foreach (string day in days)
        {
            var d = (uint)(DaysEnum)Enum.Parse(typeof(DaysEnum), day);
            if (((uint)dayOfTheWeek & d) != 0)
            {
                DayOfWeek startDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day);
                if (firstDay == null)
                {
                    firstDay = startDay;
                }
                else if (startDay > utcNow.DayOfWeek)
                {
                    nextDay = startDay;
                    break;
                }
            }
        }

        var dayCounter = 1;
        while (true)
        {
            if (utcNow.AddDays(dayCounter).Date.DayOfWeek == nextDay) break;
            else if (utcNow.AddDays(dayCounter).Date.DayOfWeek == firstDay) break;
            dayCounter++;
        }

        return utcNow.AddDays(dayCounter).Date;
    }

    public static DateTime ComputeNextDate(int everyOtherWeek, DateTime utcNow)
    {
        return utcNow.StartOfWeek(DayOfWeek.Monday).AddDays(7 * everyOtherWeek);
    }

    public static int Decode(uint dayInTheWeek)
    {
        var selectedDays = GetDays(dayInTheWeek);
        var days = Enum.GetValues(typeof(DaysEnum)).Cast<uint>();
        return days.Count(x => ((uint)selectedDays & x) != 0);
    }
}

public static class TaskActions
{
    public static void CreateTask(WalletContext dbContext, IRepo<TaskDto> repo, TaskDto model)
    {
        // put validations here
        if (model.UserId != null) throw new ArgumentException("Don't create task with an attached user");
        
        repo.Add(dbContext, model);
    }   
}