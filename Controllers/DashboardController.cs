using CosmeticPharmacy.Api.Data;
using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly CosmeticPharmacyProjectContext _db;
        private readonly ProductService _products;

        public DashboardController(CosmeticPharmacyProjectContext db, ProductService products)
        {
            _db = db;
            _products = products;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> Index()
        {
            var dto = new DashboardDto
            {
                TotalUsers = await _db.Customers.CountAsync(),
                TotalOrders = await _db.Orders.CountAsync(),
                InventoryValue = await _db.Products.SumAsync(p => (decimal?)(p.Price * p.StockQuantity)) ?? 0m,
                LatestProducts = await _products.GetLatestAsync(5)
            };

            return Ok(dto);
        }
    }
}
