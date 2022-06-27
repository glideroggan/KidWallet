using Microsoft.AspNetCore.Components;
using server.Data;
using server.Data.DTOs;
using server.Data.Message;
using server.Data.User;
using server.Services;

namespace server.Components;

public class MessagesBase : ComponentBase
{
#pragma warning disable CS8618
    [Inject] protected NotifyService NotifyService { get; set; }

    [Parameter] public UserModel? User { get; set; }
    [Inject] private NavigationManager Nav { get; set; }
#pragma warning restore CS8618

    private List<MessageModel>? Messages { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Messages = (await NotifyService.GetMessagesAsync((x) =>
            x.Status == MessageStatusEnum.Unread)).ToList();
    }

    protected async Task OnClickAsync()
    {
        Nav.NavigateTo("/messages");
        // refresh now when clicked
        Messages.Clear();
        StateHasChanged();
    }

    protected string GetMessageIcon() =>
        Messages switch
        {
            { Count: > 0 } => "assets/bootstrap-icons/bootstrap-icons.svg#envelope-check",
            _ => "assets/bootstrap-icons/bootstrap-icons.svg#envelope",
        };
}