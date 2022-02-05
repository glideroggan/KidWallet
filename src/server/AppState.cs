using server.Data.User;

public class AppState
{
    public event Action OnChange = null!;
    
    public UserModel? User { get; internal set; }
    public decimal Balance { get; internal set; }


    public void NotifyStateChanged() => OnChange?.Invoke();
}