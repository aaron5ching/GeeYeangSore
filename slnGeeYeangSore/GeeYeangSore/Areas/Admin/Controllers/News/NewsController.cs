using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using GeeYeangSore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Areas.Admin.Controllers.News
{

    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]

    public class NewsController : SuperController
    {
        private readonly GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public NewsController(GeeYeangSoreContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;

        }

        //https://localhost:7022/Admin/News/News
        public IActionResult News()
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                //如果沒有權限就會顯示NoPermission頁面
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var news = _db.HNews.ToList();
            return View(news);
        }

        [HttpPost]
        public async Task<IActionResult> Update(IFormCollection form)
        {
            if (!int.TryParse(form["id"], out int newsId))
                return Json(new { success = false, message = "無效的 ID" });

            var news =  _db.HNews.FirstOrDefault(n => n.HNewsId == newsId);
            if (news == null)
                return Json(new { success = false, message = "找不到公告" });

            // 更新文字欄位
            news.HTitle = form["HTitle"];
            news.HContent = form["HContent"];
            news.HUpdatedAt = DateTime.Now;

            // 處理圖片上傳
            var file = form.Files["HImagePath"];
            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "news");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // 儲存相對路徑到資料庫
                news.HImagePath = $"/uploads/news/{uniqueFileName}";
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }



        //[HttpPost]
        //public IActionResult DeleteNews(int HNewsId)
        //{

        //}


        [HttpPost]
        public IActionResult UpdateNews(int HNewsId, string HContent, IFormFile image, string type)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                //如果沒有權限就會顯示NoPermission頁面
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            Console.WriteLine("TEST!");
            var contact = _db.HNews.FirstOrDefault(n => n.HNewsId == HNewsId);
            if (type == "修改文章")
            {


                if (contact != null)
                {

                    contact.HContent = HContent;
                    contact.HUpdatedAt = DateTime.Now;

                    if (image != null)
                    {

                        var filePath = Path.Combine("wwwroot", "images", image.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }

                        contact.HImagePath = filePath;
                    }


                }

            }
            else
            {
                _db.Remove(contact);

            }

            _db.SaveChanges();

            return Redirect("News");

        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] HNews model, IFormFile? HImagePath)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "表單驗證失敗" });
            }

            // 🔽 處理圖片
            if (HImagePath != null && HImagePath.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/news");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(HImagePath.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await HImagePath.CopyToAsync(stream);
                }

                model.HImagePath = "/uploads/news/" + uniqueFileName;
            }

            // 🔽 設定時間
            model.HCreatedAt = DateTime.Now;
            model.HUpdatedAt = DateTime.Now;

            // 🔽 存入資料庫
            _db.HNews.Add(model);
            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }


        //以下已棄用
        // [HttpPost]
        // public IActionResult News(string HTitle, string HContent, IFormFile image)
        // {
        //     if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
        //         //如果沒有權限就會顯示NoPermission頁面
        //         return Redirect("News");
        //
        //
        //     HNews news = new HNews
        //     {
        //         HTitle = HTitle,
        //         HContent = HContent,
        //         //HImagePath = imagePath +"\\"+ imageName,
        //         HCreatedAt = DateTime.Now,
        //         HUpdatedAt = DateTime.Now
        //     };
        //
        //     if (image != null)
        //     {
        //         const string imagePath = "/images/News";
        //         string imageName = Guid.NewGuid() + Path.GetExtension(image.FileName);
        //
        //         string rootPath = _env.WebRootPath + imagePath;
        //
        //         if (!Directory.Exists(rootPath))
        //         {
        //             Directory.CreateDirectory(rootPath);
        //         }
        //
        //         string filePath = Path.Combine(rootPath, imageName);
        //
        //         string sqlPath = $"wwwroot/News/{imageName}";
        //
        //         if (image != null)
        //         {
        //             using (var fileStream = new FileStream(filePath, FileMode.Create))
        //             {
        //                 image.CopyTo(fileStream);
        //             }
        //         }
        //         news.HImagePath = imagePath + "/" + imageName;
        //     }
        //
        //     _db.HNews.Add(news);
        //     _db.SaveChanges();
        //     return Redirect("News");
        // }

        //https://localhost:7022/Admin/News/Test

        // public IActionResult Index()
        // {
        //     return View();
        // }
    }
}
