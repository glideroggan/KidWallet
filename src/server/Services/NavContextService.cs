using server.Data.User;

namespace server.Services
{
    public class NavContextService
    {
        private readonly Dictionary<string, (string Icon, string Url, RoleEnum ValidRole)[]> _subPages = new();
        private readonly AppState _state;

        public NavContextService(AppState state)
        {
            _state = state;
            /* NOTE:
             * Feels like we need to register the pages already here, as the pages will be created first when used
             * and location changed event will happen before
             */
            RegisterPage("tasks", ("assets/bootstrap-icons/bootstrap-icons.svg#plus-circle-fill", "/CreateTask", RoleEnum.Parent));
            // TODO: put in a more suitable icon
            RegisterPage("admin", ("assets/bootstrap-icons/bootstrap-icons.svg#plus-circle-fill", "/achievements", RoleEnum.Parent));
        }
        internal List<(string Icon, string Url)>? GetMenuItems(string page)
        {
            if (!_subPages.ContainsKey(page)) return null;
            
            return _subPages[page]
                .Where(p => p.ValidRole == _state.User.Role)
                .Select(p => (p.Icon, p.Url))
                .ToList();
        }

        private void RegisterPage(string mainPage, params (string name, string url, RoleEnum ValidRole)[] pages)
        {
            this._subPages.Add(mainPage, pages);
        }
    }
}
