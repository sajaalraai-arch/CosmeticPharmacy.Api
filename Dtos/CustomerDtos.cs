using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Dtos
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? UserRole { get; set; }
        public string? UserStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
     
    }

    public class CustomersIndexDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BannedUsers { get; set; }
        public List<CustomerDto> Customers { get; set; } = new();
    }

    public class CustomerCreateDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;
        [MaxLength(20)]
        public string? UserRole { get; set; }
        [MaxLength(20)]
        public string? UserStatus { get; set; }

        /// <summary>اختياري؛ القيمة الافتراضية 123456 كما في المشروع القديم.</summary>
        public string? Password { get; set; }
    }

    public class CustomerUpdateDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string UserRole { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string UserStatus { get; set; } = null!;
    }
}
