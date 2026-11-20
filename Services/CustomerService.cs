using CosmeticPharmacy.Api.Data;
using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Helpers;
using CosmeticPharmacy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Services
{
    public class CustomerService
    {
        private static readonly string[] Roles = { "Admin", "Customer" };
        private static readonly string[] Statuses = { "Active", "Banned" };

        private readonly CosmeticPharmacyProjectContext _db;

        public CustomerService(CosmeticPharmacyProjectContext db)
        {
            _db = db;
        }

        public static CustomerDto ToDto(Customer c) => new CustomerDto
        {
            CustomerId = c.CustomerId,
            FullName = c.FullName,
            Email = c.Email,
            UserRole = c.UserRole,
            UserStatus = c.UserStatus,
            CreatedAt = c.CreatedAt,
            LastLogin = c.LastLogin,
        };

        public async Task<CustomersIndexDto> GetAllAsync(string? searchTerm, string? role, string? status)
        {
            var query = _db.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string s = searchTerm.Trim();
                query = query.Where(c => c.FullName.Contains(s) || c.Email.Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.UserRole == role);
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.UserStatus == status);

            var customers = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            return new CustomersIndexDto
            {
                TotalUsers = await _db.Customers.CountAsync(),
                ActiveUsers = await _db.Customers.CountAsync(c => c.UserStatus == "Active"),
                BannedUsers = await _db.Customers.CountAsync(c => c.UserStatus == "Banned"),
                Customers = customers.Select(ToDto).ToList()
            };
        }

     

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return null;
            return ToDto(customer);
        }

        public async Task<(string? error, CustomerDto? customer)> CreateAsync(CustomerCreateDto dto)
        {
            string email = dto.Email.Trim();
            string role = string.IsNullOrWhiteSpace(dto.UserRole) ? "Customer" : dto.UserRole;
            string status = string.IsNullOrWhiteSpace(dto.UserStatus) ? "Active" : dto.UserStatus;

            if (!Roles.Contains(role) || !Statuses.Contains(status))
                return ("Invalid role or status.", null);

            if (await _db.Customers.AnyAsync(c => c.Email == email))
                return ("This email address is already in use.", null);

            var customer = new Customer
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHasher.Hash(string.IsNullOrEmpty(dto.Password) ? "123456" : dto.Password),
                UserRole = role,
                UserStatus = status,
                CreatedAt = DateTime.Now,
                Cart = new Cart { CreatedAt = DateTime.Now }
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return (null, ToDto(customer));
        }

        public async Task<(string? error, CustomerDto? customer)> UpdateAsync(int id, CustomerUpdateDto dto)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return ("NotFound", null);

            string email = dto.Email.Trim();

            if (!Roles.Contains(dto.UserRole) || !Statuses.Contains(dto.UserStatus))
                return ("Invalid role or status.", null);

            if (await _db.Customers.AnyAsync(c => c.Email == email && c.CustomerId != id))
                return ("This email address is already in use.", null);

            customer.FullName = dto.FullName.Trim();
            customer.Email = email;
            customer.UserRole = dto.UserRole;
            customer.UserStatus = dto.UserStatus;

            await _db.SaveChangesAsync();
            return (null, ToDto(customer));
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return "NotFound";

            if (await _db.Orders.AnyAsync(o => o.CustomerId == id))
                return "This user cannot be deleted because he has orders.";

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
            return null;
        }
    }
}