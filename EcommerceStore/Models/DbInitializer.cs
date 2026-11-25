     using EcommerceStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Models
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // Seed Admin User
            if (await userManager.FindByEmailAsync("admin@ecommerce.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@ecommerce.com",
                    Email = "admin@ecommerce.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Test Customer
            if (await userManager.FindByEmailAsync("customer@test.com") == null)
            {
                var customerUser = new ApplicationUser
                {
                    UserName = "customer@test.com",
                    Email = "customer@test.com",
                    FirstName = "Test",
                    LastName = "Customer",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(customerUser, "Customer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "Electronics",
                        Description = "Electronic devices and accessories",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Name = "Clothing",
                        Description = "Fashion and apparel",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Name = "Books",
                        Description = "Books and educational materials",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Name = "Home & Garden",
                        Description = "Home improvement and gardening",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Name = "Sports & Outdoors",
                        Description = "Sports equipment and outdoor gear",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Products
            if (!context.Products.Any())
            {
                var electronicsCategory = await context.Categories.FirstAsync(c => c.Name == "Electronics");
                var clothingCategory = await context.Categories.FirstAsync(c => c.Name == "Clothing");
                var booksCategory = await context.Categories.FirstAsync(c => c.Name == "Books");

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Wireless Bluetooth Headphones",
                        Description = "High-quality wireless headphones with noise cancellation",
                        Price = 79.99m,
                        StockQuantity = 50,
                        CategoryId = electronicsCategory.CategoryId,
                        ImageUrl = "/images/products/headphones.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Smart Watch",
                        Description = "Fitness tracker with heart rate monitor",
                        Price = 199.99m,
                        StockQuantity = 30,
                        CategoryId = electronicsCategory.CategoryId,
                        ImageUrl = "/images/products/smartwatch.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Laptop Stand",
                        Description = "Ergonomic aluminum laptop stand",
                        Price = 49.99m,
                        StockQuantity = 100,
                        CategoryId = electronicsCategory.CategoryId,
                        ImageUrl = "/images/products/laptop-stand.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Cotton T-Shirt",
                        Description = "Comfortable 100% cotton t-shirt",
                        Price = 19.99m,
                        StockQuantity = 200,
                        CategoryId = clothingCategory.CategoryId,
                        ImageUrl = "/images/products/tshirt.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Denim Jeans",
                        Description = "Classic fit denim jeans",
                        Price = 59.99m,
                        StockQuantity = 150,
                        CategoryId = clothingCategory.CategoryId,
                        ImageUrl = "/images/products/jeans.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Programming Book Bundle",
                        Description = "Complete programming guide for beginners",
                        Price = 89.99m,
                        StockQuantity = 75,
                        CategoryId = booksCategory.CategoryId,
                        ImageUrl = "/images/products/books.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Wireless Mouse",
                        Description = "Ergonomic wireless mouse with USB receiver",
                        Price = 24.99m,
                        StockQuantity = 120,
                        CategoryId = electronicsCategory.CategoryId,
                        ImageUrl = "/images/products/mouse.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Mechanical Keyboard",
                        Description = "RGB mechanical gaming keyboard",
                        Price = 129.99m,
                        StockQuantity = 60,
                        CategoryId = electronicsCategory.CategoryId,
                        ImageUrl = "/images/products/keyboard.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
