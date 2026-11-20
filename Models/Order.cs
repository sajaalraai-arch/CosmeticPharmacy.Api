using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticPharmacy.Api.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }                     

        public int CustomerId { get; set; }                   
        public virtual Customer Customer { get; set; } = null!;

        public DateTime OrderDate { get; set; }                  

        [Required]
        public string ShippingAddress { get; set; } = null!;      

        [Required, MaxLength(100)]
        public string City { get; set; } = null!;                 

        [Required, MaxLength(50)]
        public string ContactPhone { get; set; } = null!;        

        public decimal TotalPrice { get; set; }                   

        public int OrderStateId { get; set; }                   
        public virtual OrderState OrderState { get; set; } = null!;

        public bool? NewOrNot { get; set; } = true;              

      
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
