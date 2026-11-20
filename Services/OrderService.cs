using CosmeticPharmacy.Api.Data;
using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Services
{
    public class OrderService
    {
        private const int CompletedStateId = 3;

        private readonly CosmeticPharmacyProjectContext _db;

        public OrderService(CosmeticPharmacyProjectContext db)
        {
            _db = db;
        }

        public static IQueryable<OrderListItemDto> ProjectToList(IQueryable<Order> query)
        {
            return query.Select(o => new OrderListItemDto
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.FullName,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                OrderStateId = o.OrderStateId,
                StateName = o.OrderState.NameState
            });
        }

        public async Task<OrdersIndexDto> GetAllAsync(
            string? searchString, int? statusId, DateTime? fromDate, DateTime? toDate, string? sortOrder, int? customerId)
        {
            DateTime today = DateTime.Today;

            var stats = new OrderStatsDto
            {
                TotalOrders = await _db.Orders.CountAsync(),
                PendingOrders = await _db.Orders.CountAsync(o => o.OrderStateId == 1),
                TotalRevenue = await _db.Orders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0m,
                TodayOrders = await _db.Orders.CountAsync(o => o.OrderDate >= today)
            };

            var query = _db.Orders.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string s = searchString.Trim();
                query = query.Where(o => o.OrderId.ToString() == s || o.Customer.FullName.Contains(s));
            }
            if (statusId.HasValue) query = query.Where(o => o.OrderStateId == statusId.Value);
            if (customerId.HasValue) query = query.Where(o => o.CustomerId == customerId.Value);
            if (fromDate.HasValue) query = query.Where(o => o.OrderDate >= fromDate.Value);
            if (toDate.HasValue)
            {
                DateTime toExclusive = toDate.Value.Date.AddDays(1);
                query = query.Where(o => o.OrderDate < toExclusive);
            }

            query = sortOrder switch
            {
                "name_asc" => query.OrderBy(o => o.Customer.FullName),
                "name_desc" => query.OrderByDescending(o => o.Customer.FullName),
                "id_asc" => query.OrderBy(o => o.OrderId),
                "id_desc" => query.OrderByDescending(o => o.OrderId),
                "price_asc" => query.OrderBy(o => o.TotalPrice),
                "price_desc" => query.OrderByDescending(o => o.TotalPrice),
                "date_asc" => query.OrderBy(o => o.OrderDate),
                _ => query.OrderByDescending(o => o.OrderDate)
            };

            return new OrdersIndexDto
            {
                Stats = stats,
                Orders = await ProjectToList(query).ToListAsync(),
                States = await _db.OrderStates.AsNoTracking()
                    .OrderBy(s => s.OrderStateId)
                    .Select(s => new OrderStateDto { OrderStateId = s.OrderStateId, NameState = s.NameState })
                    .ToListAsync()
            };
        }

      
     
        public async Task<OrderDetailsDto?> GetByIdAsync(int id)
        {
            var order = await _db.Orders.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderState)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return null;

            return new OrderDetailsDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.FullName,
                CustomerEmail = order.Customer.Email,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                OrderStateId = order.OrderStateId,
                StateName = order.OrderState.NameState,
                ContactPhone = order.ContactPhone,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                PaymentMethod = order.Payments.OrderBy(p => p.PaymentId).Select(p => p.PaymentMethod).FirstOrDefault(),
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.ProductName,
                    MainImageUrl = oi.Product.MainImageUrl,
                    CategoryId = oi.Product.CategoryId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.Quantity == 0 ? 0 : oi.TotalPrice / oi.Quantity,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };
        }

        public async Task<string?> UpdateStatusAsync(int id, int statusId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return "NotFound";

            if (!await _db.OrderStates.AnyAsync(s => s.OrderStateId == statusId))
                return "Invalid status.";

            order.OrderStateId = statusId;
            await _db.SaveChangesAsync();
            return null;
        }




        public async Task<string?> DeleteOrderAsync(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return "NotFound";

            if (order.OrderStateId == CompletedStateId)
                return "Error: Cannot delete completed orders for safety audits.";

            _db.OrderItems.RemoveRange(order.OrderItems);
            _db.Payments.RemoveRange(order.Payments);
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            return null;
        }
    }
}
