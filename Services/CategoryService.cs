using CosmeticPharmacy.Api.Data;
using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Helpers;
using CosmeticPharmacy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Services
{
    public class CategoryService
    {
        private readonly CosmeticPharmacyProjectContext _db;
        private readonly IWebHostEnvironment _env;

        public CategoryService(CosmeticPharmacyProjectContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }


        public async Task<List<CategoryListItemDto>> GetAllAsync(bool activeOnly = false)
        {
            var query = _db.Categories.AsNoTracking();
            if (activeOnly) query = query.Where(c => c.IsActive);

            return await query.OrderBy(c => c.CategoryId)
                .Select(c => new CategoryListItemDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryImage = c.CategoryImage,
                    IsActive = c.IsActive,
                    ProductsCount = c.Products.Count
                })
                .ToListAsync();
        }

        public async Task<CategoryListItemDto?> GetByIdAsync(int id)
        {
            var categories = await GetAllAsync();      
            return categories.FirstOrDefault(c => c.CategoryId == id);
        }

        public Task<bool> NameExistsAsync(string name, int currentId = 0)
        {
            name = name.Trim();
            return _db.Categories.AnyAsync(c => c.CategoryName == name && c.CategoryId != currentId);
        }

        public Task<bool> HasProductsAsync(int id)
        {
            return _db.Products.AnyAsync(p => p.CategoryId == id);
        }


        public async Task<int> CreateAsync(CategoryFormDto model)
        {
            var category = new Category
            {
                CategoryName = model.CategoryName.Trim(),
                CategoryImage = await ResolveImageAsync(model),
                IsActive = model.IsActive
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return category.CategoryId;
        }

        public async Task UpdateAsync(int id, CategoryFormDto model)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return;

            category.CategoryName = model.CategoryName.Trim();
            category.CategoryImage = await ResolveImageAsync(model) ?? category.CategoryImage;
            category.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
        }


        public async Task<bool> DeactivateAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return false;

            category.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return false;

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return true;
        }


        private async Task<string?> ResolveImageAsync(CategoryFormDto model)
        {
            return await ImageStorage.SaveAsync(model.ImageFile, _env)
                   ?? (string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim());
        }
    }
}
