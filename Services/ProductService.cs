using System.Text.RegularExpressions;
using CosmeticPharmacy.Api.Data;
using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Helpers;
using CosmeticPharmacy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Services
{
    public class ProductService
    {
        private const int LowStockLimit = 30;

        private readonly CosmeticPharmacyProjectContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductService(CosmeticPharmacyProjectContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }


        public async Task<ProductsIndexDto?> GetAllAsync(string? search, string? filter, int? categoryId, string? categoryName, bool hideExpired = false)
        {
            if (categoryId.HasValue)
            {
                categoryName = await _db.Categories
                    .Where(c => c.CategoryId == categoryId)
                    .Select(c => c.CategoryName)
                    .FirstOrDefaultAsync();
                if (categoryName == null) return null;
            }

            IQueryable<Product> scope = _db.Products.AsNoTracking().Include(p => p.Category);
            if (!string.IsNullOrWhiteSpace(categoryName))
                scope = scope.Where(p => p.Category!.CategoryName == categoryName);


            if (hideExpired)
            {
                DateTime today = DateTime.Today;
                scope = scope.Where(p => p.ExpiryDate >= today);
            }

            var products = await ApplySearchAndFilter(scope, search, filter).OrderBy(p => p.ProductId).ToListAsync();

            return new ProductsIndexDto
            {
                CategoryName = categoryName,
                Stats = await BuildStatsAsync(scope),
                Products = products.Select(ToListItem).ToList()
            };
        }

        public async Task<ProductDetailsDto?> GetByIdAsync(int id)
        {
            var product = await _db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            return product == null ? null : ToDetails(product);
        }


        public async Task<List<ProductListItemDto>> GetTopActiveAsync(int count)
        {
            DateTime today = DateTime.Today;


            var products = await _db.Products.AsNoTracking().Include(p => p.Category)
                .Where(p => p.IsActive && p.ExpiryDate >= today)
                .OrderBy(p => p.ProductId)
                .Take(count)
                .ToListAsync();

            return products.Select(ToListItem).ToList();
        }


        public async Task<List<ProductListItemDto>> GetLatestAsync(int count)
        {
            var products = await _db.Products.AsNoTracking().Include(p => p.Category)
                .OrderByDescending(p => p.ProductId)
                .Take(count)
                .ToListAsync();

            return products.Select(ToListItem).ToList();
        }

        public Task<bool> BarcodeExistsAsync(string barcode, int currentId = 0)
        {
            barcode = barcode.Trim();
            return _db.Products.AnyAsync(p => p.Barcode == barcode && p.ProductId != currentId);
        }

        public Task<bool> CategoryExistsAsync(int? categoryId)
        {
            return _db.Categories.AnyAsync(c => c.CategoryId == categoryId);
        }


        public async Task<int> CreateAsync(ProductFormDto model)
        {
            var product = new Product { MainImageUrl = (await ImageStorage.SaveAsync(model.ImageFile, _env))! };
            Apply(product, model);
            await SaveGalleryAsync(product, model);

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return product.ProductId;
        }

        public async Task UpdateAsync(int id, ProductFormDto model)
        {
            var product = await _db.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return;

            product.MainImageUrl = await ImageStorage.SaveAsync(model.ImageFile, _env) ?? product.MainImageUrl;
            Apply(product, model);
            await SaveGalleryAsync(product, model);

            await _db.SaveChangesAsync();
        }


        public Task<bool> IsInOrdersAsync(int id)
        {
            return _db.OrderItems.AnyAsync(oi => oi.ProductId == id);
        }


        public async Task<bool> DeactivateAsync(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return false;

            product.IsActive = false;
            _db.CartItems.RemoveRange(await _db.CartItems.Where(ci => ci.ProductId == id).ToListAsync());

            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return false;

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return true;
        }




        private static IQueryable<Product> ApplySearchAndFilter(IQueryable<Product> query, string? search, string? filter)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim();
                query = query.Where(p => p.ProductName.Contains(s)
                                      || (p.Barcode != null && p.Barcode.Contains(s))
                                      || (p.Brand != null && p.Brand.Contains(s)));
            }

            DateTime expiryLimit = DateTime.Now.AddMonths(3);

            switch (filter?.Trim().ToLowerInvariant())
            {
                case "lowstock":
                    query = query.Where(p => p.StockQuantity < LowStockLimit);
                    break;
                case "expiring":
                    query = query.Where(p => p.ExpiryDate <= expiryLimit);
                    break;
                case "active":
                    query = query.Where(p => p.IsActive);
                    break;
            }

            return query;
        }

        private static async Task<ProductStatsDto> BuildStatsAsync(IQueryable<Product> query)
        {
            DateTime expiryLimit = DateTime.Now.AddMonths(3);

            return new ProductStatsDto
            {
                TotalProducts = await query.CountAsync(),
                LowStock = await query.CountAsync(p => p.StockQuantity < LowStockLimit),
                ExpiringSoon = await query.CountAsync(p => p.ExpiryDate <= expiryLimit),
                ActiveItems = await query.CountAsync(p => p.IsActive)
            };
        }

        private static ProductListItemDto ToListItem(Product p) => new ProductListItemDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            MainImageUrl = p.MainImageUrl,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            StockQuantity = p.StockQuantity,
            ExpiryDate = p.ExpiryDate,
            IsActive = p.IsActive,
            TargetType = p.TargetType,
            Barcode = p.Barcode,
            Size = p.Size,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.CategoryName,
            Brand = p.Brand
        };

        private static ProductDetailsDto ToDetails(Product p) => new ProductDetailsDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductSlug = p.ProductSlug,
            MainImageUrl = p.MainImageUrl,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            StockQuantity = p.StockQuantity,
            ExpiryDate = p.ExpiryDate,
            IsActive = p.IsActive,
            TargetType = p.TargetType,
            Barcode = p.Barcode,
            Size = p.Size,
            CountryOfOrigin = p.CountryOfOrigin,
            ProductDescription = p.ProductDescription,
            Ingredients = p.Ingredients,
            UsageInstructions = p.UsageInstructions,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.CategoryName,
            Brand = p.Brand,
            ProductImages = p.ProductImages
                .OrderBy(i => i.ImageId)
                .Select(i => new ProductImageDto { ImageId = i.ImageId, ImageUrl = i.ImageUrl })
                .ToList()
        };


        private static void Apply(Product product, ProductFormDto model)
        {
            product.ProductName = model.ProductName.Trim();
            product.ProductSlug = string.IsNullOrWhiteSpace(model.ProductSlug)
                ? Regex.Replace(model.ProductName.Trim().ToLowerInvariant().Replace(' ', '-'), @"[^\w-]+", "")
                : model.ProductSlug.Trim();
            product.CategoryId = model.CategoryId;
            product.Brand = model.Brand.Trim();
            product.Barcode = model.Barcode.Trim();
            product.TargetType = model.TargetType;
            product.Size = model.Size;
            product.CountryOfOrigin = model.CountryOfOrigin;
            product.ExpiryDate = model.ExpiryDate!.Value;
            product.ProductDescription = model.ProductDescription;
            product.Ingredients = model.Ingredients;
            product.UsageInstructions = model.UsageInstructions;
            product.Price = model.Price!.Value;
            product.DiscountPrice = model.DiscountPrice;
            product.StockQuantity = model.StockQuantity!.Value;
            product.IsActive = model.IsActive;
        }


        private async Task SaveGalleryAsync(Product product, ProductFormDto model)
        {
            var files = new[] { model.GalleryImage1, model.GalleryImage2, model.GalleryImage3 };
            var existing = product.ProductImages.OrderBy(i => i.ImageId).ToList();

            for (int index = 0; index < files.Length; index++)
            {
                string? url = await ImageStorage.SaveAsync(files[index], _env);
                if (url == null) continue;

                if (index < existing.Count) existing[index].ImageUrl = url;
                else product.ProductImages.Add(new ProductImage { ImageUrl = url });
            }
        }
    }
}
