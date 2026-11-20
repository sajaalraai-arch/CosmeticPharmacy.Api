using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticPharmacy.Api.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }                        

        [Required, MaxLength(30)]
        public string ProductName { get; set; } = null!;         

        [MaxLength(100)]
        public string? ProductSlug { get; set; }                 

        [MaxLength(30)]
        public string? Barcode { get; set; }                     

        public string? Brand { get; set; }                       

        [Required, MaxLength(30)]
        public string TargetType { get; set; } = null!;           

        [Required, MaxLength(20)]
        public string Size { get; set; } = null!;                

        [Required, MaxLength(30)]
        public string CountryOfOrigin { get; set; } = null!;     

        public DateTime ExpiryDate { get; set; }             

        [Required]
        [MaxLength(300)]
        public string ProductDescription { get; set; } = null!;    

        [Required]
      
        public string Ingredients { get; set; } = null!;           

        [Required]
        public string UsageInstructions { get; set; } = null!;     

        [Column(TypeName = "decimal(10,2)")]
        [Range(1, 1000)]
        public decimal Price { get; set; }
        [Range(0.99, 999)]
        public decimal? DiscountPrice { get; set; }               

        [Range(0, 250 )]
        public int StockQuantity { get; set; }

        [Required]
        [Column("MainImagePath")]
        public string MainImageUrl { get; set; } = null!;     

        public bool IsActive { get; set; } = true;              

        public int? CategoryId { get; set; }                      
        public virtual Category? Category { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
   
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
