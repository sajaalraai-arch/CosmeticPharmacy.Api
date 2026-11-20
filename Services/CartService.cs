using CosmeticPharmacy.Api.Data;
using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Services
{
    public class CartService
    {
        public const decimal TaxRate = 0.05m;
        public const decimal ShippingFee = 10.00m;
        public const decimal CouponRate = 0.15m;
        public const string ValidCoupon = "rawanx2";

        // Returned by the methods below when the product or the cart item does not exist.
        public const string NotFoundError = "NotFound";

        private readonly CosmeticPharmacyProjectContext _db;

        public CartService(CosmeticPharmacyProjectContext db)
        {
            _db = db;
        }

        public async Task<Cart> GetOrCreateCartAsync(int customerId)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId, CreatedAt = DateTime.Now };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }
            return cart;
        }

        // The price the customer pays: the discount price when it is a real discount,
        // otherwise the normal price.
        public static decimal GetUnitPrice(Product product)
        {
            if (product.DiscountPrice != null && product.DiscountPrice > 0 && product.DiscountPrice < product.Price)
                return product.DiscountPrice.Value;

            return product.Price;
        }

        // Returns an error message when the product cannot be bought now, otherwise null.
        private static string? CheckProductAvailable(Product product)
        {
            if (!product.IsActive || product.ExpiryDate.Date < DateTime.Today)
                return $"Sorry, '{product.ProductName}' is not available.";

            return null;
        }

        private static bool IsCouponValid(string? coupon)
        {
            return string.Equals(coupon?.Trim(), ValidCoupon, StringComparison.OrdinalIgnoreCase);
        }

        public static CartSummaryDto BuildSummary(decimal subtotal, string? coupon)
        {
            return new CartSummaryDto
            {
                Subtotal = subtotal,
                Tax = Math.Round(subtotal * TaxRate, 2),
                Shipping = subtotal > 0 ? ShippingFee : 0m,
                Discount = IsCouponValid(coupon) ? Math.Round(subtotal * CouponRate, 2) : 0m
            };
        }

        // Only reads data. Prices are calculated from the current product prices.
        public async Task<CartDto> GetCartAsync(int customerId)
        {
            var cart = await GetOrCreateCartAsync(customerId);

            var cartItems = await _db.CartItems
                .AsNoTracking()
                .Include(ci => ci.Product)
                .ThenInclude(p => p.Category)
                .Where(ci => ci.CartId == cart.CartId)
                .OrderBy(ci => ci.ProductId)
                .ToListAsync();

            var items = new List<CartItemDto>();
            decimal subtotal = 0;
            int itemsCount = 0;

            foreach (var cartItem in cartItems)
            {
                decimal unitPrice = GetUnitPrice(cartItem.Product);
                decimal totalPrice = unitPrice * cartItem.Quantity;

                items.Add(new CartItemDto
                {
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.ProductName,
                    MainImageUrl = cartItem.Product.MainImageUrl,
                    CategoryName = cartItem.Product.Category?.CategoryName,
                    UnitPrice = unitPrice,
                    Quantity = cartItem.Quantity,
                    TotalPrice = totalPrice
                });

                subtotal += totalPrice;
                itemsCount += cartItem.Quantity;
            }

            var cartDto = new CartDto
            {
                Items = items,
                Coupon = cart.CouponCode,
                ItemsCount = itemsCount,
                Summary = BuildSummary(subtotal, cart.CouponCode)
            };

            if (IsCouponValid(cart.CouponCode))
            {
                cartDto.CouponApplied = true;
                cartDto.DiscountPercent = (int)(CouponRate * 100);
            }

            return cartDto;
        }

        // Saves the coupon only when it is valid, and returns the cart with a message for the user.
        public async Task<CartDto> ApplyCouponAsync(int customerId, string? code)
        {
            var cart = await GetOrCreateCartAsync(customerId);
            string message;

            if (string.IsNullOrWhiteSpace(code))
            {
                cart.CouponCode = null;
                message = "The discount code was removed.";
            }
            else if (IsCouponValid(code))
            {
                int percent = (int)(CouponRate * 100);
                cart.CouponCode = ValidCoupon;
                message = $"Discount code applied successfully! You got a {percent}% discount.";
            }
            else
            {
                cart.CouponCode = null;
                message = "Sorry, this discount code is invalid or has expired.";
            }

            await _db.SaveChangesAsync();

            var cartDto = await GetCartAsync(customerId);
            cartDto.CouponMessage = message;
            return cartDto;
        }

        public async Task<int> GetItemsCountAsync(int customerId)
        {
            return await _db.CartItems
                .Where(ci => ci.Cart.CustomerId == customerId)
                .SumAsync(ci => (int?)ci.Quantity) ?? 0;
        }

        // Returns null on success, NotFoundError when the product does not exist,
        // otherwise a message that explains why the product could not be added.
        public async Task<string?> AddToCartAsync(int customerId, int productId, int quantity)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null) return NotFoundError;

            string? availableError = CheckProductAvailable(product);
            if (availableError != null) return availableError;

            var cart = await GetOrCreateCartAsync(customerId);
            var item = await _db.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            int quantityInCart = item == null ? 0 : item.Quantity;
            int newQuantity = quantityInCart + quantity;
           

            if (newQuantity > 50)
                return "Sorry, you cannot order 50 or more items of the same product. Maximum allowed quantity is 50.";

            if (newQuantity > product.StockQuantity)
            {
                if (quantityInCart > 0)
                    return $"Sorry, only {product.StockQuantity} item(s) of '{product.ProductName}' are in stock, and you already have {quantityInCart} in your cart.";

                return $"Sorry, only {product.StockQuantity} item(s) of '{product.ProductName}' are in stock.";
            }

            decimal unitPrice = GetUnitPrice(product);

            if (item == null)
            {
                _db.CartItems.Add(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = newQuantity,
                    TotalPrice = unitPrice * newQuantity
                });
            }
            else
            {
                item.Quantity = newQuantity;
                item.TotalPrice = unitPrice * newQuantity;
            }

            await _db.SaveChangesAsync();
            return null;
        }

        // Returns null on success, NotFoundError when the item is not in the cart,
        // otherwise a message that explains why the quantity could not be changed.
        public async Task<string?> UpdateQuantityAsync(int customerId, int productId, string action)
        {
            var item = await _db.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Cart.CustomerId == customerId && ci.ProductId == productId);

            if (item == null) return NotFoundError;

            if (string.Equals(action, "plus", StringComparison.OrdinalIgnoreCase))
            {
                string? availableError = CheckProductAvailable(item.Product);
                if (availableError != null) return availableError;
                if (item.Quantity >= 50)
                    return "Sorry, you cannot order 50 or more items. Maximum allowed quantity is 50.";

                if (item.Quantity + 1 > item.Product.StockQuantity)
                    return $"Sorry, only {item.Product.StockQuantity} item(s) of '{item.Product.ProductName}' are in stock.";

                item.Quantity += 1;
            }
            else
            {
                item.Quantity -= 1;
            }

            if (item.Quantity <= 0)
                _db.CartItems.Remove(item);
            else
                item.TotalPrice = GetUnitPrice(item.Product) * item.Quantity;

            await _db.SaveChangesAsync();
            return null;
        }

        public async Task<bool> RemoveItemAsync(int customerId, int productId)
        {
            var item = await _db.CartItems
                .FirstOrDefaultAsync(ci => ci.Cart.CustomerId == customerId && ci.ProductId == productId);

            if (item == null) return false;

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ClearAsync(int customerId)
        {
            var items = await _db.CartItems.Where(ci => ci.Cart.CustomerId == customerId).ToListAsync();
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }

        // Everything happens inside one transaction: if anything fails, nothing is saved
        // (no order is created and no stock is deducted).
        public async Task<(Order? Order, string? Error)> PlaceOrderAsync(int customerId, PlaceOrderDto dto)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var cart = await _db.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null || cart.CartItems.Count == 0)
                    return (null, "Your cart is empty! Please add products before checking out.");

                // Copy the items now: after the cart items are deleted below, the cart's own list may change.
                var cartItems = new List<CartItem>(cart.CartItems);

                // 1) Check every product again with the current data from the database.
                decimal subtotal = 0;
                foreach (var cartItem in cartItems)
                {
                    string? availableError = CheckProductAvailable(cartItem.Product);
                    if (cartItem.Quantity >= 50)
                        return (null, $"Sorry, you cannot order 50 or more items of '{cartItem.Product.ProductName}'. Maximum allowed quantity is 49.");
                    if (availableError != null) return (null, availableError);

                    if (cartItem.Quantity > cartItem.Product.StockQuantity)
                        return (null, $"Sorry, only {cartItem.Product.StockQuantity} item(s) of '{cartItem.Product.ProductName}' are in stock. Please update your cart.");

                    subtotal += GetUnitPrice(cartItem.Product) * cartItem.Quantity;
                }

                var summary = BuildSummary(subtotal, cart.CouponCode);

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    ShippingAddress = dto.Address.Trim(),
                    City = dto.City.Trim(),
                    ContactPhone = dto.Phone.Trim(),
                    TotalPrice = summary.Total,
                    OrderStateId = 1,       // Pending
                    NewOrNot = true
                };

                foreach (var cartItem in cartItems)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        TotalPrice = GetUnitPrice(cartItem.Product) * cartItem.Quantity
                    });
                }

                order.Payments.Add(new Payment
                {
                    PaymentMethod = string.Equals(dto.PaymentMethod, "cod", StringComparison.OrdinalIgnoreCase)
                        ? "Cash On Delivery"
                        : "Credit Card",
                    PaymentState = "Pending"
                });

                // 2) Delete the cart items FIRST. If the same order is sent twice at the same time,
                // the second request finds the rows already deleted and fails here,
                // so the stock is never deducted twice and no duplicate order is created.
                _db.CartItems.RemoveRange(cartItems);
                await _db.SaveChangesAsync();

                // 3) Deduct the stock. The UPDATE only works when enough stock is left,
                // so two customers can never buy the last items together.
                foreach (var cartItem in cartItems)
                {
                    int updatedRows = await _db.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE Products SET StockQuantity = StockQuantity - {cartItem.Quantity} WHERE ProductId = {cartItem.ProductId} AND StockQuantity >= {cartItem.Quantity}");

                    if (updatedRows == 0)
                    {
                        await transaction.RollbackAsync();
                        return (null, $"Sorry, the stock of '{cartItem.Product.ProductName}' has just changed. Please check your cart and try again.");
                    }
                }

                // 4) Save the order and clear the coupon.
                _db.Orders.Add(order);
                cart.CouponCode = null;
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                return (order, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return (null, "Your cart was already processed or has changed. Please check your cart and try again.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
