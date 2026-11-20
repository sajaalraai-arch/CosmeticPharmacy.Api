using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Dtos
{
    public class OrderListItemDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int OrderStateId { get; set; }
        public string? StateName { get; set; }
    }

    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TodayOrders { get; set; }
    }

    public class OrderStateDto
    {
        public int OrderStateId { get; set; }
        public string NameState { get; set; } = "";
    }

    public class OrdersIndexDto
    {
        public OrderStatsDto Stats { get; set; } = new();
        public List<OrderListItemDto> Orders { get; set; } = new();
        public List<OrderStateDto> States { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? MainImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderDetailsDto : OrderListItemDto
    {
        public string? CustomerEmail { get; set; }
        public string ContactPhone { get; set; } = "";
        public string ShippingAddress { get; set; } = "";
        public string City { get; set; } = "";
        public string? PaymentMethod { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class UpdateOrderStatusDto
    {
        public int StatusId { get; set; }
    }


    public class PlaceOrderDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = null!;
        [Required, MaxLength(50)]
        public string Phone { get; set; } = null!;
        [Required]
        public string Address { get; set; } = null!;

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = null!;



    }
}
