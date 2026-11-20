using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Models
{
    public class CartItem
    {
        public int CartId { get; set; }
        public virtual Cart Cart { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Range(1, 50)]
        public int Quantity { get; set; }                          // Annotation

        public decimal TotalPrice { get; set; }                    // Fluent (decimal(18,2))
    }
}
