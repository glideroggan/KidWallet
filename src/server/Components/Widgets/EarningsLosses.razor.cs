using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using server.Data.User;
using server.Services;

namespace server.Components.Widgets;

/*
 * [DONE] - Try to make a template for widgets, as there are some duplicate code
 */

public class EarningsLossesBase : ComponentBase
{
    [Inject] private AccountService AccountService { get; set; } = null!;

    [Parameter] public RoleEnum Role { get; set; }

    protected List<(string? Name, int Earnings)> children = new();
    
    protected override async Task OnInitializedAsync()
    {
        children = await AccountService.GetEarningsAsync(2);
    }
    
}