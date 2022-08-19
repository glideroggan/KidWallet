using server.Data.User;
using server.Services;

public class AppState
{
    private readonly UserService _userService;

    public AppState(UserService userService)
    {
        _userService = userService;
    }
    public event Action OnChange = null!;
    
    public UserModel? User { get; internal set; }

    public async Task NotifyStateChanged()
    {
        if (User != null)
            User = await _userService.GetUserAsync(User.Name);
        OnChange?.Invoke();   
    }
}