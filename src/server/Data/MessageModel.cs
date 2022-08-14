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
        return new MessageModel(n.NotificationId, n.Message, n.MessageType, n.Status, n.Sender.ToModel(0), n.TargetName,
            n.IdentifierId);
        // return new MessageModel
        // {
        //     Id = n.NotificationId,
        //     Message = n.Message,
        //     IdentifierId = n.IdentifierId,
        //     Sender = n.Sender.ToModel(0),
        //     Type = n.MessageType,
        //     Status = n.Status
        // };
    }
}

public record MessageModel(int Id, string Message, MessageType Type, MessageStatusEnum Status, UserModel Sender,
    string? ToName, int? IdentifierId);


// public class MessageModel
// {
//     public int Id { get; set; }
//     public string Message { get; set; }
//     public MessageType Type { get; set; }
//     public int? IdentifierId { get; set; }
//     public MessageStatusEnum Status { get;set; }
//     public UserModel Sender { get; set; }
//     public UserModel? To { get; set; }
// }