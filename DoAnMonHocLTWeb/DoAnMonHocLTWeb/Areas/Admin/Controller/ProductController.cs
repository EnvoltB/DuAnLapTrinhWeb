using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearDTK.Models;
using GearDTK.Repositories;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // GET: Admin/Product
    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllWithCategoryAsync();
        return View(products);
    }

    // GET: Admin/Product/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
        return View();
    }

    // POST: Admin/Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        // Remove validation cho các field không cần thiết
        ModelState.Remove("MainImageUrl");
        ModelState.Remove("GalleryImages");
        ModelState.Remove("Category");
        ModelState.Remove("CategoryId");
        ModelState.Remove("CreatedAt");
        ModelState.Remove("UpdatedAt");

        if (ModelState.IsValid)
        {
            try
            {
                // Tạo slug từ tên nếu chưa có
                if (string.IsNullOrEmpty(product.Slug))
                {
                    product.Slug = GenerateSlug(product.Name);
                }

                // Kiểm tra slug đã tồn tại
                var existingProduct = await _productRepository.GetBySlugAsync(product.Slug);
                if (existingProduct != null)
                {
                    ModelState.AddModelError("Slug", "Slug này đã tồn tại. Vui lòng nhập slug khác.");
                    ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
                    return View(product);
                }

                // Xử lý upload ảnh chính
                if (product.MainImageFile != null && product.MainImageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(product.MainImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("MainImageFile", "Chỉ chấp nhận file JPG, JPEG, PNG, GIF, WEBP");
                        ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
                        return View(product);
                    }

                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.MainImageFile.CopyToAsync(stream);
                    }

                    product.MainImageUrl = "/images/products/" + fileName;
                }
                else
                {
                    product.MainImageUrl = "/images/products/default.png";
                }

                // Xử lý upload thư viện ảnh
                if (product.GalleryFiles != null && product.GalleryFiles.Count > 0)
                {
                    List<string> galleryPaths = new List<string>();
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products/gallery");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var file in product.GalleryFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var fileExtension = Path.GetExtension(file.FileName).ToLower();
                            string fileName = Guid.NewGuid().ToString() + fileExtension;
                            string filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            galleryPaths.Add("/images/products/gallery/" + fileName);
                        }
                    }

                    product.GalleryImages = string.Join(",", galleryPaths);
                }

                product.CreatedAt = DateTime.UtcNow;
                await _productRepository.AddAsync(product);
                await _productRepository.SaveChangesAsync();

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
                return View(product);
            }
        }

        ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    // GET: Admin/Product/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    // POST: Admin/Product/Edit/5 (CÓ XỬ LÝ UPLOAD ẢNH)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        // Lấy thông tin sản phẩm cũ từ database
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        // Remove validation không cần thiết
        ModelState.Remove("Category");
        ModelState.Remove("CreatedAt");
        ModelState.Remove("MainImageUrl");
        ModelState.Remove("GalleryImages");

        if (ModelState.IsValid)
        {
            try
            {
                // Cập nhật các trường cơ bản
                existingProduct.Name = product.Name;
                existingProduct.Slug = string.IsNullOrEmpty(product.Slug) ? GenerateSlug(product.Name) : product.Slug;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.Brand = product.Brand;
                existingProduct.Price = product.Price;
                existingProduct.ComparePrice = product.ComparePrice;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.ShortDescription = product.ShortDescription;
                existingProduct.Description = product.Description;
                existingProduct.Weight = product.Weight;
                existingProduct.Color = product.Color;
                existingProduct.Specifications = product.Specifications;
                existingProduct.IsFeatured = product.IsFeatured;
                existingProduct.IsNew = product.IsNew;
                existingProduct.IsBestSeller = product.IsBestSeller;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                // ========== XỬ LÝ UPLOAD ẢNH CHÍNH MỚI ==========
                if (product.MainImageFile != null && product.MainImageFile.Length > 0)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existingProduct.MainImageUrl) && existingProduct.MainImageUrl != "/images/products/default.png")
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.MainImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Upload ảnh mới
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.MainImageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.MainImageFile.CopyToAsync(stream);
                    }

                    existingProduct.MainImageUrl = "/images/products/" + fileName;
                }

                // ========== XỬ LÝ THÊM ẢNH GALLERY MỚI ==========
                if (product.GalleryFiles != null && product.GalleryFiles.Count > 0)
                {
                    List<string> existingGallery = string.IsNullOrEmpty(existingProduct.GalleryImages)
                        ? new List<string>()
                        : existingProduct.GalleryImages.Split(',').ToList();

                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products/gallery");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var file in product.GalleryFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            existingGallery.Add("/images/products/gallery/" + fileName);
                        }
                    }

                    existingProduct.GalleryImages = string.Join(",", existingGallery);
                }

                await _productRepository.UpdateAsync(existingProduct);
                await _productRepository.SaveChangesAsync();

                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(product.Id))
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

        ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    // POST: Admin/Product/RemoveGalleryImage
    [HttpPost]
    public async Task<IActionResult> RemoveGalleryImage(int productId, string imageUrl)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
        }

        var galleryImages = product.GalleryImages?.Split(',').ToList() ?? new List<string>();
        var relativePath = imageUrl;

        if (galleryImages.Remove(relativePath))
        {
            product.GalleryImages = string.Join(",", galleryImages);
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            // Xóa file vật lý
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            return Json(new { success = true });
        }

        return Json(new { success = false, message = "Không tìm thấy ảnh" });
    }

    // GET: Admin/Product/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // POST: Admin/Product/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            // Xóa ảnh chính
            if (!string.IsNullOrEmpty(product.MainImageUrl) && product.MainImageUrl != "/images/products/default.png")
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.MainImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Xóa ảnh gallery
            if (!string.IsNullOrEmpty(product.GalleryImages))
            {
                foreach (var img in product.GalleryImages.Split(','))
                {
                    var galleryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.TrimStart('/'));
                    if (System.IO.File.Exists(galleryPath))
                    {
                        System.IO.File.Delete(galleryPath);
                    }
                }
            }

            await _productRepository.DeleteAsync(product);
            await _productRepository.SaveChangesAsync();
            TempData["Success"] = "Xóa sản phẩm thành công!";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ProductExists(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null;
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