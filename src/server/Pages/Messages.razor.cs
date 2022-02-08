using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using server.Data.DTOs;
using server.Data.Message;
using server.Data.User;
using server.Services;

namespace server.Pages;

public class MessagesBase : ComponentBase
{
#pragma warning disable CS8618
    [Inject] private NotifyService NotifyService { get; set; }
    [Inject] private AccountService AccountService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private LoginService LoginService { get; set; }
    [Inject] private ProtectedSessionStorage SessionStorage { get; set; }
    [Inject] private AppState _state { get; set; }
#pragma warning restore CS8618
    
    protected List<MessageModel>? Msgs { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var results = await LoginService.ValidateLoginAsync(async () => await SessionStorage.GetAsync<string>("token"));
            if (!results) NavigationManager.NavigateTo("/", true);
            
            await UpdateMessages();
        }
        catch (InvalidOperationException e)
        {
            // We can't do JS interop calls at this time, so we wait until render is complete and check login again
            // afterRender = true;
        }
        
    }

    private async Task UpdateMessages()
    {
        var query = await NotifyService.GetMessagesAsync(x =>
            x.Status == MessageStatusEnum.Unread || x.Sender.UserId == _state.User.Id);
        Msgs = query.ToList();
    }

    protected async Task Denied(int msgId)
    {
        return;
        
        /*
         * Wait with this, the idea is to deny purchases that kids asks for, but right now much of the flow doesn't work
         */
        
        // deny the request, the reserve should be cancelled
        var msg = Msgs.First(x => x.Id == msgId);
        await NotifyService.DeniedAsync(msgId);
    }

    protected string GetStatus(MessageModel msg)
    {
        return msg switch
        {
            { Status: MessageStatusEnum.Unread } => "unread",
            { Status: MessageStatusEnum.Denied } => "denied",
            _ => ""
        };
    }

    protected async Task Remove(MessageModel msg)
    {
        await NotifyService.RemoveMessage(msg);
        await UpdateMessages();
        StateHasChanged();
    }

    protected bool IsRemovalDisabled(MessageModel msg)
    {
        return msg switch
        {
            { } when _state.User.Role == RoleEnum.Parent => false, 
            { Sender: var sender} when sender.Id != _state.User.Id => true,
            _ => false
        };
    }

    protected bool IsActionsDisabled(MessageModel msg)
    {
        return msg switch
        {
            {Type: MessageType.DoneTask} 
                when _state.User.Role == RoleEnum.Parent => true,
            { Sender: var sender } when sender.Id == _state.User.Id => true,
            _ => false
        };
    }
}