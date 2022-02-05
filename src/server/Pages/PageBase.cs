using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using server.Services;

namespace server.Pages;

public abstract class PageBase : ComponentBase
{
    [Inject] private LoginService LoginService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private ProtectedSessionStorage SessionStorage { get; set; }
    [Inject] protected AppState State { get; set; }
    protected abstract Task OnInitializedAsyncCallback();
    protected sealed override async Task OnInitializedAsync()
    {
        try
        {
            var results = await LoginService.ValidateLoginAsync(async () => await SessionStorage.GetAsync<string>("token"));
            if (!results) NavigationManager.NavigateTo("/", true);
            
            // call OnInitialized
            await OnInitializedAsyncCallback();
        }
        catch (InvalidOperationException e)
        {
            // We can't do JS interop calls at this time, so we wait until render is complete and check login again
            // afterRender = true;
        }
    }
}