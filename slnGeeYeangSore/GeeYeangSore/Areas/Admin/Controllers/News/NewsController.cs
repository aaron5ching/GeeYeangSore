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
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var news = _db.HNews.ToList();
                return View(news);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"系統公告頁面載入失敗: {ex.Message}");
                TempData["Error"] = "載入資料時發生錯誤，請稍後再試。";
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(IFormCollection form)
        {
            try
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
                    try
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "news");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        // 如果有舊圖片，先刪除
                        if (!string.IsNullOrEmpty(news.HImagePath))
                        {
                            var oldImagePath = Path.Combine(_env.WebRootPath, news.HImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // 儲存相對路徑到資料庫
                        news.HImagePath = $"/uploads/news/{uniqueFileName}";
                    }
                    catch (Exception ex)
                    {
                        // 記錄圖片處理錯誤
                        Console.WriteLine($"更新圖片失敗: {ex.Message}");
                        return Json(new { success = false, message = "圖片更新失敗，請稍後再試" });
                    }
                }

                await _db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"更新公告失敗: {ex.Message}");
                return Json(new { success = false, message = "更新資料時發生錯誤，請稍後再試" });
            }
        }



        //[HttpPost]
        //public IActionResult DeleteNews(int HNewsId)
        //{

        //}


        [HttpPost]
        public IActionResult UpdateNews(int HNewsId, string HContent, IFormFile image, string type)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                Console.WriteLine("TEST!");
                var contact = _db.HNews.FirstOrDefault(n => n.HNewsId == HNewsId);
                if (contact == null)
                {
                    TempData["Error"] = "找不到要更新的公告。";
                    return RedirectToAction("News");
                }

                if (type == "修改文章")
                {
                    contact.HContent = HContent;
                    contact.HUpdatedAt = DateTime.Now;

                    if (image != null)
                    {
                        try
                        {
                            // 如果有舊圖片，先刪除
                            if (!string.IsNullOrEmpty(contact.HImagePath))
                            {
                                var oldImagePath = Path.Combine(_env.WebRootPath, contact.HImagePath.TrimStart('/'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            var filePath = Path.Combine(_env.WebRootPath, "images", image.FileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                image.CopyTo(stream);
                            }

                            contact.HImagePath = $"/images/{image.FileName}";
                        }
                        catch (Exception ex)
                        {
                            // 記錄圖片處理錯誤
                            Console.WriteLine($"更新圖片失敗: {ex.Message}");
                            TempData["Error"] = "圖片更新失敗，請稍後再試。";
                            return RedirectToAction("News");
                        }
                    }

                    TempData["Success"] = "公告更新成功！";
                }
                else
                {
                    _db.Remove(contact);
                    TempData["Success"] = "公告刪除成功！";
                }

                _db.SaveChanges();
                return RedirectToAction("News");
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"更新/刪除公告失敗: {ex.Message}");
                TempData["Error"] = "操作失敗，請稍後再試。";
                return RedirectToAction("News");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] HNews model, IFormFile? HImagePath)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "表單驗證失敗" });
                }

                // 🔽 處理圖片
                if (HImagePath != null && HImagePath.Length > 0)
                {
                    try
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
                    catch (Exception ex)
                    {
                        // 記錄圖片處理錯誤
                        Console.WriteLine($"上傳圖片失敗: {ex.Message}");
                        return Json(new { success = false, message = "圖片上傳失敗，請稍後再試" });
                    }
                }

                // 🔽 設定時間
                model.HCreatedAt = DateTime.Now;
                model.HUpdatedAt = DateTime.Now;

                // 🔽 存入資料庫
                _db.HNews.Add(model);
                await _db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"新增公告失敗: {ex.Message}");
                return Json(new { success = false, message = "新增資料時發生錯誤，請稍後再試" });
            }
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
