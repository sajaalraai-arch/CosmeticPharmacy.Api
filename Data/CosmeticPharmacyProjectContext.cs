using CosmeticPharmacy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticPharmacy.Api.Data
{
    public class CosmeticPharmacyProjectContext : DbContext
    {
        public CosmeticPharmacyProjectContext(DbContextOptions<CosmeticPharmacyProjectContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get ; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderState> OrderStates { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         
            modelBuilder.Entity<Category>(e =>
            {
               
                e.HasIndex(c => c.CategoryName).IsUnique();
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.Property(p => p.Brand).HasMaxLength(100);              
                e.Property(p => p.ExpiryDate).HasColumnType("date");
                e.Property(p => p.DiscountPrice).HasColumnType("decimal(10,2)");
                e.HasIndex(p => p.Barcode).IsUnique();
            });

            modelBuilder.Entity<Customer>(e =>
            {
               
                e.Property(c => c.UserRole).HasMaxLength(20);
                e.Property(c => c.UserStatus).HasMaxLength(20);
                e.Property(c => c.CreatedAt).HasColumnType("date").HasDefaultValueSql("GETDATE()");
                e.HasIndex(c => c.Email).IsUnique();
            });

            modelBuilder.Entity<Cart>(e =>
            {
                e.Property(c => c.CreatedAt).HasColumnType("date").HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Order>(e =>
            {
                e.Property(o => o.OrderDate).HasColumnType("date");
                e.Property(o => o.TotalPrice).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Payment>(e =>
            {
                e.Property(p => p.PaymentState).HasMaxLength(50);
            });

            modelBuilder.Entity<Comment>(e =>
            {
                e.Property(c => c.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()").IsRequired();
            });
            modelBuilder.Entity<CartItem>()
               .Property(ci => ci.TotalPrice)
               .HasColumnType("decimal(18,2)");




            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

       
            modelBuilder.Entity<ProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

          
            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderState)
                .WithMany(s => s.Orders)
                .HasForeignKey(s => s.OrderStateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

         
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Customer)
                .WithOne(cu => cu.Cart)
                .HasForeignKey<Cart>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

        
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Customer)
                .WithMany(cu => cu.Comments)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

          
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<OrderItem>().HasKey(oi => new { oi.OrderId, oi.ProductId });
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi=>oi.Order)
                .WithMany(o=>o.OrderItems)
                .HasForeignKey(o=>o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi=>oi.Product)
                .WithMany(p=>p.OrderItems)
                .HasForeignKey(p=>p.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


          
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => new { ci.CartId, ci.ProductId });

         
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

          
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId);

    
           


            modelBuilder.Entity<OrderState>().HasData(
                new OrderState { OrderStateId = 1, NameState = "Pending" },
                new OrderState { OrderStateId = 2, NameState = "Processing" },
                new OrderState { OrderStateId = 3, NameState = "Completed" },
                new OrderState { OrderStateId = 4, NameState = "Cancelled" });

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Skin Care", CategoryImage = "/images/17afed7cb2554e7b197825dc46d6ff8d.jpg", IsActive = true },
                new Category { CategoryId = 2, CategoryName = "Body Care", CategoryImage = "/images/31397c9ca296bdae1c2d3f402ccbc710.jpg", IsActive = true },
                new Category { CategoryId = 3, CategoryName = "Hair Care", CategoryImage = "/images/34f7ee962da4f2f18144173ffc6b37f1.jpg.jpeg", IsActive = true });

            // مستخدم المدير الافتراضي: admin@beauty.com / Admin123
            modelBuilder.Entity<Customer>().HasData(new Customer
            {
                CustomerId = 1,
                FullName = "Admin User",
                Email = "admin@beauty.com",
                PasswordHash = "v1.100000.AQIDBAUGBwgJCgsMDQ4PEA==.rRdEjrVqjMsUfdtSqJMrlCD5135BhQf4TlICMxg6Xbk=",
                UserRole = "Admin",
                UserStatus = "Active",
                CreatedAt = new DateTime(2026, 1, 1),
                LastLogin = null,
             
            });

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1, CategoryId = 1, Brand = "La Roche-Posay", ProductName = "La Roche-Posay Anthelios",
                    ProductSlug = "la-roche-anthelios", Barcode = "3337875706605", TargetType = "All Types", Size = "50ml",
                    CountryOfOrigin = "France", ExpiryDate = new DateTime(2027, 12, 31),
                    ProductDescription = "High protection sunscreen", Ingredients = "Water, Diisopropyl Sebacate",
                    UsageInstructions = "Apply before sun exposure", Price = 120.00m, DiscountPrice = 110.00m, StockQuantity = 50,
                    MainImageUrl = "/images/0e783e31314620c9257274e840da9ec8.jpg", IsActive = true
                },
                new Product
                {
                    ProductId = 2, CategoryId = 2, Brand = "CeraVe", ProductName = "CeraVe Hydrating Cleanser",
                    ProductSlug = "cerave-hydrating-cleanser", Barcode = "3337875597197", TargetType = "Dry Skin", Size = "473ml",
                    CountryOfOrigin = "USA", ExpiryDate = new DateTime(2026, 6, 30),
                    ProductDescription = "Moisturizing facial cleanser", Ingredients = "Ceramides, Hyaluronic Acid",
                    UsageInstructions = "Use morning and evening", Price = 85.00m, DiscountPrice = 80.00m, StockQuantity = 100,
                    MainImageUrl = "/images/2b851c6d16d008c87a5e6e26e50b6818.jpg", IsActive = true
                },
                new Product
                {
                    ProductId = 3, CategoryId = 3, Brand = "The Ordinary", ProductName = "The Ordinary Vitamin C",
                    ProductSlug = "the-ordinary-vit-c", Barcode = "769915190311", TargetType = "Normal Skin", Size = "30ml",
                    CountryOfOrigin = "Canada", ExpiryDate = new DateTime(2027, 1, 15),
                    ProductDescription = "Brightening serum", Ingredients = "Ascorbic Acid, Ethoxydiglycol",
                    UsageInstructions = "Apply a few drops to face", Price = 45.00m, DiscountPrice = 45.00m, StockQuantity = 75,
                    MainImageUrl = "/images/69ae74a2f338c0b29b516f36c2ada04f.jpg", IsActive = true
                });
        }
    }
}
