using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using server.Data.User;
using server.Services;

namespace server.Components.Widgets;

/*
 * [TODO] - Try to make a template for widgets, as there are some duplicate code
 */

public class EarningsLossesBase : ComponentBase
{
    [Inject] private AccountService AccountService { get; set; }
    [Inject] private AppState State { get; set; }
    
    [Parameter] public RoleEnum Role { get; set; }

    protected List<(string? Name, int Earnings)> children = new();
    
    protected override async Task OnInitializedAsync()
    {
        // TODO: if not logged in, throw error? Show something else?
        children = await AccountService.GetEarningsAsync(2);
    }
    
}