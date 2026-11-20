using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticPharmacy.Api.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; }                           

        public int? ProductId { get; set; }                       
        public virtual Product? Product { get; set; }

        [Required]
        [Column("ImagePath")]
        public string ImageUrl { get; set; } = null!;            
    }
}
