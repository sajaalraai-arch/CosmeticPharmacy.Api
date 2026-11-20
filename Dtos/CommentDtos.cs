using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Dtos
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public int? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
    }

    public class CommentCreateDto
    {
        public int ProductId { get; set; }

        [Required, MaxLength(1000)]
        public string Content { get; set; } = null!;

   
        [Range(1, 5, ErrorMessage = "Please choose a rating from 1 to 5 stars.")]
        public int Rating { get; set; }
    }
}
