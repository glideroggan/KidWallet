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
    
#pragma warning restore CS8618

    [CascadingParameter] private Action<string> NotificationCallback { get;set; }

    protected int Balance;
    protected int Cost;
    protected string? Description;

    protected override Task OnInitializedAsyncCallback()
    {
        Balance = State.User.Balance;
        return OnInitializedAsync();
    }

    protected async Task Send()
    {
        return;
        // TODO: not implemented
        // TODO: check the inputs so they are ok

        // var reserveId = await AccountService.ReserveMoneyAsync(Cost);
        // await MessageService.SendMsg(RoleEnum.Parent, 
        //     $"Kan jag köpa '{Description}' för {Cost} SEK?",
        //     reserveId: reserveId);
        //
        // NotificationCallback("Skickat");
    }
}