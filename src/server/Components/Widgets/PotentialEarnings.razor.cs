using Microsoft.AspNetCore.Components;
using server.Data.User;
using server.Services;

namespace server.Components.Widgets;

/*
 * [TODO] - make this available on kids home page also, but just showing for her/him
 * [BUG] - Does this really update when other tasks have been completed? or does it count inactive?
 */

public class PotentialEarningsBase : ComponentBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Inject] private TaskService TaskService { get; set; }

    [Inject] private UserService UserService { get; set; }
    [Inject] private AppState State { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected List<(string Name, int CanEarnToday)> children = new();

    [Parameter] public RoleEnum Role { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // get kids
        var kidsTask = UserService.GetChildrenAsync(State.User.Id);
        // get tasks
        var tasks = await TaskService.GetTasksAsync(x => x.NotBefore < DateTime.UtcNow);
        // foreach kid, filter correct tasks
        var kids = await kidsTask;
        children = kids.Select(x => (x.Name, 0)).ToList();
        for (int i = 0; i < children.Count; i++) 
        {
            var name = children[i].Name;
            var money = tasks
                .Where(x => x.TargetUser == null || x.TargetUser.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.Payout);
            children[i] = (name, money); 
        }
    }
}
