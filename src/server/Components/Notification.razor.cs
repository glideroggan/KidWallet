using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace server.Components;

public class NotificationBase : ComponentBase
{
    [Parameter] public NotificationMessage? Message { get; set; }
    [Parameter] public EventCallback OnDone { get; set; }
    protected string styles { get; set; } = "hidden";
    
    private Timer? _notificationTimer;

    protected override void OnParametersSet()
    {
        if (Message == null || _notificationTimer != null) return;
        
        _notificationTimer = new Timer(3000);
        _notificationTimer.Elapsed += (_, _) =>
        {
            styles = "hidden";
            _notificationTimer.Enabled = false;
            _notificationTimer = null;
            Message = null;
            InvokeAsync(() => OnDone.InvokeAsync());
            InvokeAsync(() => StateHasChanged());
        };
        
        styles = "shown";
        StateHasChanged();
        _notificationTimer.Start();
    }
}

public record NotificationMessage(string Message);
