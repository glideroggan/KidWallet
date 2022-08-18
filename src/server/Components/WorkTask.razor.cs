using Microsoft.AspNetCore.Components;
using server.Data.Task;
using server.Data.User;
using server.Pages;
using server.Services;
using server.Services.Exceptions;


/* 
 * [DONE] - When removing a task we wait for server response, we should indicate this in some way on the card
 *      Maybe the card will show a circle waiting? https://loading.io/css/
 *      Alittle janky animation, maybe fine-tune it so things don't jump around?
 */
namespace server.Components;
public class WorkTaskBase : PageBase
{
    [Parameter] public TaskModel Data { get; set; }
    [Parameter] public EventCallback OnChange { get; set; }

    protected override Task OnInitializedAsyncCallback()
    {
        return Task.CompletedTask;
    }

    [Inject] public TaskService TaskService { get; set; }

    protected bool Waiting { get; set; } 

    protected async Task ClickedCard()
    {
        try
        {
            if (Data.NotBefore > DateTime.UtcNow) return;

            Waiting = true;
            switch (State.User?.Role)
            {
                case RoleEnum.Parent when Data.User == null:
                    return;
                case RoleEnum.Parent when Data.User?.Role == RoleEnum.Child:
                    Data = await TaskService.UnReserveAsync(Data.Id);
                    break;
                default:
                {
                    if (Data.User != null && State.User?.Id == Data.User.Id)
                    {
                        Data = await TaskService.UnReserveAsync(Data.Id);
                    }
                    else
                    {
                        Data = await TaskService.ReserveAsync(Data.Id, State.User.Id);
                    }

                    break;
                }
            }
        }
        catch (ServiceException e)
        {
            NotificationCallback(e.Message);
            await OnChange.InvokeAsync();
        }
        finally
        {
            Waiting = false;
        }

        StateHasChanged();
    }

    protected async Task OnDoneAndApprove()
    {
        // TODO: do the different tasks that is needed depending on role
        Waiting = true;
        switch (State.User.Role, Data.Status)
        {
            case (RoleEnum.Child, StatusEnum.OnGoing):
                await TaskService.SetAsDone(Data.Id);
                break;
            case (RoleEnum.Parent, StatusEnum.WaitingForApproval):
                await TaskService.Approve(Data.Id);
                break;
            default: throw new NotImplementedException("There are more cases");
        }
        await OnChange.InvokeAsync();
        Waiting = false;
    }

    protected async Task OnDisapprove()
    {
        // TODO: put some feedback to the card when pushing the buttons
        // start with just some spinner, and disallowing any other clicks
        // because a call to backend could take time
        // or, queue it and send it, so change on the data, and queue the BE call
        // TODO: wrap the waiting?
        Waiting = true;
        switch (State.User.Role, Data.Status)
        {
            case (RoleEnum.Parent, StatusEnum.WaitingForApproval):
                await TaskService.DisApprove(Data.Id);
                break;
            default: throw new NotImplementedException("There are more cases");
        }
        Waiting = false;
        await OnChange.InvokeAsync();
        
    }

    protected async Task OnRemoveTask()
    {
        if (State.User.Role != RoleEnum.Parent) return;
        
        Waiting = true;
        await TaskService.RemoveAsync(Data.Id);
        await OnChange.InvokeAsync();
        Waiting = false;
    }

    protected string CanWeTrashIt()
    {
        // TODO: not that good solution to not being logged in?
        if (State.User == null) return "";
        return (Data.Status, State.User.Role) switch
        {
            (_, RoleEnum.Parent) => "assets/bootstrap-icons/bootstrap-icons.svg#trash",
            _ => ""
        };
    }
        
    
    /// <summary>
    /// Check mark
    ///  WaitingForApproval, Parent
    /// None
    ///  WaitingForApproval, Child
    /// </summary>
    /// <returns></returns>
    protected string GetApproveAndDoneButtonIcon()
    {
        if (State.User == null) return "";
        return (Data.Status, State.User.Role) switch
        {
            (StatusEnum.WaitingForApproval, RoleEnum.Parent) => "assets/bootstrap-icons/bootstrap-icons.svg#check",
            (StatusEnum.OnGoing, RoleEnum.Child) => "assets/bootstrap-icons/bootstrap-icons.svg#check",
            _ => ""
        };
    }
        

    /// <summary>
    /// Deny
    ///  WaitingForApproval, Parent
    /// None
    ///  WaitingForApproval, Child
    /// </summary>
    /// <returns></returns>
    protected string GetApproveButtonIconDenied()
    {
        if (State.User == null) return "";
        return (Data.Status, State.User.Role) switch
        {
            (StatusEnum.WaitingForApproval, RoleEnum.Parent) => "assets/bootstrap-icons/bootstrap-icons.svg#emoji-frown-fill",
            _ => "",
        };
    }
        

}
