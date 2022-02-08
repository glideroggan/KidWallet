using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using server.Data.User;
using server.Services;
using server.Services.Exceptions;

namespace server.Pages;

public abstract class PageBase : ComponentBase
{
#pragma warning disable CS8618
    [Inject] private LoginService LoginService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private ProtectedSessionStorage SessionStorage { get; set; }
    [CascadingParameter] protected Action<string> NotificationCallback { get; set; }
    [Inject] protected AppState State { get; set; }
#pragma warning restore CS8618

    protected virtual bool CheckAuthorization(RoleEnum currentRole)
    {
        return true;
    }
    protected abstract Task OnInitializedAsyncCallback();
    protected sealed override async Task OnInitializedAsync()
    {
        try
        {
            var results = await LoginService.ValidateLoginAsync(async () => await SessionStorage.GetAsync<string>("token"));
            if (!results) NavigationManager.NavigateTo("/", true);
    
            // is user allowed?
            if (!CheckAuthorization(State.User.Role)) NavigationManager.NavigateTo("/", true);
            
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