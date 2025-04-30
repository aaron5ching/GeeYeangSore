using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Areas.Admin.Controllers.News
{

    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class GuideController : SuperController
    {

        private readonly GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public GuideController(GeeYeangSoreContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;

        }
        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"Index 頁面載入失敗: {ex.Message}");
                return RedirectToAction("Error", "Home", new { area = "Admin" });
            }
        }

        //https://localhost:7022/Admin/Guide/Guide
        public IActionResult Guide()
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var contact = _db.HGuides.ToList();
                return View(contact);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"指南手冊頁面載入失敗: {ex.Message}");
                TempData["Error"] = "載入資料時發生錯誤，請稍後再試。";
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        [HttpPost]
        public IActionResult Guide(string HTitle, string HContent, IFormFile Image)
        {
            try
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
                    try
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
                    catch (Exception ex)
                    {
                        // 記錄圖片處理錯誤
                        Console.WriteLine($"圖片處理失敗: {ex.Message}");
                        TempData["Error"] = "圖片上傳失敗，請稍後再試。";
                        return RedirectToAction("Guide");
                    }
                }

                else
                {
                    guide.HImagePath = null;
                }
                _db.HGuides.Add(guide);
                _db.SaveChanges();
                TempData["Success"] = "新增指南手冊成功！";
                return RedirectToAction("Guide");
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"新增指南手冊失敗: {ex.Message}");
                TempData["Error"] = "新增資料時發生錯誤，請稍後再試。";
                return RedirectToAction("Guide");
            }
        }

        [HttpPost]
        public IActionResult DeleteGuide(int HGuideId)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var guide = _db.HGuides.FirstOrDefault(g => g.HGuideId == HGuideId);

                if (guide != null)
                {
                    // 如果有圖片，嘗試刪除圖片文件
                    if (!string.IsNullOrEmpty(guide.HImagePath))
                    {
                        try
                        {
                            var imagePath = Path.Combine(_env.WebRootPath, guide.HImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            // 記錄圖片刪除錯誤，但繼續執行
                            Console.WriteLine($"刪除圖片文件失敗: {ex.Message}");
                        }
                    }

                    _db.HGuides.Remove(guide);
                    _db.SaveChanges();
                    TempData["Success"] = "指南手冊刪除成功！";
                }
                else
                {
                    TempData["Error"] = "找不到要刪除的指南手冊。";
                }

                return RedirectToAction("Guide");
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"刪除指南手冊失敗: {ex.Message}");
                TempData["Error"] = "刪除資料時發生錯誤，請稍後再試。";
                return RedirectToAction("Guide");
            }
        }


        [HttpPost]
        public IActionResult UpdateGuide(int HGuideId, string HTitle, string HContent, IFormFile Image)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var guide = _db.HGuides.FirstOrDefault(g => g.HGuideId == HGuideId);
                if (guide == null)
                {
                    TempData["Error"] = "找不到要更新的指南手冊。";
                    return RedirectToAction("Guide");
                }

     
                guide.HTitle = HTitle;
                guide.HContent = HContent;

                if (Image != null)
                {
                    try
                    {
                        // 如果有舊圖片，先刪除
                        if (!string.IsNullOrEmpty(guide.HImagePath))
                        {
                            var oldImagePath = Path.Combine(_env.WebRootPath, guide.HImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // 保存新圖片
                        var imagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            Image.CopyTo(stream);
                        }
                        guide.HImagePath = $"/images/{Image.FileName}";
                    }
                    catch (Exception ex)
                    {
                        // 記錄圖片處理錯誤
                        Console.WriteLine($"更新圖片失敗: {ex.Message}");
                        TempData["Error"] = "圖片更新失敗，請稍後再試。";
                        return RedirectToAction("Guide");
                    }
                }

                guide.HUpdatedAt = DateTime.Now;

                _db.SaveChanges();

                TempData["Success"] = "指南手冊更新成功！";
                return RedirectToAction("Guide");
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"更新指南手冊失敗: {ex.Message}");
                TempData["Error"] = "更新資料時發生錯誤，請稍後再試。";
                return RedirectToAction("Guide");
            }
        }


    }
}
