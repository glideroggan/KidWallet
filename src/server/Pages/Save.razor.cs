using Microsoft.AspNetCore.Components;
using server.Services;

namespace server.Pages;

public class SaveBase : PageBase
{
    [Parameter] public string? Name { get; set; }
    [Inject] private UserService UserService { get; set; }
    
    protected int MaxMoney { get; set; }
    protected bool Done { get; set; } = false;
    protected override async Task OnInitializedAsyncCallback()
    {
        var money = await UserService.GetUserAsync(Name);
        MaxMoney = money.Balance;
        Done = true;
        StateHasChanged();
    }
}