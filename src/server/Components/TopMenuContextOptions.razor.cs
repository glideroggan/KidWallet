using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using server.Services;

namespace server.Components;

public class TopMenuContextOptionsBase : ComponentBase, IDisposable
{
#pragma warning disable CS8618
    [Inject] protected NavigationManager NavManager { get; set; }

    [Inject] private NavContextService NavContextService { get; set; }
#pragma warning restore CS8618

    protected List<(string Icon, string Url)>? Links { get; set; } = new();

    private void LocationChanged(object? sender, LocationChangedEventArgs args)
    {
        
        var uri = new Uri(NavManager.Uri);
        if (uri.Segments.Length == 1)
        {
            Links?.Clear();
            StateHasChanged();    
            return;
        }
        
        var page = uri.Segments[^1];
        Links = NavContextService.GetMenuItems(page);
        StateHasChanged();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            NavManager.LocationChanged += LocationChanged;
        }
    }
    public void Dispose()
    {
        NavManager.LocationChanged -= LocationChanged;
    }
}