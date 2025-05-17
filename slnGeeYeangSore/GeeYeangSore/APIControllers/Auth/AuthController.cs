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
            if (!VerifyTenantPassword(tenant, vm.txtPassword))
                return Unauthorized(new { success = false, message = "密碼錯誤" });

            // 判斷是房東還是房客
            bool isLandlord = tenant.HIsLandlord;
            string role;
            if (isLandlord)
            {
                role = "landlord";
            }
            else
            {
                role = "tenant";
            }

            // 登入成功時寫入 Session
            SessionManager.SetLogin(HttpContext, tenant);

            // 回傳登入成功資料
            return Ok(new
            {
                success = true,
                user = tenant.HEmail,
                userName = tenant.HUserName,
                tenantId = tenant.HTenantId,
                role = role,
                isLandlord = tenant.HIsLandlord
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
                if (!SessionManager.IsLoggedIn(HttpContext))
                    return Unauthorized(new { success = false, message = "未登入" });
                // 從 Session 取得登入的 Email
                var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);


                // 查找租客資料
                var tenant = _db.HTenants.FirstOrDefault(t => t.HEmail == email && !t.HIsDeleted);
                if (tenant == null)
                {
                    return NotFound(new { success = false, message = "找不到該使用者" });
                }

                // 判斷是房東還是房客
                bool isLandlord = tenant.HIsLandlord;
                string role;
                if (isLandlord)
                {
                    role = "landlord";
                }
                else
                {
                    role = "tenant";
                }

                // 回傳使用者資訊
                return Ok(new
                {
                    success = true,
                    user = tenant.HEmail,
                    userName = tenant.HUserName,
                    tenantId = tenant.HTenantId,
                    role = role,
                    isLandlord = tenant.HIsLandlord
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器錯誤", error = ex.Message });
            }
        }
    }
}
