using System.Linq.Expressions;
using CosmeticPharmacy.Api.Data;
using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Helpers;
using CosmeticPharmacy.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private static readonly Expression<Func<Comment, CommentDto>> ToDto = c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            Rating = c.Rating,
            CreatedAt = c.CreatedAt,
            ProductId = c.ProductId,
            CustomerId = c.CustomerId,
            CustomerName = c.Customer.FullName
        };

        private readonly CosmeticPharmacyProjectContext _db;

        public CommentsController(CosmeticPharmacyProjectContext db)
        {
            _db = db;
        }


        [HttpGet("product/{productId:int:min(1)}")]
        public async Task<ActionResult<List<CommentDto>>> GetByProduct([FromRoute] int productId)
        {
            bool productExists = await _db.Products
                .AnyAsync(p => p.ProductId == productId);

            if (!productExists)
                return NotFound(new MessageDto { Message = "Product not found." });

            var comments = await _db.Comments.AsNoTracking()
                .Where(c => c.ProductId == productId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(ToDto)
                .ToListAsync();

            return Ok(comments);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CommentCreateDto dto)
        {
            if (!await _db.Products.AnyAsync(p => p.ProductId == dto.ProductId))
                return NotFound(new MessageDto { Message = "Product not found." });

            var comment = new Comment
            {
                Content = dto.Content.Trim(),
                Rating = dto.Rating,
                ProductId = dto.ProductId,
                CustomerId = User.GetCustomerId() ?? 0,
                CreatedAt = DateTime.Now
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            var created = await _db.Comments.AsNoTracking().Where(c => c.Id == comment.Id).Select(ToDto).FirstAsync();
            return CreatedAtAction(nameof(GetByProduct), new { productId = comment.ProductId }, created);
        }

      
        [HttpDelete("{id:int:min(1)}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            bool isOwner = comment.CustomerId == User.GetCustomerId();
            if (!isOwner && !User.IsInRole("Admin")) return Forbid();

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
