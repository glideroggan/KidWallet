﻿using Microsoft.AspNetCore.Components;
using server.Data.User;

namespace server.Components;

/*
 * [TODO] check if name right under image is better
 * [TODO] adjust the numbers under the icons with a bit of gap
 */

public class UserCardBase : ComponentBase
{
    [Inject] public AppState State { get; set; } = null!;
    [Inject] protected NavigationManager Nav { get; set; }
    [Parameter] public UserModel Model { get; set; } = null!;
    

    protected bool CanTransferMoney() => State.User?.Role == RoleEnum.Parent && Model.Role == RoleEnum.Child;

    // protected override Task OnParametersSetAsync()
    // {
    //     StateHasChanged();
    //     return base.OnParametersSetAsync();
    // }

    protected Task PayingOrTransferring(string name)
    {
        switch (State.User.Role)
        {
            // TODO: fix the pay page, so that children can pay parents
            case RoleEnum.Child when Model.Id == State.User.Id:
                Nav.NavigateTo("pay");
                break;
            case RoleEnum.Parent when Model.Role == RoleEnum.Child:
                Nav.NavigateTo($"transfer/{name}");
                break;
        }
        return Task.CompletedTask;
    }

    protected Task SaveMoney(string name)
    {
        Nav.NavigateTo($"save/{name}");
        return Task.CompletedTask;
    }
}
