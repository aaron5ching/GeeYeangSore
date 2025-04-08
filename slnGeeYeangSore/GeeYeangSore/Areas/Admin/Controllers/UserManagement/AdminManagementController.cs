using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models; // ✅ 加入 Models 命名空間

namespace GeeYeangSore.Areas.Admin.Controllers.UserManagement
{
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")] // ✅ 補上路由方便維護
    public class AdminManagementController : Controller
    {
        private readonly GeeYeangSoreContext _context; // ✅ 注入 DbContext

        public AdminManagementController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        // 🐥 管理者列表主頁（可依帳號搜尋）
        public IActionResult AdminManagement(string? searchAccount = null)
        {
            // 🐥 撈取所有管理員資料
            var admins = _context.HAdmins
                .Where(a => string.IsNullOrEmpty(searchAccount) || a.HAccount.Contains(searchAccount))
                .OrderByDescending(a => a.HCreatedAt) // 依建立時間排序
                .ToList();

            // 🐥 傳送資料至主頁
            return View("~/Areas/Admin/Views/User/AdminManagement.cshtml", admins);
        }
    }
}
