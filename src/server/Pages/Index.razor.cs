using Microsoft.AspNetCore.Components;
using server.Data.User;
using server.Services;

/*
 * [DONE] - Add widgets about how much each kid could earn today
 */

namespace server.Pages;
public class IndexBase : ComponentBase, IDisposable
{
#pragma warning disable CS8618
    [Inject] protected AppState State { get; private set; }
    [Inject] protected UserService UserService { get; private set; }
#pragma warning restore CS8618

    protected List<UserModel> Children = new();

    
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            State.OnChange += StateChange;
        }
    }
    private void StateChange()
    {
        InvokeAsync(() => StateHasChanged());
    }
    public void Dispose()
    {
        State.OnChange -= StateChange;
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await UpdateStateAsync();
    }

    protected async Task UpdateStateAsync()
    {
        if (State.User == null) return;
        
        Children.Clear();
        Children = await UserService.GetChildrenAsync(State.User.Id);
        StateHasChanged();
    }
}