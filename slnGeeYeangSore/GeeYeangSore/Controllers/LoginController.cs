using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;
using GeeYeangSore.ViewModels;
using System.Linq;

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
            try
            {
                // 先嘗試根據帳號查詢管理者
                var adminByAccount = _context.HAdmins
                    .FirstOrDefault(a => a.HAccount == vm.txtAccount);

                if (adminByAccount != null)
                {
                    bool isAuthenticated = false;

                    // 1. 檢查是否有鹽值，如果有則使用加鹽哈希驗證
                    if (!string.IsNullOrEmpty(adminByAccount.HSalt))
                    {
                        isAuthenticated = PasswordHasher.VerifyPassword(
                            vm.txtPassword,
                            adminByAccount.HSalt,
                            adminByAccount.HPassword
                        );
                    }
                    // 2. 否則使用直接比對（向後兼容）
                    else if (adminByAccount.HPassword == vm.txtPassword)
                    {
                        isAuthenticated = true;
                    }

                    if (isAuthenticated)
                    {
                        HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, adminByAccount.HAccount);
                        HttpContext.Session.SetString(CDictionary.SK_LOGINED_ROLE, adminByAccount.HRoleLevel);
                        HttpContext.Session.SetString("UserType", "Admin");

                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
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
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"登入失敗: {ex.Message}");
                ViewBag.LoginError = "登入處理發生錯誤，請稍後再試";
                return View();
            }
        }

        // 登出
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
