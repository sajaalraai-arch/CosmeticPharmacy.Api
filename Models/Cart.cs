using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Models
{
   
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public int CustomerId { get; set; }                        // FK (Unique) - Fluent API
        public virtual Customer Customer { get; set; } = null!;

        [MaxLength(50)]
        public string? CouponCode { get; set; }                    // Annotation

        public DateTime CreatedAt { get; set; }                    // Fluent

  
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
} 
