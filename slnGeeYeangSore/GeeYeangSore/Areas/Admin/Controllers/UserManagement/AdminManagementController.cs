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
        [IgnoreAntiforgeryToken]
        public IActionResult Create([FromBody] HAdmin newAdmin)
        {

            // ✅ 解法一：從 Session 取出帳號再查詢資料
            var currentAccount = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            var currentAdmin = _context.HAdmins.FirstOrDefault(a => a.HAccount == currentAccount);

            // ✅ 權限驗證
            if (currentAdmin?.HRoleLevel != "超級管理員")
                return Forbid(); // 🔐 拒絕非超級管理員新增帳號

            // ✅ 基本欄位驗證
            if (string.IsNullOrWhiteSpace(newAdmin.HAccount) || string.IsNullOrWhiteSpace(newAdmin.HPassword))
                return BadRequest("帳號或密碼不得為空");

            newAdmin.HCreatedAt = DateTime.Now;
            newAdmin.HUpdateAt = DateTime.Now;

            _context.HAdmins.Add(newAdmin);
            _context.SaveChanges();

            return Ok();
        }

        // GET：載入編輯表單
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var admin = _context.HAdmins.Find(id);
            if (admin == null)
                return NotFound();

            return PartialView("~/Areas/Admin/Partials/UserManagement/_EditAdminPartial.cshtml", admin);
        }

        // POST：接收編輯結果
        [HttpPost]
        public IActionResult Edit([FromBody] HAdmin edited)
        {
            var admin = _context.HAdmins.Find(edited.HAdminId);
            if (admin == null)
                return NotFound();

            admin.HAccount = edited.HAccount;

            // 密碼不為空才更新
            if (!string.IsNullOrWhiteSpace(edited.HPassword))
                admin.HPassword = edited.HPassword;

            admin.HRoleLevel = edited.HRoleLevel;
            admin.HUpdateAt = DateTime.Now;

            _context.SaveChanges();
            return Ok("success");
        }


        // ✅ POST：刪除管理員
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Delete(int id)
        {
            // 取得目前登入者帳號
            var currentAccount = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            var currentAdmin = _context.HAdmins.FirstOrDefault(a => a.HAccount == currentAccount);

            // ✅ 僅限超級管理員執行刪除
            if (currentAdmin?.HRoleLevel != "超級管理員")
                return Forbid();

            // ✅ 找到要刪除的對象
            var admin = _context.HAdmins.Find(id);
            if (admin == null)
                return NotFound();

            // ✅ 禁止刪除超級管理員帳號（保護機制）
            if (admin.HRoleLevel == "超級管理員")
                return BadRequest("不可刪除超級管理員");

            _context.HAdmins.Remove(admin);
            _context.SaveChanges();

            return Ok("deleted");
        }


    }
}
