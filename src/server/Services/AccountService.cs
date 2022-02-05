using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Data.DTOs;
using server.Data.User;
using server.Pages;
using server.Repositories;
using server.Services.Exceptions;

namespace server.Services;

public class AccountService
{
    private readonly IRepo<SpendingAccountDto> _spendingRepo;
    private readonly IRepo<SavingAccountDto> _savingsRepo;
    private readonly IRepo<ReserveDto> _reserveRepo;
    private readonly IDbContextFactory<WalletContext> _dbContextFactory;
    private readonly AppState _state;
    private readonly IRepo<UserDto> _userRepo;
    private readonly IRepo<AccountHistoryDto> _accountHistoryRepo;

    public AccountService(IRepo<SpendingAccountDto> spendingRepo, IRepo<SavingAccountDto> savingsRepo, 
        IRepo<ReserveDto> reserveRepo, IDbContextFactory<WalletContext> dbContextFactory,
        IRepo<UserDto> userRepo, AppState state, IRepo<AccountHistoryDto> accountHistoryRepo)
    {
        _spendingRepo = spendingRepo;
        _state = state;
        _userRepo = userRepo;
        _accountHistoryRepo = accountHistoryRepo;
        _dbContextFactory = dbContextFactory;
        _reserveRepo = reserveRepo;
        _savingsRepo = savingsRepo;
    }

    public async Task<int> ReserveMoneyAsync(int cost)
    {
        // TODO: move the reserved money to a separate pool for this user, like a transactions
        // that haven't been sent
        
        // TODO: apply rules to see if the action is valid
        if (_state.User is not { Role: RoleEnum.Child })
        {
            throw new ServiceException("Du är inte inloggad");
        }
        
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        // get owner account
        // TODO: both below should be "Not tracked", as we don't want accidental changes to these entities
        var loggedInUser = await _userRepo.GetByIdAsync(dbContext, _state.User.Id);
        dbContext.Entry(loggedInUser).Reference("SpendingAccount").Load();
        var ownerAccount = await _spendingRepo.GetByIdAsync(dbContext, loggedInUser.SpendingAccount.SpendingAccountId);
        
        // get dest account, which should be parent
        var parent = await _userRepo.GetByIdAsync(dbContext, loggedInUser.ParentId.Value);
        dbContext.Entry(parent).Reference("SpendingAccount").Load();
        var destAccount = await _spendingRepo.GetByIdAsync(dbContext, parent.SpendingAccount.SpendingAccountId);
        
        var reserve = new ReserveDto
        {
            Amount = cost,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(1),
            DestAccount = destAccount,
            OwnerAccount = ownerAccount
        };
        var createdReserve = _reserveRepo.Add(dbContext, reserve);
        await _reserveRepo.SaveAsync(dbContext);
        return createdReserve.ReserveId;
    }

    public async Task<List<(string? Name, int Earnings)>> GetEarningsAsync(int weeks)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var limit = DateTime.Now.Subtract(TimeSpan.FromDays(weeks * 7));
        // get child account history
        var rows = await _accountHistoryRepo.GetAll(dbContext)
            .Where(r => r.CreatedDate > limit)
            .Include("User")
            .Where(r => r.User.Role == RoleEnum.Child)
            .Select(x => new { x.User.Name, x.Funds, x.Description })
            .ToListAsync();

        // group by each kid and sum earnings
        var list = rows
            .GroupBy(x => new { x.Name })
            .Select(x => new { x.Key.Name, Earnings = x.Sum(c => c.Funds) })
            .ToList();

        return list
            .Select(x => (x.Name, x.Earnings))
            .ToList();
    }

    public async Task KidBuyAsync(TransferModel transferData)
    {
        // TODO: refactor
        // TODO: validate model
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        // create transaction
        var transaction = _spendingRepo.CreateTransaction(dbContext);
        try
        {
            // check money on source, is enough?
            var senderUser = await _userRepo.GetAll(dbContext)
                .Where(x => x.UserId == transferData.SenderUser.Id)
                .Include("SpendingAccount")
                .FirstAsync();
            if (senderUser.SpendingAccount.Balance < transferData.Funds)
            {
                // TODO: make it a service exception
                throw new ArgumentException("Not enough Funds");
            }
            // get dest user
            var destUser = await _userRepo.GetAll(dbContext)
                .Where(x => x.UserId == senderUser.ParentId)
                .Include("SpendingAccount")
                .FirstAsync();

            // take away money from source
            senderUser.SpendingAccount.Balance -= transferData.Funds;
        
            // create account history row for source
            // TODO: should make this even easier to not have to know if it is a debit or credit even
            await AccountActions.CreateAccountHistory(dbContext, _accountHistoryRepo, 
                senderUser, destUser, -transferData.Funds, transferData.Description);

            // add money on dest
            destUser.SpendingAccount.Balance += transferData.Funds;
        
            // create account history row for dest
            await AccountActions.CreateAccountHistory(dbContext, _accountHistoryRepo, 
                destUser, senderUser, transferData.Funds, transferData.Description);

            await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            // TODO: logs?
            // TODO: notify?
            await transaction.RollbackAsync();
        }
    }
}

