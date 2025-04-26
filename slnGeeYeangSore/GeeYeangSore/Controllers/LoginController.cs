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
            // 查詢管理者帳號 - 直接用帳號密碼字串比對方式
            var admin = _context.HAdmins
                .FirstOrDefault(a => a.HAccount == vm.txtAccount && a.HPassword == vm.txtPassword);

            // 如果直接比對找不到，則可能是密碼已經被加密，先找到帳號然後進行驗證
            if (admin == null)
            {
                var adminByAccount = _context.HAdmins
                    .FirstOrDefault(a => a.HAccount == vm.txtAccount);

                // 如果找到了帳號，並且有鹽值，嘗試進行密碼驗證
                if (adminByAccount != null && !string.IsNullOrEmpty(adminByAccount.HSalt))
                {
                    // 暫時註解掉密碼驗證，以便用戶能先登入系統新增管理者
                    /*
                    bool isPasswordValid = PasswordHasher.VerifyPassword(
                        vm.txtPassword, 
                        adminByAccount.HSalt,
                        adminByAccount.HPassword
                    );

                    if (isPasswordValid)
                    {
                        admin = adminByAccount;
                    }
                    */
                }
            }

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
