using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Dtos
{
    public class PaymentDto
    {
        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = null!;

        public string? PaymentState { get; set; } = "Pending";
    }
}
