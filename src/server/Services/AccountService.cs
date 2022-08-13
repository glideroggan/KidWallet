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
        throw new NotImplementedException();
        // Wait with this until we have the "kid pay" flow ready
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

    public async Task KidBuyAsync(KidBuyModel kidBuyData)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await kidBuyData.ValidateAsync(dbContext, _userRepo);

        // create transaction
        var transaction = _spendingRepo.CreateTransaction(dbContext);
        try
        {
            // get sender user
            var senderUser = await _userRepo.GetAll(dbContext)
                .Where(x => x.UserId == kidBuyData.SenderUser.Id)
                .Include("SpendingAccount")
                .FirstAsync();
            // get dest user
            var destUser = await _userRepo.GetAll(dbContext)
                .Where(x => x.UserId == senderUser.ParentId)
                .Include("SpendingAccount")
                .FirstAsync();

            // do the transfer
            await AccountActions.TransferFundsAsync(dbContext, _spendingRepo,
                senderUser.SpendingAccount.SpendingAccountId,
                destUser.SpendingAccount.SpendingAccountId, kidBuyData.Funds);

            // create account history row for source
            await AccountActions.CreateAccountHistory(dbContext, _accountHistoryRepo,
                senderUser, destUser, -kidBuyData.Funds, kidBuyData.Description);

            // create account history row for dest
            await AccountActions.CreateAccountHistory(dbContext, _accountHistoryRepo,
                destUser, senderUser, kidBuyData.Funds, kidBuyData.Description);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new ServiceException(e.Message);
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
        var debug = repo.Add(ctx, model);

        return Task.CompletedTask;
    }

    internal static async Task CreateStat(WalletContext dbContext, IRepo<StatDto> repo, TaskDto taskDto)
    {
        // TODO: thinking here is wrong, as we remove the tasks, so it will create a new taskId, so we can't base the stats on the taskId
        var stat = StatHelper.CreateStat(taskDto);
        // get stat from db, if exists
        var statDb = repo.GetAll(dbContext)
            .FirstOrDefault(x => x.Description == stat.Description && x.UserId == stat.UserId);
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
        IRepo<SpendingAccountDto> spendingRepo,
        IRepo<AccountHistoryDto> histRepo, TaskDto taskDto)
    {
        // get account from source
        Debug.Assert(taskDto.User.ParentId != null);
        var parent = await userRepo.GetAll(dbContext)
            .Include("SpendingAccount")
            .FirstOrDefaultAsync(u => u.UserId == taskDto.User.ParentId.Value);
        Debug.Assert(parent != null);
        //var parent = users
        //    .Include("SpendingAccount")
        //    .FirstAsync(u => u.UserId == taskDto.User.ParentId.Value);
        Debug.Assert(parent.SpendingAccount.Balance >= taskDto.Payout);

        // to child spending account
        var child = await userRepo.GetAll(dbContext)
            .Include("SpendingAccount")
            .FirstAsync(u => u.UserId == taskDto.User.UserId);
        var taskOwner = child.Name;

        // transfer
        await TransferFundsAsync(dbContext, spendingRepo,
            parent.SpendingAccount.SpendingAccountId,
            child.SpendingAccount.SpendingAccountId, taskDto.Payout);

        // create a history rows
        await CreateAccountHistory(dbContext, histRepo, parent, child, -taskDto.Payout, taskDto.Description);
        await CreateAccountHistory(dbContext, histRepo, child, parent, taskDto.Payout, taskDto.Description);

        await dbContext.SaveChangesAsync();

        return taskOwner;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="repo"></param>
    /// <param name="sourceAccountId">Account to get money from</param>
    /// <param name="destAccountId">Account to receive money</param>
    /// <param name="funds">Positive value</param>
    public static async Task TransferFundsAsync(WalletContext dbContext, IRepo<SpendingAccountDto> repo,
        int sourceAccountId, int destAccountId, int funds)
    {
        Debug.Assert(funds > 0);
        var sourceAccount = await repo.GetAll(dbContext)
            .FirstAsync(a => a.SpendingAccountId == sourceAccountId);
        var destAccount = await repo.GetAll(dbContext)
            .FirstAsync(a => a.SpendingAccountId == destAccountId);

        sourceAccount.Balance -= funds;
        destAccount.Balance += funds;

        await dbContext.SaveChangesAsync();
    }
}