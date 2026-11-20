using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Dtos
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? MainImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartSummaryDto
    {
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Discount { get; set; }
        public decimal Total => (Subtotal + Tax + Shipping) - Discount;
    }

    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public CartSummaryDto Summary { get; set; } = new();
        public string? Coupon { get; set; }
        public int ItemsCount { get; set; }

        // Coupon information shown by Angular
        public bool CouponApplied { get; set; }
        public int DiscountPercent { get; set; }
        public string? CouponMessage { get; set; }
    }

    public class ApplyCouponDto
    {
        [MaxLength(50, ErrorMessage = "The discount code is too long.")]
        public string? Code { get; set; }
    }

    public class AddToCartDto
    {
        public int ProductId { get; set; }

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50.")]
        public int Quantity { get; set; }
    }
}
