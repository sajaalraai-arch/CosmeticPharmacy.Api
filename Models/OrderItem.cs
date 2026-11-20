using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticPharmacy.Api.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Range(1, 500)]
        public int Quantity { get; set; }                      

        public decimal TotalPrice { get; set; }                  
    }
}