public static class AccountActions
{
    internal static async Task CancelReserveAsync(WalletContext ctx, IRepo<ReserveDto> repo, int reserveId)
    {
        var reserve = await repo.GetByIdAsync(ctx, reserveId);
        if (reserve == null) return;
        repo.Remove(ctx, reserve);
        await ctx.SaveChangesAsync();
    }

    internal static Task CreateAccountHistory(WalletContext ctx, IRepo<AccountHistoryDto> repo,
        UserDto sender, UserDto destUser, int funds, string description)
    {
        var model = new AccountHistoryDto
        {
            Description = description,
            Funds = funds,
            User = sender,
            CreatedDate = DateTime.UtcNow,
            UserId = sender.UserId,
            DestAccountId = destUser.SpendingAccount.SpendingAccountId,
            DestAccountType = AccountTypeEnum.Spending,
            SourceAccountId = sender.SpendingAccount.SpendingAccountId,
            SourceAccountType = AccountTypeEnum.Spending
        };
        repo.Add(ctx, model);
        
        return Task.CompletedTask;
    }
    internal static async Task CreateStat(WalletContext dbContext, IRepo<StatDto> repo, TaskDto taskDto)
    {
        var stat = StatHelper.CreateStat(taskDto);
        // get stat from db, if exists
        var statDb = await repo.GetBy2Id(dbContext, taskDto.TaskId,
            taskDto.UserId.Value);
        if (statDb != null)
        {
            statDb.Count++;
        }
        else
        {
            repo.Add(dbContext, stat);
        }
        await dbContext.SaveChangesAsync();
    }
    
    internal static async Task<string?> PayoutOfTask(WalletContext dbContext, IRepo<UserDto> userRepo, 
        IRepo<AccountHistoryDto> histRepo, TaskDto taskDto)
    {
        // get account from source
        string? taskOwner;
        Debug.Assert(taskDto.User.ParentId != null);
        var parent = await userRepo.GetAll(dbContext)
            .Include("SpendingAccount")
            .FirstAsync(u => u.UserId == taskDto.User.ParentId.Value);
        Debug.Assert(parent.SpendingAccount.Balance >= taskDto.Payout);

        // to child spending account
        var child = await userRepo.GetAll(dbContext)
            .Include("SpendingAccount")
            .FirstAsync(u => u.UserId == taskDto.User.UserId);
        taskOwner = child.Name;
        child.SpendingAccount.Balance += taskDto.Payout;
        parent.SpendingAccount.Balance -= taskDto.Payout;
        
        // create a history row
        // TODO: important, create an account history row
        var sourceHist = new AccountHistoryDto
        {
            Description = taskDto.Description,
            User = child,
            Funds = taskDto.Payout,
            CreatedDate = DateTime.UtcNow,
            UserId = child.UserId,
            DestAccountId = child.SpendingAccount.SpendingAccountId,
            DestAccountType = AccountTypeEnum.Spending,
            SourceAccountId = parent.SpendingAccount.SpendingAccountId,
            SourceAccountType = AccountTypeEnum.Spending,
        };
        var destHist = new AccountHistoryDto
        {
            Description = taskDto.Description,
            User = parent,
            Funds = -taskDto.Payout,
            CreatedDate = DateTime.UtcNow,
            UserId = parent.UserId,
            DestAccountId = child.SpendingAccount.SpendingAccountId,
            DestAccountType = AccountTypeEnum.Spending,
            SourceAccountId = parent.SpendingAccount.SpendingAccountId,
            SourceAccountType = AccountTypeEnum.Spending,
        };
        histRepo.Add(dbContext, sourceHist);
        histRepo.Add(dbContext, destHist);

        await dbContext.SaveChangesAsync();
        
        return taskOwner;
    }
}
