using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Helpers;
using CosmeticPharmacy.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmeticPharmacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        private int CurrentCustomerId => User.GetCustomerId() ?? 0;

        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            return await _cartService.GetCartAsync(CurrentCustomerId);
        }

        // Applies (or removes) a discount code. The returned cart contains the message to show to the user.
        [HttpPost("coupon")]
        public async Task<ActionResult<CartDto>> ApplyCoupon([FromBody] ApplyCouponDto dto)
        {
            return await _cartService.ApplyCouponAsync(CurrentCustomerId, dto.Code);
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount()
        {
            return await _cartService.GetItemsCountAsync(CurrentCustomerId);
        }

        [HttpGet("checkout")]
        public async Task<ActionResult<CartDto>> GetCheckout()
        {
            var cart = await _cartService.GetCartAsync(CurrentCustomerId);

            if (cart.Items.Count == 0)
                return BadRequest(new MessageDto { Message = "Your cart is empty! Please add products before checking out." });

            return cart;
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            string? error = await _cartService.AddToCartAsync(CurrentCustomerId, dto.ProductId, dto.Quantity);

            if (error == CartService.NotFoundError)
                return NotFound(new MessageDto { Message = "Product not found." });

            if (error != null)
                return Conflict(new MessageDto { Message = error });

            return Ok(new { success = true, itemsCount = await _cartService.GetItemsCountAsync(CurrentCustomerId) });
        }

        [HttpPut("items/{productId:int:min(1)}")]
        public async Task<IActionResult> UpdateQuantity([FromRoute] int productId, [FromQuery] string action = "plus")
        {
            if (!string.Equals(action, "plus", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(action, "minus", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new MessageDto { Message = "Invalid action. Use plus or minus." });

            string? error = await _cartService.UpdateQuantityAsync(CurrentCustomerId, productId, action);

            if (error == CartService.NotFoundError)
                return NotFound(new MessageDto { Message = "This product is not in your cart." });

            if (error != null)
                return Conflict(new MessageDto { Message = error });

            return NoContent();
        }

        [HttpDelete("items/{productId:int:min(1)}")]
        public async Task<IActionResult> RemoveItem([FromRoute] int productId)
        {
            bool found = await _cartService.RemoveItemAsync(CurrentCustomerId, productId);
            return found ? NoContent() : NotFound();
        }

        [HttpDelete]
        public async Task<IActionResult> Clear()
        {
            await _cartService.ClearAsync(CurrentCustomerId);
            return NoContent();
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto dto)
        {
            var (order, error) = await _cartService.PlaceOrderAsync(CurrentCustomerId, dto);

            if (order == null) return BadRequest(new MessageDto { Message = error ?? "Unable to place the order." });

            return Ok(new
            {
                orderId = order.OrderId,
                message = "Your purchase has been completed successfully! Thank you for shopping with us."
            });
        }
    }
}
