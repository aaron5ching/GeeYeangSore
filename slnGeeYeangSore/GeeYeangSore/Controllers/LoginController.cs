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

        // AJAX 管理員登入處理
        [HttpPost]
        public IActionResult AdminLogin([FromBody] CLoginViewModel vm)
        {
            try
            {
                // 根據帳號查詢管理者
                var admin = _context.HAdmins
                    .FirstOrDefault(a => a.HAccount == vm.txtAccount);

                if (admin != null)
                {
                    bool isAuthenticated = false;

                    // 1. 檢查是否有鹽值，如果有則使用加鹽哈希驗證
                    if (!string.IsNullOrEmpty(admin.HSalt))
                    {
                        isAuthenticated = PasswordHasher.VerifyPassword(
                            vm.txtPassword,
                            admin.HSalt,
                            admin.HPassword
                        );
                    }
                    // 2. 否則使用直接比對（向後兼容）
                    else if (admin.HPassword == vm.txtPassword)
                    {
                        isAuthenticated = true;
                    }

                    if (isAuthenticated)
                    {
                        HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, admin.HAccount);
                        HttpContext.Session.SetString(CDictionary.SK_LOGINED_ROLE, admin.HRoleLevel);
                        HttpContext.Session.SetString("UserType", "Admin");

                        return Json(new { success = true });
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

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "帳號或密碼錯誤" });
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"AJAX 登入失敗: {ex.Message}");
                return Json(new { success = false, message = "登入處理發生錯誤，請稍後再試" });
            }
        }

        // 登出
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
    }
}
