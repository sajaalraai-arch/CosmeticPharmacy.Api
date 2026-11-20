using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Dtos
{
    public class CategoryListItemDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public string? CategoryImage { get; set; }
        public bool IsActive { get; set; }
        public int ProductsCount { get; set; }
    }


    public class CategoryFormDto
    {
        [Required(ErrorMessage = "Category Name is required")]
        [MaxLength(100)]
        public string CategoryName { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
