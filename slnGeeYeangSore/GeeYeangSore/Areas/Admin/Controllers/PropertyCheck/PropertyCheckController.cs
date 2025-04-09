using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
using System.IO;

namespace GeeYeangSore.Areas.Admin.Controllers.PropertyCheck
{
    // GET:https://localhost:7022/Admin/PropertyCheck/Index
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]

    public class PropertyCheckController : Controller
    {
        private readonly GeeYeangSoreContext _context;

        public PropertyCheckController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(int page = 1, string searchId = "", string searchRent = "")
        {
            int pageSize = 15; // 每頁15筆資料

            var query = _context.HProperties
                .Include(p => p.HLandlord)
                .Where(p => p.HStatus == "未驗證");

            // 搜尋房源ID
            if (!string.IsNullOrEmpty(searchId))
            {
                if (int.TryParse(searchId, out int propertyId))
                {
                    query = query.Where(p => p.HPropertyId == propertyId);
                }
            }

            // 搜尋租金
            if (!string.IsNullOrEmpty(searchRent))
            {
                if (decimal.TryParse(searchRent, out decimal rent))
                {
                    query = query.Where(p => p.HRentPrice == rent);
                }
            }

            var properties = await query
                .OrderByDescending(p => p.HPublishedDate)
                .ToListAsync();

            ViewBag.SearchId = searchId;
            ViewBag.SearchRent = searchRent;

            return View(properties.ToPagedList(page, pageSize));
        }

        // GET: Admin/PropertyCheck/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.HProperties
                .Include(p => p.HLandlord)
                .Include(p => p.HPropertyFeatures)
                .Include(p => p.HPropertyImages)
                .FirstOrDefaultAsync(m => m.HPropertyId == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: Admin/PropertyCheck/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var property = await _context.HProperties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            property.HStatus = "已驗證";
            _context.Update(property);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/PropertyCheck/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.HProperties
                .Include(p => p.HPropertyImages)
                .FirstOrDefaultAsync(p => p.HPropertyId == id);

            if (property != null)
            {
                // 刪除相關的圖片檔案
                foreach (var image in property.HPropertyImages)
                {
                    if (!string.IsNullOrEmpty(image.HImageUrl))
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Property", Path.GetFileName(image.HImageUrl));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                _context.HProperties.Remove(property);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
