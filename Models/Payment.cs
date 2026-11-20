using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int? OrderId { get; set; }                         
        public virtual Order? Order { get; set; }

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = null!;       

        public string? PaymentState { get; set; } = "Pending";     
    }
}
