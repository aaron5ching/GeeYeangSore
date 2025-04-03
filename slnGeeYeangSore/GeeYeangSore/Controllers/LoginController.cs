using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;
using GeeYeangSore.ViewModels;

namespace GeeYeangSore.Controllers
{
    public class LoginController : Controller
    {
        private readonly GeeYeangSoreContext _context;

        public LoginController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        // 顯示登入頁面
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 登入
        [HttpPost]
        public IActionResult Login(CLoginViewModel vm)
        {
            // 查詢管理者帳號
            var admin = _context.HAdmins
                .FirstOrDefault(a => a.HAccount == vm.txtAccount && a.HPassword == vm.txtPassword);

            if (admin != null)
            {
                HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, admin.HAccount);
                HttpContext.Session.SetString(CDictionary.SK_LOGINED_ROLE, admin.HRoleLevel);
                HttpContext.Session.SetString("UserType", "Admin");

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // 查詢前台租客帳號
            var tenant = _context.HTenants
                .FirstOrDefault(t => t.HUserName == vm.txtAccount && t.HPassword == vm.txtPassword);

            if (tenant != null)
            {
                HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, tenant.HUserName);
                HttpContext.Session.SetString(CDictionary.SK_LOGINED_ROLE, "User");
                HttpContext.Session.SetString("UserType", "Tenant");

                return RedirectToAction("Index", "Home"); // 前台頁面
            }

            ViewBag.LoginError = "帳號或密碼錯誤";
            return View();
        }

        // 登出
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
