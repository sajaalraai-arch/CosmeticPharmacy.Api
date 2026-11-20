using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmeticPharmacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _products;

        public ProductsController(ProductService products)
        {
            _products = products;
        }

       
        [HttpGet]
        public async Task<ActionResult<ProductsIndexDto>> GetAll(
            [FromQuery] string? search, [FromQuery] string? filter,
            [FromQuery] int? categoryId, [FromQuery] string? categoryName, [FromQuery] bool hideExpired = false)
        {
            var result = await _products.GetAllAsync(search, filter, categoryId, categoryName, hideExpired);
            if (result == null) return NotFound();
            return result;
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<ProductDetailsDto>> GetById([FromRoute] int id)
        {
            var product = await _products.GetByIdAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductFormDto model)
        {
            if (model.ImageFile is not { Length: > 0 })
                ModelState.AddModelError(nameof(model.ImageFile), "Please upload an image.");

            await CheckReferencesAsync(model);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            int id = await _products.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id }, new { productId = id });
        }

        [HttpPut("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] ProductFormDto model)
        {
            if (await _products.GetByIdAsync(id) == null) return NotFound();

            await CheckReferencesAsync(model, id);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            await _products.UpdateAsync(id, model);
            return Ok(new { productId = id });
        }

        /// <summary>
        /// الحذف بالترتيب: يتحقق أولاً هل المنتج في طلبات سابقة.
        /// - في طلبات ولم يوافق المستخدم بعد: 409 مع canDeactivate=true ليُسأل عن التعطيل.
        /// - DELETE api/products/5?choice=deactivate (وافق على التعطيل): يعطّل المنتج ويحذفه من السلات.
        /// - ليس في طلبات: يُحذف (وبنوده في السلات تُحذف تلقائياً).
        /// </summary>
        [HttpDelete("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] string? choice = null)
        {
            bool deactivate = string.Equals(choice, "deactivate", StringComparison.OrdinalIgnoreCase);

            if (!deactivate && await _products.IsInOrdersAsync(id))
            {
                return Conflict(new MessageDto
                {
                    Message = "This product cannot be deleted because it is linked to orders. Do you want to make it inactive instead?",
                    CanDeactivate = true
                });
            }

            if (deactivate)
            {
                if (!await _products.DeactivateAsync(id)) return NotFound();
                return Ok(new MessageDto { Message = "The product was deactivated." });
            }

            if (!await _products.DeleteAsync(id)) return NotFound();
            return NoContent();
        }

        /// <summary>التحققات التي تحتاج قاعدة البيانات (بقية التحققات في ProductFormDto).</summary>
        private async Task CheckReferencesAsync(ProductFormDto model, int currentId = 0)
        {
            if (await _products.BarcodeExistsAsync(model.Barcode, currentId))
                ModelState.AddModelError(nameof(model.Barcode), "Sorry, this barcode is already registered!");

            if (!await _products.CategoryExistsAsync(model.CategoryId))
                ModelState.AddModelError(nameof(model.CategoryId), "The selected category does not exist.");
        }
    }
}
