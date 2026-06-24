using GearDTK.Data;
using GearDTK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace NguyenThang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FAQController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.FAQs.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                _context.Add(faq);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(faq);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);

            if (faq == null)
                return NotFound();

            return View(faq);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                _context.Update(faq);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(faq);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);

            if (faq != null)
            {
                _context.FAQs.Remove(faq);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}