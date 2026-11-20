using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CosmeticPharmacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly CategoryService _categories;
        private readonly ProductService _products;

        public HomeController(CategoryService categories, ProductService products)
        {
            _categories = categories;
            _products = products;
        }

        [HttpGet]
        public async Task<ActionResult<HomeDto>> Index()
        {
            var categories = await _categories.GetAllAsync(activeOnly: true);

            var topProducts = await _products.GetTopActiveAsync(5);

            return Ok(new HomeDto { Categories = categories, TopProducts = topProducts });
        }
    }
}
