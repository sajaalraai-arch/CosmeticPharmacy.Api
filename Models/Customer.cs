using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticPharmacy.Api.Models
{
  
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;                 // + Unique index (Fluent)

        [Required]
        public string PasswordHash { get; set; } = null!;

             
        public string? UserRole { get; set; } = "Customer";        // Fluent (HasMaxLength 20)
        public string? UserStatus { get; set; } = "Active";        // Fluent (HasMaxLength 20)

        public DateTime? CreatedAt { get; set; }                   // Fluent (datetime + default GETDATE())
        [Column(TypeName ="date")]
        public DateTime? LastLogin { get; set; }                   // Fluent (datetime)

       

        // One-to-One مع السلة
        public virtual Cart? Cart { get; set; }
        // One-to-Many
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
