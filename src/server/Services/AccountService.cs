using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Web;
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

    // TODO: we really need an easy function to call here to get the users balance

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cost">The amount in a negative value</param>
    /// <param name="sourceUserId">User Id to draw money from</param>
    /// <param name="destUserId">User Id that will receive the money</param>
    /// <returns>Id of reserve row</returns>
    /// <exception cref="AccountException"></exception>
    public async Task<int> ReserveMoneyAsync(int cost, int sourceUserId, int destUserId)
    {
        // * Check that sender have the money
        if (_state.User.Balance < cost)
        {
            throw new AccountException("Not enough money");
        }

        // reserve the money
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var transaction = _spendingRepo.CreateTransaction(dbContext);
        var reserveDto = await AccountActions.TransferToReserveAsync(dbContext, _reserveRepo, _userRepo,
            sourceUserId, destUserId, cost);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        // update balance for state (depends if the source user is the same as the current user)
        if (_state.User.Id == sourceUserId)
        {
            await _state.NotifyStateChanged();
        }


        return reserveDto.ReserveId;
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

    public async Task CancelReserveAsync(int reserveId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var reserveDto = await _reserveRepo.GetByIdAsync(dbContext, reserveId);
        Debug.Assert(reserveDto != null);

        await AccountActions.CancelReserveAsync(dbContext, _reserveRepo, reserveDto.ReserveId);
    }

    public async Task ExecuteReservationAsync(int reserveId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var transaction = _spendingRepo.CreateTransaction(dbContext);
        var reserveDto = await _reserveRepo.GetByIdAsync(dbContext, reserveId);
        Debug.Assert(reserveDto != null);
        await AccountActions.ExecuteReserveAsync(dbContext, _reserveRepo, reserveDto.ReserveId);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task TransferToSavingsAsync(int amount, int projectedAmount, DateTime releaseDate)
    {
        /* 
         * validations
         *      check that we have the amount in spending account
         * create a row in savings account with the amount
         * reduce the amount from spending account
         */
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var transaction = _spendingRepo.CreateTransaction(dbContext);
        
        // validate
        var user = await _userRepo.GetAll(dbContext).Where(x => x.UserId == _state.User.Id)
            .Include("SpendingAccount")
            .FirstOrDefaultAsync();
        var spendingAccount = user.SpendingAccount;
        if (spendingAccount.Balance < amount)
        {
            throw new AccountException("Not enough money");
        }
        if (releaseDate < DateTime.Now)
        {
            throw new AccountException("Release date must be in the future");
        }
        
        // savings account
        var savingsData = new SavingAccountDto()
        {
            User = user,
            CalculatedFunds = projectedAmount,
            DepositFunds = amount,
            ReleaseDate = releaseDate,
            UserId = user.UserId,
        };
        _savingsRepo.Add(dbContext, savingsData);
        
        // reduce
        spendingAccount.Balance -= amount;
        
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        await _state.NotifyStateChanged();
    }

    public async Task<List<SavingAccountRowModel>> GetSavingsAsync(int userId)
    {
        await DoOutstandingTransfersAsync(); 
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var savings = await _savingsRepo.GetAll(dbContext)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.ReleaseDate)
            .Select(x => x.ToModel())
            .ToListAsync();

        return savings;
    }

    private async Task DoOutstandingTransfersAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var transaction = _spendingRepo.CreateTransaction(dbContext);
        var outstandingTransfers = await _savingsRepo.GetAll(dbContext)
            .Where(x => x.ReleaseDate <= DateTime.UtcNow)
            .Include(x => x.User).ThenInclude(x => x.SpendingAccount)
            .ToListAsync();
        if (outstandingTransfers.Count == 0) return;

        foreach (var savingAccountDto in outstandingTransfers)
        {
            Debug.Assert(savingAccountDto.User.SpendingAccount != null);
            AccountActions.TransferFromSavingsAsync(dbContext, _savingsRepo, 
                savingAccountDto, savingAccountDto.User.SpendingAccount);
        }
        
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        await _state.NotifyStateChanged();
    }
}

public class AccountException : Exception
{
    public AccountException(string msg) : base(msg)
    {
    }
}

public static class AccountActions
{
    internal static async Task CancelReserveAsync(WalletContext ctx, IRepo<ReserveDto> repo, int reserveId)
    {
        // get the reserve
        var reserve = await repo.GetAll(ctx)
            .Where(x => x.ReserveId == reserveId)
            .Include("OwnerAccount")
            .FirstOrDefaultAsync();
        // return money to sender
        Debug.Assert(reserve != null, nameof(reserve) + " != null");
        var sourceSpendingAccount = reserve.OwnerAccount;
        sourceSpendingAccount.Balance -=
            reserve.Amount; // the amount is negative from the beginning, so returning needs to be positive
        
        // delete reserve
        repo.Remove(ctx, reserve);

        await ctx.SaveChangesAsync();
    }

    internal static async Task<ReserveDto> TransferToReserveAsync(WalletContext ctx, IRepo<ReserveDto> reserveRepo,
        IRepo<UserDto> userRepo, int sourceUserId, int destUserId, int amount)
    {
        // reduce spending account balance
        var sourceUser = await userRepo.GetAll(ctx).Include("SpendingAccount")
            .FirstOrDefaultAsync(a => a.UserId == sourceUserId);
        var destUser = await userRepo.GetAll(ctx).Include("SpendingAccount")
            .FirstOrDefaultAsync(a => a.UserId == destUserId);
        Debug.Assert(sourceUser != null && destUser != null);

        var sourceSpendingAccountDto = sourceUser.SpendingAccount;
        var destSpendingAccountDto = destUser.SpendingAccount;
        Debug.Assert(sourceSpendingAccountDto != null && destSpendingAccountDto != null);

        sourceSpendingAccountDto.Balance += amount;
        // add a reserve row
        var dto = new ReserveDto
        {
            Amount = amount,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(1),
            OwnerAccount = sourceSpendingAccountDto,
            DestAccount = destSpendingAccountDto
        };

        var model = reserveRepo.Add(ctx, dto);
        await ctx.SaveChangesAsync();
        return model;
    }

    public static async Task ExecuteReserveAsync(WalletContext dbContext, IRepo<ReserveDto> reserveRepo,
        int reserveDtoReserveId)
    {
        var reserve = await reserveRepo.GetAll(dbContext)
            .Where(x => x.ReserveId == reserveDtoReserveId)
            .Include("OwnerAccount")
            .Include("DestAccount")
            .FirstOrDefaultAsync();
        Debug.Assert(reserve != null);

        var funds = -reserve.Amount;
        Debug.Assert(funds > 0);
        var destAccount = reserve.DestAccount;

        // only add to dest account, as the reservation is already deducted from the source account
        destAccount.Balance += funds;

        await dbContext.SaveChangesAsync();

        // remove reserve
        reserveRepo.Remove(dbContext, reserve);
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

    public static void TransferFromSavingsAsync(WalletContext dbContext, IRepo<SavingAccountDto> savingsRepo, 
        SavingAccountDto fromAccount,
        SpendingAccountDto toAccount)
    {
        // TODO: should this be in history?
        
        var amount = fromAccount.CalculatedFunds;
        toAccount.Balance += amount;
        
        savingsRepo.Remove(dbContext, fromAccount);
    }
}