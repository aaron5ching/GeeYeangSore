using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models; // ✅ 加入 Models 命名空間
using GeeYeangSore.Controllers; // ✅ 加上這行


namespace GeeYeangSore.Areas.Admin.Controllers.UserManagement
{
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")] // ✅ 補上路由方便維護
    public class AdminManagementController : SuperController
    {
        private readonly GeeYeangSoreContext _context; // ✅ 注入 DbContext

        public AdminManagementController(GeeYeangSoreContext context)
        {
            _context = context;

        }

        // 🐥 管理者列表主頁（可依帳號搜尋）
        public IActionResult AdminManagement(string? searchAccount = null)
        {
            if (!HasAnyRole("超級管理員", "系統管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });


            // 🐥 撈取所有管理員資料
            var admins = _context.HAdmins
                .Where(a => string.IsNullOrEmpty(searchAccount) || a.HAccount.Contains(searchAccount))
                .OrderBy(a => a.HAdminId)
                .ToList();

            ViewBag.IsSuperAdmin = HasAnyRole("超級管理員"); // ✅ 傳給 View 用

            return View("~/Areas/Admin/Views/User/AdminManagement.cshtml", admins);
        }




        // ✅ GET：顯示新增表單
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("~/Areas/Admin/Partials/UserManagement/_CreateAdminPartial.cshtml", new HAdmin());
        }

        // ✅ POST：接收建立表單
        [HttpPost]
        public IActionResult Create([FromBody] HAdmin newAdmin)
        {
            // 取得目前登入的管理員 ID（假設你用 Session 儲存）
            var currentAdminId = HttpContext.Session.GetInt32("AdminId");

            // 撈出登入者資料
            var currentAdmin = _context.HAdmins.FirstOrDefault(a => a.HAdminId == currentAdminId);

            // 權限驗證
            if (currentAdmin?.HRoleLevel != "超級管理員")
                return Forbid(); // 🔐 拒絕非超級管理員新增帳號

            // 基本欄位驗證
            if (string.IsNullOrWhiteSpace(newAdmin.HAccount) || string.IsNullOrWhiteSpace(newAdmin.HPassword))
            {
                return BadRequest("帳號或密碼不得為空");
            }

            newAdmin.HCreatedAt = DateTime.Now;
            newAdmin.HUpdateAt = DateTime.Now;

            _context.HAdmins.Add(newAdmin);
            _context.SaveChanges();

            return Ok();
        }



    }
}
