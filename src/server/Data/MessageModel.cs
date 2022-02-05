// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
using server.Data.DTOs;
using server.Data.User;
using server.Services;

namespace server.Data.Message;
#pragma warning disable CS8618

public static class MessageExtensions
{
    public static MessageModel ToModel(this NotificationDto n)
    {
        return new MessageModel
        {
            Id = n.NotificationId,
            Message = n.Message,
            IdentifierId = n.IdentifierId,
            Sender = n.Sender.ToModel(0),
            Type = n.MessageType,
            Status = n.Status
        };
    }
}



public class MessageModel
{
    public int Id { get; set; }
    public string Message { get; set; }
    public MessageType Type { get; set; }
    public int? IdentifierId { get; set; }
    public MessageStatusEnum Status { get;set; }
    public UserModel Sender { get; set; }
}