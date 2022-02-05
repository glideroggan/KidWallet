using Microsoft.AspNetCore.Components;
using server.Data;
using server.Services;

namespace server.Components
{
    public class LoginModuleBase : ComponentBase
    {
#pragma warning disable CS8618
	    [Inject] protected LoginService LoginService { get; private set; }
#pragma warning restore CS8618


	    protected IList<LoginUserModel>? Users { get; private set; }

		protected override async Task OnInitializedAsync()
		{
            Users = await LoginService.GetUsers();
		}

		[Parameter] public EventCallback Successful { get; set; }

		protected async Task Login(string username)
		{
			await Successful.InvokeAsync();
			
		}
	}
}
