using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticPharmacy.Api.Models
{
    /// <summary>
    /// تصنيف المنتجات. (Many-to-One مع المنتجات)
    /// Data Annotations: CategoryName.   Fluent API: CategoryImage + الفهرس الفريد.
    /// </summary>
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }                        // PK by convention (ClassNameId)

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = null!;          // Data Annotation

        [Column("Image")]
        public string? CategoryImage { get; set; }                

        public bool IsActive { get; set; } = true;                 // convention

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
