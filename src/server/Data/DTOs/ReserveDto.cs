using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable CS8618

namespace server.Data.DTOs;

[Table(name: "Reserves")]
public class ReserveDto
{
    [Key]
    public int ReserveId { get; set; }
    
    public int Amount { get; set; }

    public SpendingAccountDto OwnerAccount { get; set; }
    public SpendingAccountDto DestAccount { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
}