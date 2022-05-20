using server.Data.User;

namespace server.Pages;

/*
 * [TODO] information about the family
 *      number of users, parents, children
 *      number of tasks
 * [TODO] Context menu to create/edit users
 * [DONE] context menu to create achievements
 * [DONE] add validation for Role on this page
 *      after it is tested, remove it from the navmenu
 */

public class AdminBase : PageBase
{
    protected override bool CheckAuthorization(RoleEnum currentRole) => currentRole == RoleEnum.Parent;

    protected override Task OnInitializedAsyncCallback()
    {
        return Task.CompletedTask;
    }
}