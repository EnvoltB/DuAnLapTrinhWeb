using GearDTK.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NguyenThang.Controllers
{
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> FAQ()
        {
            var faqs = await _context.FAQs
                .Where(x => x.IsActive)
                .OrderBy(x => x.Category)
                .ToListAsync();

            return View(faqs);
        }
    }
}