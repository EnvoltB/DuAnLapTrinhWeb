using Microsoft.AspNetCore.Identity;
using GearDTK.Models;

namespace GearDTK.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Đảm bảo database đã được tạo
        await context.Database.EnsureCreatedAsync();

        // ========== TẠO CÁC ROLE ==========
        string[] roleNames = { "Admin", "Employee", "Supplier", "Customer" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($"Đã tạo role: {roleName}");
            }
        }

        // ========== TẠO TÀI KHOẢN ADMIN MẶC ĐỊNH ==========
        var adminEmail = "admin@geardtk.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Quản trị viên hệ thống",
                Phone = "0123456789",
                Address = "Hà Nội, Việt Nam",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                Console.WriteLine("========================================");
                Console.WriteLine("ĐÃ TẠO TÀI KHOẢN ADMIN THÀNH CÔNG!");
                Console.WriteLine("Email: admin@geardtk.com");
                Console.WriteLine("Mật khẩu: Admin@123");
                Console.WriteLine("========================================");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Lỗi tạo Admin: {error.Description}");
                }
            }
        }

        // ========== TẠO TÀI KHOẢN EMPLOYEE MẶC ĐỊNH ==========
        var employeeEmail = "employee@geardtk.com";
        var employeeUser = await userManager.FindByEmailAsync(employeeEmail);

        if (employeeUser == null)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = employeeEmail,
                Email = employeeEmail,
                EmailConfirmed = true,
                FullName = "Nhân viên bán hàng",
                Phone = "0987654321",
                Address = "Hồ Chí Minh, Việt Nam",
                EmployeeCode = "EMP001",
                Department = "Sales",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Employee@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Employee");
                Console.WriteLine("Đã tạo tài khoản Employee: employee@geardtk.com / Employee@123");
            }
        }

        // ========== TẠO TÀI KHOẢN SUPPLIER MẶC ĐỊNH ==========
        var supplierEmail = "supplier@geardtk.com";
        var supplierUser = await userManager.FindByEmailAsync(supplierEmail);

        if (supplierUser == null)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = supplierEmail,
                Email = supplierEmail,
                EmailConfirmed = true,
                FullName = "Nhà cung cấp",
                Phone = "0912345678",
                Address = "Đà Nẵng, Việt Nam",
                CompanyName = "ATK Gear Official",
                TaxCode = "1234567890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Supplier@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Supplier");
                Console.WriteLine("Đã tạo tài khoản Supplier: supplier@geardtk.com / Supplier@123");
            }
        }

        // ========== TẠO TÀI KHOẢN CUSTOMER MẶC ĐỊNH ==========
        var customerEmail = "customer@geardtk.com";
        var customerUser = await userManager.FindByEmailAsync(customerEmail);

        if (customerUser == null)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = customerEmail,
                Email = customerEmail,
                EmailConfirmed = true,
                FullName = "Khách hàng",
                Phone = "0977777777",
                Address = "Hải Phòng, Việt Nam",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Customer@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Customer");
                Console.WriteLine("Đã tạo tài khoản Customer: customer@geardtk.com / Customer@123");
            }
        }

        // ========== SEED CATEGORIES (nếu chưa có) ==========
        if (!context.Categories.Any())
        {
            Console.WriteLine("Đang seed Categories...");

            var categories = new List<Category>
            {
                new Category { Name = "Gaming Mice", Slug = "gaming-mice", IconClass = "fa-solid fa-computer-mouse", Description = "Chuột gaming siêu nhẹ với cảm biến cao cấp", DisplayOrder = 1, ShowOnHomepage = true },
                new Category { Name = "Gaming Keyboards", Slug = "gaming-keyboards", IconClass = "fa-solid fa-keyboard", Description = "Bàn phím cơ và Hall Effect tốc độ cao", DisplayOrder = 2, ShowOnHomepage = true },
                new Category { Name = "Mouse Pads", Slug = "mouse-pads", IconClass = "fa-solid fa-square", Description = "Lót chuột chuyên nghiệp cho độ chính xác cao", DisplayOrder = 3, ShowOnHomepage = true },
                new Category { Name = "Gaming Headsets", Slug = "gaming-headsets", IconClass = "fa-solid fa-headphones", Description = "Tai nghe gaming âm thanh vòm 7.1", DisplayOrder = 4, ShowOnHomepage = true }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("Đã seed Categories thành công!");
        }
    }
}