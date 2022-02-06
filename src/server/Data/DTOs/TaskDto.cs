using server.Data.Task;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using server.Services;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace server.Data.DTOs;

public static class TaskDtoExtensions
{
    public static TaskModel ToModel(this TaskDto t)
    {
        return new TaskModel
        {
            Created = t.Created,
            Description = t.Description,
            Id = t.TaskId,
            ImageUrl = t.ImgUrl,
            NotBefore = t.NotBefore,
            Payout = t.Payout,
            Status = t.Status,
            User = t.User?.ToModel(0),
            TargetUser = t.TargetUser?.ToModel(0),
            DayInTheWeek = DayAndWeekHelper.GetDays((uint)t.DayInTheWeek)
        };
    }
}

public class TaskDto
{
    [Key]
    public int TaskId { get; set; }
    public StatusEnum Status { get; set; }
    
    public int? UserId { get; set; }
    [ForeignKey("UserId")]
    public UserDto? User { get;set; }
    
    public int? SpecificUserId { get; set; }
    [ForeignKey("SpecificUserId")]
    public UserDto? TargetUser { get; set; }
    public string? Description { get; set; }
    public string? ImgUrl { get; set; }
    public int Payout { get; set; }
    public DateTime NotBefore { get; set; }
    /* Encode 7 bits in an int, being able to be combine
        * 1  00000001 Monday
        * 2  00000010 Thuesday
        * 4  00000100 Wednesday
        * 8  00001000 Thursday
        * 16 00010000 Friday
        * 32 00100000 Saturday
        * 64 01000000 Sunday
        * Monday & Thuesday & Wednesday 00000111 = 1*1 + 1*2 + 1*4 = 7
        * Monday 00000001 = 1*1 = 1
        */
    
    public long DayInTheWeek { get; set; }
    public bool Week { get; set; }
    public int EveryOtherWeek { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    // do not clone task if once
    public bool Once { get; set; }

}
