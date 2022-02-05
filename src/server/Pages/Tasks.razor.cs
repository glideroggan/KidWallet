using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using server.Data.Task;
using server.Data.User;
using server.Services;

/*
 * [BUG] - reserved tasks should be sorted on top (child and parent)
 * [DONE] - Inactive tasks should be visible when parent
 */

namespace server.Pages;
public class TasksBase : PageBase
{
#pragma warning disable CS8618
    [Inject] protected TaskService TaskService { get; private set; }
    [Inject] private LoginService LoginService { get; set; }
    [Inject] private ProtectedSessionStorage SessionStorage { get; set; }
#pragma warning restore CS8618
    
    protected List<TaskModel>? Tasks { get; private set; }

    protected override async Task OnInitializedAsyncCallback()
    {
        Tasks = await TaskService.GetTasksAsync(t => true);
    }

    protected async Task ChildUpdate()
    {
        // PERF: could be good to get the value that changed, instead of getting all the tasks again
        Tasks = await TaskService.GetTasksAsync(t => true);
        StateHasChanged();
    }
    
}
