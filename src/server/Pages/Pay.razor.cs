using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using server.Data.User;
using server.Services;
using Microsoft.AspNetCore.Components;

namespace server.Pages;

public class PayBase : PageBase
{
#pragma warning disable CS8618
    [Inject] private AccountService AccountService { get; set; }
    [Inject] private NotifyService MessageService { get; set; }
    [Inject] private UserService UserService { get; set; }

#pragma warning restore CS8618

    protected int Balance;
    protected int Cost;
    protected string? Description;

    protected override Task OnInitializedAsyncCallback()
    {
        Balance = State.User.Balance;
        return Task.CompletedTask;
    }

    protected async Task Send()
    {
        var parentId = await UserService.GetParentAsync(State.User.Id);
        var reserveId = await AccountService.ReserveMoneyAsync(-Cost, State.User.Id, parentId);
        var msg = new NotifyMessage(MessageType.Buy,
            $"Kan jag köpa '{Description}' för {Cost} SEK?", reserveId);
        await MessageService.SendMsgAsync(msg);
        NotificationCallback("Skickat");
        // TODO: after actions like this, we should reset the page, so the values are back to default
        
        
        // TODO: there will be a balance change after this on the affected account, so we should send out an event
        // so that pages can update their balances
    }
}