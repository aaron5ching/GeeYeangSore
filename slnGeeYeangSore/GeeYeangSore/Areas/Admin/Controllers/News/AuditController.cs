using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Areas.Admin.Controllers.News
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class AuditController : SuperController
    {
        private readonly GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public AuditController(GeeYeangSoreContext db, IWebHostEnvironment env)
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

        //https://localhost:7022/Admin/Audit/Audit
        public IActionResult Audit()
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var contact = _db.HAudits.ToList();
                return View(contact);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"審核頁面載入失敗: {ex.Message}");
                TempData["Error"] = "載入資料時發生錯誤，請稍後再試。";
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        [HttpPost]
        public IActionResult Audit(int HAuditId, string typeString)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var audit = _db.HAudits.FirstOrDefault(item => item.HAuditId == HAuditId);
                if (audit != null)
                {
                    Console.WriteLine($"Old: {audit.HStatus}, New: {typeString}");
                    audit.HStatus = typeString;
                    audit.HReviewedAt = DateTime.Now;
                    _db.SaveChanges();
                    TempData["Success"] = "審核狀態更新成功！";
                }
                else
                {
                    TempData["Error"] = "找不到要審核的申請。";
                }

                var contact = _db.HAudits.ToList();
                return View(contact);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"更新審核狀態失敗: {ex.Message}");
                TempData["Error"] = "更新審核狀態時發生錯誤，請稍後再試。";
                return RedirectToAction("Audit");
            }
        }

        //[HttpPost]
        //public IActionResult AcceptApply(int )
        //{

        //}
        //[HttpPost]
        //public IActionResult RejectApply()
        //{

        //}
    }
}