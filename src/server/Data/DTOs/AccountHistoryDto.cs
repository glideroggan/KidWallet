using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Data.DTOs;

public enum AccountTypeEnum
{
    None,
    Spending,
    Savings
}

[Table(name: "AccountHistories")]
public class AccountHistoryDto
{
    [Key]
    public int TransactionDataId { get; set; }
    [Required]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public UserDto User { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    [Required]
    public int SourceAccountId { get; set; }
    [Required]
    public AccountTypeEnum SourceAccountType {get; set; }
    [Required]
    public int DestAccountId { get; set; }
    [Required]
    public AccountTypeEnum DestAccountType { get; set; }
    [Range(1, double.MaxValue)]
    public int Funds { get; set; }
    [MaxLength(255)]
    public string Description { get; set; }
}