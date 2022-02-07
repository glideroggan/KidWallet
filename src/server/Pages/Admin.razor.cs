namespace server.Pages;

/*
 * [TODO] context menu to create achievements
 * [TODO] Context menu to create new users
 * [TODO] add validation for Role on this page
 *      after it is tested, remove it from the navmenu
 */

public class AdminBase : PageBase
{
    protected override Task OnInitializedAsyncCallback()
    {
        return Task.CompletedTask;
    }
}