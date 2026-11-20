using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticPharmacy.Api.Models
{
    [Table("OrderStates")]
    public class OrderState
    {
        public int OrderStateId { get; set; }                    

        
        [Column("StateName")]
        [MaxLength(20)]
        public string NameState { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
