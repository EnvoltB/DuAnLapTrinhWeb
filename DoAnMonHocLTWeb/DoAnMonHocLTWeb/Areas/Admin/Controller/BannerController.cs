using GearDTK.Data;
using GearDTK.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BannerController : Controller
{
    private const long MaxBannerImageSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BannerController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Admin/Banner
    public async Task<IActionResult> Index()
    {
        var Banner = await _context.Banners
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();
        return View(Banner);
    }

    // GET: Admin/Banner/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Banner/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Banners banner)
    {
        RemoveBannerValidationFields();
        ValidateImageFile(banner.ImageFile, isRequired: true);

        if (ModelState.IsValid)
        {
            try
            {
                banner.ImageUrl = await SaveBannerImageAsync(banner.ImageFile!);
                banner.Title ??= Path.GetFileNameWithoutExtension(banner.ImageFile!.FileName);
                banner.CreatedAt = DateTime.Now;

                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }
        }

        return View(banner);
    }

    // GET: Admin/Banner/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var Banner = await _context.Banners.FindAsync(id);
        if (Banner == null)
        {
            return NotFound();
        }

        return View(Banner);
    }

    // POST: Admin/Banner/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Banners Banner)
    {
        if (id != Banner.Id)
        {
            return NotFound();
        }

        RemoveBannerValidationFields();
        ValidateImageFile(Banner.ImageFile, isRequired: false);

        if (ModelState.IsValid)
        {
            try
            {
                var existingBanner = await _context.Banners.FindAsync(id);
                if (existingBanner == null)
                {
                    return NotFound();
                }

                existingBanner.LinkUrl = Banner.LinkUrl;
                existingBanner.DisplayOrder = Banner.DisplayOrder;
                existingBanner.IsActive = Banner.IsActive;
                existingBanner.IsMainSlider = Banner.IsMainSlider;

                if (Banner.ImageFile != null && Banner.ImageFile.Length > 0)
                {
                    DeleteBannerImage(existingBanner.ImageUrl);
                    existingBanner.ImageUrl = await SaveBannerImageAsync(Banner.ImageFile);

                    if (string.IsNullOrEmpty(existingBanner.Title))
                    {
                        existingBanner.Title = Path.GetFileNameWithoutExtension(Banner.ImageFile.FileName);
                    }
                }

                existingBanner.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật Banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BannerExists(Banner.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }
        }

        await RestoreExistingBannerValuesAsync(Banner, id);
        return View(Banner);
    }

    // GET: Admin/Banner/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var Banner = await _context.Banners
            .FirstOrDefaultAsync(b => b.Id == id);

        if (Banner == null)
        {
            return NotFound();
        }

        return View(Banner);
    }

    // POST: Admin/Banner/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var Banner = await _context.Banners.FindAsync(id);
        if (Banner != null)
        {
            DeleteBannerImage(Banner.ImageUrl);

            _context.Banners.Remove(Banner);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa Banner thành công!";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Banner/ToggleStatus
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var Banner = await _context.Banners.FindAsync(id);
        if (Banner == null)
        {
            return Json(new { success = false, message = "Không tìm thấy Banner" });
        }

        Banner.IsActive = !Banner.IsActive;
        await _context.SaveChangesAsync();

        return Json(new { success = true, isActive = Banner.IsActive });
    }

    // POST: Admin/Banner/ToggleMainSlider
    [HttpPost]
    public async Task<IActionResult> ToggleMainSlider(int id)
    {
        var Banner = await _context.Banners.FindAsync(id);
        if (Banner == null)
        {
            return Json(new { success = false, message = "Không tìm thấy Banner" });
        }

        Banner.IsMainSlider = !Banner.IsMainSlider;
        await _context.SaveChangesAsync();

        return Json(new { success = true, isMainSlider = Banner.IsMainSlider });
    }

    // GET: Admin/Banner/GetBanner
    [HttpGet]
    public async Task<IActionResult> GetBanner()
    {
        var Banner = await _context.Banners
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new
            {
                b.Id,
                b.ImageUrl,
                b.LinkUrl,
                b.Title,
                b.DisplayOrder
            })
            .ToListAsync();

        return Json(Banner);
    }

    // GET: Admin/Banner/GetMainSliderBanner
    [HttpGet]
    public async Task<IActionResult> GetMainSliderBanner()
    {
        var Banner = await _context.Banners
            .Where(b => b.IsActive && b.IsMainSlider)
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new
            {
                b.Id,
                b.ImageUrl,
                b.LinkUrl,
                b.Title,
                b.DisplayOrder
            })
            .ToListAsync();

        return Json(Banner);
    }

    private async Task<bool> BannerExists(int id)
    {
        return await _context.Banners.AnyAsync(b => b.Id == id);
    }

    private void RemoveBannerValidationFields()
    {
        ModelState.Remove("ImageUrl");
        ModelState.Remove("Title");
        ModelState.Remove("Description");
        ModelState.Remove("UpdatedAt");
        ModelState.Remove("ProductId");
        ModelState.Remove("Product");
    }

    private void ValidateImageFile(IFormFile? imageFile, bool isRequired)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            if (isRequired)
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh");
            }

            return;
        }

        if (imageFile.Length > MaxBannerImageSize)
        {
            ModelState.AddModelError("ImageFile", "Kích thước ảnh không được vượt quá 5MB");
        }

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
        {
            ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file JPG, PNG, WEBP hoặc GIF");
        }
    }

    private async Task<string> SaveBannerImageAsync(IFormFile imageFile)
    {
        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "banners");
        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName).ToLowerInvariant()}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        return $"/images/banners/{fileName}";
    }

    private void DeleteBannerImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }
    }

    private async Task RestoreExistingBannerValuesAsync(Banners Banner, int id)
    {
        var existingBanner = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (existingBanner == null)
        {
            return;
        }

        Banner.ImageUrl = existingBanner.ImageUrl;
        Banner.CreatedAt = existingBanner.CreatedAt;
        Banner.Title = existingBanner.Title;
        Banner.Description = existingBanner.Description;
    }
}
