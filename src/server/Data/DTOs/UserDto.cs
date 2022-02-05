using server.Data.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace server.Data.DTOs;
public static class UserExtensions
{
    public static UserModel ToModel(this UserDto d, int banked) => 
        new UserModel(d.UserId, d.Name, d.Password, d.ProfileImg, d.Role, d.SpendingAccount?.Balance ?? 0, banked);
    
}

[Table("Users")]
public class UserDto
{
    [Key]
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? ProfileImg { get; set; }
    public User.RoleEnum Role { get; set; }

    [ForeignKey("SpendingAccountId")]
    public SpendingAccountDto? SpendingAccount { get; set; }
    public int? ParentId { get; set; }
    public string? Password { get; set; }

    public List<NotificationDto> Notifications { get; set; }
}
