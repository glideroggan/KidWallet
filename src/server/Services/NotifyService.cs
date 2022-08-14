using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Data.DTOs;
using server.Data.Message;
using server.Data.User;
using server.Repositories;

namespace server.Services;

public enum NotificationTargetEnum
{
    None,
    Individual,
    Parent,
    Child
}

public enum MessageType
{
    None = 0,
    DoneTask,
    Buy,
    DeniedBuy
}

public record NotifyMessage(MessageType MessageType, string Msg, int? Identifier=null, int? ReceiverSpecificId=null);

public class NotifyService
{
    private readonly IRepo<NotificationDto> _repo;
    private readonly IDbContextFactory<WalletContext> _dbContextFactory;
    private readonly AppState _state;
    private readonly IRepo<ReserveDto> _reserveRepo;
    private readonly IRepo<UserDto> _userRepo;

    public NotifyService(IRepo<NotificationDto> repo, IDbContextFactory<WalletContext> dbContextFactory, AppState state, IRepo<UserDto> userRepo, IRepo<ReserveDto> reserveRepo)
    {
        _repo = repo;
        _dbContextFactory = dbContextFactory;
        _state = state;
        _reserveRepo = reserveRepo;
        _userRepo = userRepo;
    }
    internal async Task TaskApproved(int taskId, string parentName, string taskName, int money, string taskOwner)
    {
        return;
        // TODO: not implemented
        // Debug.Assert(taskId != 0);
        // Debug.Assert(parentName.Length > 1);
        // Debug.Assert(taskName.Length > 1);
        // Debug.Assert(money > 0);
        // Debug.Assert(taskOwner.Length > 1);
        //
        // await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        // var note = await _repo.GetAll(dbContext)
        //     .FirstAsync(t => t.TaskId == taskId);
        // note.Message = $"{parentName} har godkänt '{taskName}'. Du har fått {money} kr in på kontot.";
        // note.Target = NotificationTargetEnum.Individual;
        // note.TargetName = taskOwner;
        //
        // await _repo.UpdateAsync(dbContext, note);
    }

    internal async Task RemoveMessage(MessageModel msg)
    {
        // TODO: check validation, who can delete what?
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        switch (msg.Type)
        {
            default:
                // as parent, just remove message
                await NotifyActions.RemoveMessage(dbContext, _repo, msg.Id);
                await dbContext.SaveChangesAsync();
                break;
        }
    }

    internal async Task DeniedAsync(string msg, int receiverId)
    {
        var msgData = new NotifyMessage(MessageType.DeniedBuy, msg, ReceiverSpecificId:receiverId);
        await SendMsgAsync(msgData);
    }

    public async Task<IList<MessageModel>> GetMessagesAsync(Expression<Func<NotificationDto, bool>> WhereCallback)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var msgs = await _repo.GetAll(dbContext)
            .Include("Sender")
            .Where(WhereCallback)
            .Select(x => x.ToModel())
            .ToListAsync();

        return msgs;
    }

    public async Task SendMsgAsync(NotifyMessage msg)
    {
        Debug.Assert(_state.User != null);
        if (_state.User == null) return;

        var noteTarget = msg.MessageType switch
        {
            MessageType.DoneTask => NotificationTargetEnum.Parent,
            MessageType.Buy => NotificationTargetEnum.Parent,
            MessageType.DeniedBuy => NotificationTargetEnum.Individual,
            _ => throw new NotImplementedException("TODO:")
        };
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var sender = await _userRepo.GetByIdAsync(dbContext, _state.User.Id);
        UserDto? receiver = null;
        if (noteTarget == NotificationTargetEnum.Individual)
        {
            Debug.Assert(msg.ReceiverSpecificId.HasValue);
            receiver = await _userRepo.GetByIdAsync(dbContext, msg.ReceiverSpecificId.Value);
        }
        var data = new NotificationDto
        {
            Message = msg.Msg,
            IdentifierId = msg.Identifier,
            Target = noteTarget,
            Sender = sender,
            Status = MessageStatusEnum.Unread,
            TargetName = receiver?.Name,
            MessageType = msg.MessageType
        };
        
        _repo.Add(dbContext, data);
        await _repo.SaveAsync(dbContext);
        // TODO: notify the message system, this should work so that all that are already online
        // can get push updates
    }

    public async Task TaskDoneAsync(string taskDescription, int taskId)
    {
        /*
         * send message to parent that the task is done and should be checked
         */
        var msg = new NotifyMessage(MessageType.DoneTask, $"I'm done with task '{taskDescription}!", taskId);
        await SendMsgAsync(msg);
    }
}

internal static class NotifyActions
{
    internal static async Task RemoveMessage(WalletContext ctx, IRepo<NotificationDto> repo, int id)
    {
        var item = await repo.GetByIdAsync(ctx, id);
        ctx.Remove(item);
        await ctx.SaveChangesAsync();
    }
}
