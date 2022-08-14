using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Data.DTOs;
using server.Data.User;
using server.Repositories;

namespace server.Services;


public class UserService
{
    private readonly IRepo<UserDto> _repo;
    private readonly IRepo<SavingAccountDto> _savingsRepo;
    private readonly IDbContextFactory<WalletContext> _dbContextFactory;

    public UserService(IRepo<UserDto> repo, IRepo<SavingAccountDto> savingsRepo,
        IDbContextFactory<WalletContext> dbContextFactory)
    {
        _repo = repo;
        _dbContextFactory = dbContextFactory;
        _savingsRepo = savingsRepo;
    }

    internal async Task<UserModel?> GetUserAsync(string username)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await _repo.GetAll(dbContext).Include("SpendingAccount")
            .Where(u => u.Name == username)
            .FirstAsync();

        var savingRows = await _savingsRepo.GetAll(dbContext)
            .Where(a => a.UserId == user.UserId)
            .ToListAsync();

        var banked = savingRows
            .Select(ac => new
            {
                Total = savingRows.Sum(acs => acs.CalculatedFunds),
            }).FirstOrDefault();

        return new UserModel(user.UserId, user.Name, user.Password, user.ProfileImg, user.Role, 
            user.SpendingAccount?.Balance ?? 0, 
            banked?.Total ?? 0);
    }

	internal async Task<IList<TResult>> GetAll<TResult>(Func<UserDto, TResult> modelCallback)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        // NOTE: a service should not return a DTO, it can't also return an IQueryable
        return (await _repo.GetAll(dbContext)
            .ToListAsync())
            .Select(modelCallback)
            .ToList();
    }

	private record Banked(int UserId, int Total);
    private static int GetBanked(int userId, List<Banked> rows)
    {
        return rows.Any(a => a.UserId == userId) ? rows.First(a => a.UserId == userId).Total : 0;
    }
    internal async Task<List<UserModel>> GetChildrenAsync(int parentId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var savingRows = await _savingsRepo.GetAll(dbContext)
            .GroupBy(a => a.UserId)
            .Select(a => new Banked(a.Key, a.Sum(aa => aa.CalculatedFunds)))
            .ToListAsync();

        var children = await _repo.GetAll(dbContext)
            .Include("SpendingAccount")
            .Where(u => u.ParentId == parentId)
            .Select(u => u.ToModel(GetBanked(u.UserId, savingRows)))
            .ToListAsync();

        return children;
    }

    public async Task<int> GetParentAsync(int userId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var userDto = await _repo.GetAll(dbContext)
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync();
        if (userDto == null || userDto.ParentId == null)
        {
            throw new UserServiceException("User has no parent, or user doesn't exists");
        }
        return userDto.ParentId.Value;
    }
}

public class UserServiceException : Exception
{
    public UserServiceException(string msg) : base(msg)
    {
    }
}
