using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using server.Data;
using server.Services;

namespace server.Components;

public class LoginCardBase : ComponentBase
{
#pragma warning disable CS8618
	[Inject] protected LoginService LoginService { get; private set; }
	[Inject] private ProtectedSessionStorage SessionStorage { get; set; }

	[Parameter] public LoginUserModel User { get; set; }
    [Parameter] public EventCallback<string> OnSuccess { get; set; }
    [CascadingParameter] public Action<string> NotificationCallback { get; set; }
#pragma warning restore CS8618

    protected string? Password;

    protected bool PasswordNeeded;

    protected async Task Login()
	{
        PasswordNeeded = User.Role == Data.User.RoleEnum.Parent;
        if (!PasswordNeeded)
        {
	        await ValidateUser();
        }
	}

    private async Task ValidateUser()
    {
	    var results = await LoginService.ValidateAsync(User.Username, Password);
	    if (results.Ok)
	    {
		    await SessionStorage.SetAsync("token", results.Token);
		    await OnSuccess.InvokeAsync(User.Username);
	    }
	    else
	    {
		    // TODO: we need to send message to notify
		    NotificationCallback("Nope");
	    }
    }


    protected async Task LoginButton()
    {
	    await ValidateUser();
	    // var results = await LoginService.ValidateAsync(User.Username, Password);
	    // if (results.Ok)
	    // {
		   //  await SessionStorage.SetAsync("token", results.Token);
		   //  await OnSuccess.InvokeAsync(User.Username);
	    // }
    }

    protected async void KeyboardHandler(KeyboardEventArgs args)
	{
        if (args.Code == "Enter")
        {
	        await LoginButton();
        }
	}
}