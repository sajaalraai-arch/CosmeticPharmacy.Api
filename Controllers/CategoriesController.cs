using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmeticPharmacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private const string NameExistsMessage = "A category with this name already exists.";

        private readonly CategoryService _categories;

        public CategoriesController(CategoryService categories)
        {
            _categories = categories;
        }

        
        [HttpGet]
        public async Task<ActionResult<List<CategoryListItemDto>>> GetAll()
        {
            return await _categories.GetAllAsync();
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<CategoryListItemDto>> GetById([FromRoute] int id)
        {
            var category = await _categories.GetByIdAsync(id);
            if (category == null) return NotFound();
            return category;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CategoryFormDto model)
        {
            if (await _categories.NameExistsAsync(model.CategoryName))
                return Conflict(new MessageDto { Message = NameExistsMessage });

            int id = await _categories.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id }, await _categories.GetByIdAsync(id));
        }

        [HttpPut("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] CategoryFormDto model)
        {
            if (await _categories.GetByIdAsync(id) == null) return NotFound();

          
            if (await _categories.NameExistsAsync(model.CategoryName, id))
                return Conflict(new MessageDto { Message = NameExistsMessage });

            await _categories.UpdateAsync(id, model);
            return Ok(await _categories.GetByIdAsync(id));
        }


        [HttpDelete("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (await _categories.HasProductsAsync(id))
            {
                if (!await _categories.DeactivateAsync(id)) return NotFound();
                return Ok(new MessageDto { Message = "This category contains products, so it was deactivated instead of being deleted." });
            }

            if (!await _categories.DeleteAsync(id)) return NotFound();
            return NoContent();
        }
    }
}
