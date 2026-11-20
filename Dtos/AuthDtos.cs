using System.ComponentModel.DataAnnotations;

namespace CosmeticPharmacy.Api.Dtos
{
    public class LoginDto
    {
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class RegisterDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MinLength(4)]
        public string Password { get; set; } = null!;
    }

    public class CurrentUserDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "Customer";
    }

    public class MessageDto
    {
        public string Message { get; set; } = "";
        public bool CanDeactivate { get; set; }
    }
}
