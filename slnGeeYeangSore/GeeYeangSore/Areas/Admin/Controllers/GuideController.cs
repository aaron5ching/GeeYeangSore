using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class GuideController : SuperController
    {

        private readonly Models.GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public GuideController(Models.GeeYeangSoreContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;

        }
        public IActionResult Index()
        {
            return View();
        }

        //https://localhost:7022/Admin/Guide/Guide
        public IActionResult Guide()
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                //如果沒有權限就會顯示NoPermission頁面
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var contact = _db.HGuides.ToList();
            return View(contact);
        }

        [HttpPost]
        public IActionResult Guide(string HTitle, string HContent, IFormFile Image)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                //如果沒有權限就會顯示NoPermission頁面
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            const string imagePath = "/images/Guide";

            HGuide guide = new HGuide
            {
                HTitle = HTitle,
                HContent = HContent,
                HCreatedAt = DateTime.Now,
                HUpdatedAt = DateTime.Now
            };
            if (Image != null)
            {
                string imageName = Guid.NewGuid() + Path.GetExtension(Image.FileName);

                string rootPath = _env.WebRootPath + imagePath;

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }
                string filePath = Path.Combine(rootPath, imageName);

                string sqlPath = $"wwwroot/Guide/{imageName}";

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Image.CopyTo(fileStream);
                }
                guide.HImagePath = imagePath + "/" + imageName;
            }

            else
            {
                guide.HImagePath = null;
            }
            _db.HGuides.Add(guide);
            _db.SaveChanges();
            return Redirect("Guide");
        }

        [HttpPost]
        public IActionResult DeleteGuide(int HGuideId)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                //如果沒有權限就會顯示NoPermission頁面
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var guide = _db.HGuides.FirstOrDefault(g => g.HGuideId == HGuideId);

            _db.HGuides.Remove(guide);
            _db.SaveChanges();

            TempData["Success"] = "公告刪除成功！";
            return RedirectToAction("Guide");
        }


        [HttpPost]
        public IActionResult UpdateGuide(int HGuideId, string HTitle, string HContent, IFormFile Image)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                //如果沒有權限就會顯示NoPermission頁面
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var guide = _db.HGuides.FirstOrDefault(g => g.HGuideId == HGuideId);
            if (guide == null)
            {
                TempData["Error"] = "找不到該公告";
                return RedirectToAction("Guide");
            }

     
            guide.HTitle = HTitle;
            guide.HContent = HContent;

            if (Image != null)
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", Image.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    Image.CopyTo(stream);
                }
                guide.HImagePath = $"/images/{Image.FileName}";
            }

            guide.HUpdatedAt = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "公告更新成功！";
            return RedirectToAction("Guide");
        }


    }
}
