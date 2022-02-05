using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Data.DTOs;

[Table(name: "DoneTasks")]
public class StatDto
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key, Column(Order = 1)]
    public int TaskId { get; set; }
    public string? Description { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key, Column(Order = 3)]
    public int UserId { get; set; }
    
    public int Count { get; set; }

}
