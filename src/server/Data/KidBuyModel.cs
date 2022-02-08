using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using server.Data.DTOs;
using server.Data.User;
using server.Repositories;

namespace server.Data;

public record KidBuyModel(UserModel SenderUser, int Funds, string Description)
{
    public async System.Threading.Tasks.Task ValidateAsync(WalletContext dbContext, IRepo<UserDto> userRepo)
    {
        if (Funds <= 0) throw new ValidationException("No need as the funds needs to be more than zero");
        
        // check money on source, is enough?
        var senderUser = await userRepo.GetAll(dbContext)
            .Where(x => x.UserId == SenderUser.Id)
            .Include("SpendingAccount")
            .FirstAsync();
        if (senderUser.SpendingAccount.Balance < Funds)
        {
            throw new ValidationException("Not enough Funds");
        }
    }
}
