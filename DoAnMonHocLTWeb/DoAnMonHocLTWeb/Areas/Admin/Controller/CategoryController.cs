using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Models;
using GearDTK.Repositories;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    // GET: Admin/Category
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return View(categories);
    }

    // GET: Admin/Category/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            // Tạo slug từ tên nếu chưa có
            if (string.IsNullOrEmpty(category.Slug))
            {
                category.Slug = GenerateSlug(category.Name);
            }

            // Kiểm tra slug đã tồn tại
            var existingCategory = await _categoryRepository.GetBySlugAsync(category.Slug);
            if (existingCategory != null)
            {
                ModelState.AddModelError("Slug", "Slug này đã tồn tại. Vui lòng nhập slug khác.");
                return View(category);
            }

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
            TempData["Success"] = "Thêm danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Admin/Category/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Admin/Category/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Tạo slug từ tên nếu chưa có
                if (string.IsNullOrEmpty(category.Slug))
                {
                    category.Slug = GenerateSlug(category.Name);
                }

                await _categoryRepository.UpdateAsync(category);
                await _categoryRepository.SaveChangesAsync();
                TempData["Success"] = "Cập nhật danh mục thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CategoryExists(category.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Admin/Category/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryRepository.GetByIdWithProductsAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Admin/Category/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _categoryRepository.GetByIdWithProductsAsync(id);

        // Kiểm tra nếu danh mục có sản phẩm thì không cho xóa
        if (category != null && category.Products != null && category.Products.Any())
        {
            TempData["Error"] = $"Không thể xóa danh mục vì có {category.Products.Count} sản phẩm đang thuộc danh mục này!";
            return RedirectToAction(nameof(Index));
        }

        await _categoryRepository.DeleteAsync(category);
        await _categoryRepository.SaveChangesAsync();
        TempData["Success"] = "Xóa danh mục thành công!";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CategoryExists(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null;
    }

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        string slug = name.ToLower().Trim();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        return slug;
    }
}