using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Areas.Admin.Controllers.News
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class ContactController : SuperController
    {
        private readonly GeeYeangSoreContext _db;

        public ContactController(GeeYeangSoreContext db)
        {
            _db = db;
            //0
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

        //https://localhost:7022/Admin/Contact/Contact
        public IActionResult Contact()
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var contact = _db.HContacts.ToList();
                return View(contact);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"聯絡我們頁面載入失敗: {ex.Message}");
                TempData["Error"] = "載入資料時發生錯誤，請稍後再試。";
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        [HttpPost]
        public IActionResult Contact(int HContactId, string HReplyContent)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                Console.WriteLine("replay");

                var contact = _db.HContacts.FirstOrDefault(n => n.HContactId == HContactId);

                if (contact != null)
                {
                    contact.HReplyAt = DateTime.Now;
                    contact.HReplyContent = HReplyContent;
                    contact.HStatus = true;

                    _db.SaveChanges();
                    TempData["Success"] = "回覆訊息成功！";
                }
                else
                {
                    TempData["Error"] = "找不到要回覆的訊息。";
                }

                //確保不為model 不為null
                var contacts = _db.HContacts?.ToList() ?? new List<HContact>();
                return View(contacts);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"回覆訊息失敗: {ex.Message}");
                TempData["Error"] = "回覆訊息時發生錯誤，請稍後再試。";
                return RedirectToAction("Contact");
            }
        }
    }
}