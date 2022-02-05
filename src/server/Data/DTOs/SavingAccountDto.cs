using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace server.Data.DTOs;

public class SavingAccountDto
{
    [Key]
    public int SavingsAccountEntityId { get; set; }
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public UserDto User { get; set; }
    public int DepositFunds { get; set; }
    public int CalculatedFunds { get; set; }
    public DateTime ReleaseDate { get; set; }
}
