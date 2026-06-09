using Microsoft.AspNetCore.Identity;

namespace GearDTK.Data;

public class ApplicationUser : IdentityUser
{
    // Thông tin cá nhân
    public string? FullName { get; set; }
    public string? Address { get; set; }

    // Sửa lỗi: dùng property khác thay vì PhoneNumber (đã có trong IdentityUser)
    public string? Phone { get; set; }  // Đổi tên thành Phone

    // Thông tin cho Supplier
    public string? CompanyName { get; set; }
    public string? TaxCode { get; set; }

    // Thông tin cho Employee
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }

    // Thời gian
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Trạng thái
    public bool IsActive { get; set; } = true;
}