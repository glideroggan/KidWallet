using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using server.Services;
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace server.Data.DTOs;

public enum MessageStatusEnum
{
    None, Unread, Approved, Denied
}


[Table(name: "Notifications")]
public class NotificationDto
{
    [Key]
    public int NotificationId { get; set; }
    
    public string Message { get; set; } = null!;
    public MessageStatusEnum Status { get; set; }
    
    public MessageType MessageType { get; set; }

    public int? IdentifierId { get; set; }
    
    public NotificationTargetEnum Target { get; set; }

    [StringLength(maximumLength:20)]
    public string? TargetName { get; set; }

    [ForeignKey("UserId")]
    public UserDto Sender { get; set; }
}