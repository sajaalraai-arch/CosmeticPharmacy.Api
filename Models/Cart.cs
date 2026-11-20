using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Models
{
    /// <summary>
    /// السلة: علاقة One-to-One مع العميل. (كانت جدول Baskets بسطر لكل منتج، أصبحت السلة + CartItems).
    /// </summary>
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public int CustomerId { get; set; }                        // FK (Unique) - Fluent API
        public virtual Customer Customer { get; set; } = null!;

        [MaxLength(50)]
        public string? CouponCode { get; set; }                    // Annotation

        public DateTime CreatedAt { get; set; }                    // Fluent

        // Many-to-Many مع المنتجات عبر CartItem (كيان الربط مع بيانات إضافية)
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
} 
