using System.Security.Claims;
using CosmeticPharmacy.Api.Data;
using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Helpers;
using CosmeticPharmacy.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CosmeticPharmacyProjectContext _db;

        public AuthController(CosmeticPharmacyProjectContext db)
        {
            _db = db;
        }

       
        [HttpGet("emails")]
        public async Task<ActionResult<List<string>>> GetRegisteredEmails()
        {
            return await _db.Customers.AsNoTracking().OrderBy(c => c.Email).Select(c => c.Email).ToListAsync();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            string email = dto.Email.Trim();
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null || !PasswordHasher.Verify(dto.Password, customer.PasswordHash))
                return Unauthorized(new MessageDto { Message = "Incorrect email or password" });

            if (customer.UserStatus == "Banned")
                return StatusCode(StatusCodes.Status403Forbidden, new MessageDto { Message = "Sorry,this account is blocked" });

            customer.LastLogin = DateTime.Now;     
            await _db.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                new Claim(ClaimTypes.Name, customer.FullName),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim(ClaimTypes.Role, customer.UserRole ?? "Customer")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Ok(new CurrentUserDto
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Email = customer.Email,
                Role = customer.UserRole ?? "Customer"
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            string email = dto.Email.Trim();

            if (await _db.Customers.AnyAsync(c => c.Email == email))
                return Conflict(new MessageDto { Message = "This email address is already in use. Please choose another email address" });

            var customer = new Customer
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                UserRole = "Customer",
                UserStatus = "Active",
                CreatedAt = DateTime.Now,
                Cart = new Cart { CreatedAt = DateTime.Now }
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new MessageDto { Message = "You have been successfully registered" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public ActionResult<CurrentUserDto> GetCurrentUser()
        {
            return Ok(new CurrentUserDto
            {
                CustomerId = User.GetCustomerId() ?? 0,
                FullName = User.FindFirstValue(ClaimTypes.Name) ?? "",
                Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
                Role = User.FindFirstValue(ClaimTypes.Role) ?? "Customer"
            });
        }
    }
}
