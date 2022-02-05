using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable CS8618

namespace server.Data.DTOs
{
    public class SpendingAccountDto
    {
        [Key]
        public int SpendingAccountId { get; set; }
        public int Balance { get; set; }

        [InverseProperty("OwnerAccount")]
        public List<ReserveDto> ReservedAmounts { get; set; }
        
        [InverseProperty("DestAccount")]
        public List<ReserveDto> ReservedIncomingAmounts { get; set; }
    }
}
