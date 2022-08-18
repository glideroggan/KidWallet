using Microsoft.AspNetCore.Components;

namespace server.Shared;

public class NavMenuBase : ComponentBase, IDisposable
{
#pragma warning disable CS8618
    [Inject] protected AppState State { get; private set; }
    [Inject] protected IWebHostEnvironment Env { get; private set; }
#pragma warning restore CS8618
    private bool _collapseNavMenu = true;
    protected string? NavMenuCssClass => _collapseNavMenu ? "collapse" : null;
    protected void ToggleNavMenu()
    {
        _collapseNavMenu = !_collapseNavMenu;
    }

    private Action _delegate = null!;
    
	protected override void OnAfterRender(bool firstRender)
	{
        if (firstRender)
        {
            _delegate = StateChange;
            State.OnChange += _delegate;
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

}
