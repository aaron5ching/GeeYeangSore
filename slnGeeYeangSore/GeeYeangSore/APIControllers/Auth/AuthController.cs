using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;
using GeeYeangSore.Data;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using GeeYeangSore.ViewModels;
using GeeYeangSore.APIControllers.Session;

namespace GeeYeangSore.APIControllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        public AuthController(GeeYeangSoreContext db) : base(db) { }

        // 登入
        [HttpPost("login")]
        public IActionResult Login([FromBody] CLoginViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.txtAccount))
                return BadRequest(new { success = false, message = "請輸入帳號（Email）" });
            if (string.IsNullOrWhiteSpace(vm.txtPassword))
                return BadRequest(new { success = false, message = "請輸入密碼" });

            var tenant = _db.HTenants.FirstOrDefault(t => t.HEmail == vm.txtAccount && !t.HIsDeleted);
            if (tenant == null)
                return Unauthorized(new { success = false, message = "查無此帳號（Email）" });

            // 檢查黑名單
            if (IsBlacklisted())
            {
                return new JsonResult(new { success = false, message = "此帳號已被停權" }) { StatusCode = StatusCodes.Status403Forbidden };
            }

            // 登入失敗次數鎖定
            if (tenant.HLoginFailCount >= 5 && tenant.HLastLoginAt.HasValue && tenant.HLastLoginAt.Value.AddMinutes(15) > DateTime.UtcNow)
            {
                return new JsonResult(new { success = false, message = "錯誤次數過多，請稍後再試" }) { StatusCode = StatusCodes.Status403Forbidden };
            }

            if (!VerifyTenantPassword(tenant, vm.txtPassword))
            {
                tenant.HLoginFailCount++;
                tenant.HLastLoginAt = DateTime.UtcNow;
                _db.SaveChanges();
                return Unauthorized(new { success = false, message = "密碼錯誤" });
            }

            // 驗證成功，重設失敗次數
            tenant.HLoginFailCount = 0;
            tenant.HLastLoginAt = DateTime.UtcNow;
            _db.SaveChanges();

            // 寫入Session（用SessionHelper）
            SessionManager.SetLogin(HttpContext, tenant);

            return Ok(new
            {
                success = true,
                user = tenant.HEmail,
                role = "User",
                tenantId = tenant.HTenantId,
                userName = tenant.HUserName,
                email = tenant.HEmail
            });
        }

        // 登出
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            if (!IsLoggedIn())
            {
                return Unauthorized(new { success = false, message = "尚未登入" });
            }
            SessionManager.Clear(HttpContext);
            return Ok(new { success = true, message = "登出成功" });
        }

        // 密碼驗證封裝
        private bool VerifyTenantPassword(HTenant tenant, string inputPassword)
        {
            return !string.IsNullOrEmpty(tenant.HSalt) && PasswordHasher.VerifyPassword(inputPassword, tenant.HSalt, tenant.HPassword);
        }

        // 取得目前登入用戶
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(email))
            {
                return Ok(new { success = false, message = "未登入" });
            }
            var tenant = _db.HTenants.FirstOrDefault(t => t.HEmail == email && !t.HIsDeleted);
            if (tenant == null)
            {
                return Ok(new { success = false, message = "用戶不存在" });
            }
            return Ok(new
            {
                success = true,
                user = tenant.HEmail,
                role = "User",
                tenantId = tenant.HTenantId,
                userName = tenant.HUserName,
                email = tenant.HEmail
            });
        }
    }
}
