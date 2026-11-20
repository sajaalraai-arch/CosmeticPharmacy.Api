using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Models
{
    
    public class Comment
    {
        public int Id { get; set; }

        [Required, MaxLength(1000)]
        public string Content { get; set; } = null!;
        [Range (1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; }                    // Fluent (datetime + default GETDATE())

        public int ProductId { get; set; }                         // FK - Product 1 : N Comments
        public virtual Product Product { get; set; } = null!;

        public int CustomerId { get; set; }                        // FK - Customer 1 : N Comments
        public virtual Customer Customer { get; set; } = null!;
    }
}
