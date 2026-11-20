using System.ComponentModel.DataAnnotations;
using CosmeticPharmacy.Api.Helpers;

namespace CosmeticPharmacy.Api.Dtos
{
    public class ProductListItemDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? MainImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string? TargetType { get; set; }
        public string? Barcode { get; set; }
        public string? Size { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Brand { get; set; }
    }

    public class ProductImageDto
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    public class ProductDetailsDto : ProductListItemDto
    {
        public string? ProductSlug { get; set; }
        public string? CountryOfOrigin { get; set; }
        public string? ProductDescription { get; set; }
        public string? Ingredients { get; set; }
        public string? UsageInstructions { get; set; }
        public List<ProductImageDto> ProductImages { get; set; } = new();
    }

    public class ProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int LowStock { get; set; }
        public int ExpiringSoon { get; set; }
        public int ActiveItems { get; set; }
    }

    public class ProductsIndexDto
    {
        public ProductStatsDto Stats { get; set; } = new();
        public List<ProductListItemDto> Products { get; set; } = new();
        public string? CategoryName { get; set; }
    }


    public class ProductFormDto : IValidatableObject
    {
        [Required, MaxLength(30)]
        public string ProductName { get; set; } = null!;

        [MaxLength(100)]
        public string? ProductSlug { get; set; }

        [Required]
        public int? CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Brand { get; set; } = null!;

        [Required, MaxLength(30)]
        public string Barcode { get; set; } = null!;

        [Required, MaxLength(30)]
        public string TargetType { get; set; } = null!;

        [Required, MaxLength(20)]
        public string Size { get; set; } = null!;

        [Required, MaxLength(30)]
        public string CountryOfOrigin { get; set; } = null!;

        [Required]
        public DateTime? ExpiryDate { get; set; }

        [Required, MaxLength(300)]
        public string ProductDescription { get; set; } = null!;

        [Required]
        public string Ingredients { get; set; } = null!;

        [Required]
        public string UsageInstructions { get; set; } = null!;

        [Required, Range(1, 1000)]
        public decimal? Price { get; set; }

        [Range(0.99, 999)]
        public decimal? DiscountPrice { get; set; }

        [Required, Range(0, 250)]
        public int? StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public IFormFile? ImageFile { get; set; }
        public IFormFile? GalleryImage1 { get; set; }
        public IFormFile? GalleryImage2 { get; set; }
        public IFormFile? GalleryImage3 { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DiscountPrice > Price)
                yield return new ValidationResult("Discount price cannot be higher than the base price.", new[] { nameof(DiscountPrice) });

            foreach (var (field, file) in new[]
            {
                (nameof(ImageFile), ImageFile),
                (nameof(GalleryImage1), GalleryImage1),
                (nameof(GalleryImage2), GalleryImage2),
                (nameof(GalleryImage3), GalleryImage3)
            })
            {
                string? error = ImageStorage.Validate(file);
                if (error != null) yield return new ValidationResult(error, new[] { field });
            }
        }
    }
}
