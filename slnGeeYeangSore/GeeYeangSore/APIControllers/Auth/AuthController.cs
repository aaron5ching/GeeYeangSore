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
            // 驗證輸入
            if (string.IsNullOrEmpty(vm.txtAccount) || string.IsNullOrEmpty(vm.txtPassword))
            {
                return BadRequest(new { success = false, message = "帳號或密碼為空" });
            }

            // 找出帳號
            var tenant = _db.HTenants.FirstOrDefault(t => t.HEmail == vm.txtAccount && !t.HIsDeleted);
            if (tenant == null)
                return Unauthorized(new { success = false, message = "查無此帳號" });

            // 驗證密碼
            if (!PasswordHasher.VerifyPassword(vm.txtPassword, tenant.HSalt, tenant.HPassword))
                return Unauthorized(new { success = false, message = "密碼錯誤" });

            // 判斷是否為房東
            bool isLandlord = _db.HLandlords.Any(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);

            // 判斷角色
            string role = isLandlord && tenant.HIsTenant ? "both"
                        : isLandlord ? "landlord"
                        : "tenant";

            // 寫入 Session
            HttpContext.Session.SetString("SK_LOGINED_USER", tenant.HEmail);

            // 回傳登入成功資料
            return Ok(new
            {
                success = true,
                user = tenant.HEmail,
                userName = tenant.HUserName,
                tenantId = tenant.HTenantId,
                role = role
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
            try
            {
                // 從 Session 取得登入的 Email
                var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { success = false, message = "未登入" });
                }

                // 查找租客資料
                var tenant = _db.HTenants.FirstOrDefault(t => t.HEmail == email && !t.HIsDeleted);
                if (tenant == null)
                {
                    return NotFound(new { success = false, message = "找不到該使用者" });
                }

                // 判斷是否為房東
                bool isLandlord = _db.HLandlords.Any(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                string role = isLandlord && tenant.HIsTenant ? "both"
                            : isLandlord ? "landlord"
                            : "tenant";

                // 回傳使用者資訊
                return Ok(new
                {
                    success = true,
                    user = tenant.HEmail,
                    userName = tenant.HUserName,
                    tenantId = tenant.HTenantId,
                    role = role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器錯誤", error = ex.Message });
            }
        }
    }
}
