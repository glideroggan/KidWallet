namespace server.Data.User;

public enum RoleEnum
{
    None,
    Parent,
    Child,
    Admin
}

public record UserModel(int Id, string Name, string PasswordHash, string? ImageUrl, RoleEnum Role, int Balance, int banked);
