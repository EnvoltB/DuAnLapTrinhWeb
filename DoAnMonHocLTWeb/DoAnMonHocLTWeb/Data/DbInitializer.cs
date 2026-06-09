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

        // ========== TẠO TÀI KHOẢN ADMIN ==========
        var adminEmail = "admin@geardtk.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Quản trị viên",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                Console.WriteLine("Đã tạo tài khoản Admin");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Lỗi: {error.Description}");
                }
            }
        }
    }
}