using System.Runtime.InteropServices.JavaScript;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.Areas.Admin.Controllers

{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class AboutController : Controller
    {

        private readonly Models.GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public AboutController(Models.GeeYeangSoreContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;

        }

        public IActionResult Index()
        {
            return View();
        }



        //https://localhost:7022/Admin/About/About
        public IActionResult About()
        {
            var news = _db.HAbouts.ToList();

            return View(news);
        }

        [HttpPost]
        public IActionResult About(string HTitle, string HContent, IFormFile image)
        {
            HAbout news = new HAbout
            {
                HTitle = HTitle,
                HContent = HContent,
                HCreatedAt = DateTime.Now,
                HUpdatedAt = DateTime.Now
            };
            _db.HAbouts.Add(news);
            _db.SaveChanges();
            return RedirectToAction("About");

        }
        [HttpPost]
        public IActionResult UpdateAbout(int HAboutId, string HTitle, string HContent)
        {

            var about = _db.HAbouts.FirstOrDefault(item => item.HAboutId == HAboutId);

            if (about != null)
            {
                about.HTitle = HTitle;
                about.HContent = HContent;
                about.HUpdatedAt = DateTime.Now;


                _db.SaveChanges();
                TempData["Success"] = "公告修改成功！";
            }

            return RedirectToAction("About");
        }
        [HttpPost]
        public IActionResult DeleteAbout(int HAboutId)
        {

            var about = _db.HAbouts.FirstOrDefault(item => item.HAboutId == HAboutId);
            _db.Remove(about);
            _db.SaveChanges();
            TempData["Success"] = "公告刪除成功！";
            return RedirectToAction("About");
        }

    }
    
}
