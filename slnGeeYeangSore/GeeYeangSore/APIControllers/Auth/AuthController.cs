using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore; // ✅ 若有使用 .Include()
using GeeYeangSore.Data;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using GeeYeangSore.ViewModels;
using GeeYeangSore.APIControllers.Session;
using GeeYeangSore.DTO.User;
using Google.Apis.Auth; //第三方登入



namespace GeeYeangSore.APIControllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        public AuthController(GeeYeangSoreContext db) : base(db) { }

        // 登入
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CLoginViewModel vm)
        {
            // Step 0：驗證欄位是否為空
            if (string.IsNullOrEmpty(vm.txtAccount) || string.IsNullOrEmpty(vm.txtPassword))
            {
                return BadRequest(new { success = false, message = "帳號或密碼為空" });
            }

            if (string.IsNullOrEmpty(vm.RecaptchaToken))
            {
                return BadRequest(new { success = false, message = "reCAPTCHA token 缺失" });
            }

            // 🛡️ Step 1：驗證 reCAPTCHA Token
            if (!await VerifyRecaptchaAsync(vm.RecaptchaToken))
            {
                return Unauthorized(new { success = false, message = "reCAPTCHA 驗證失敗" });
            }

            // Step 2：查詢帳號
            var tenant = _db.HTenants.FirstOrDefault(t => t.HEmail == vm.txtAccount && !t.HIsDeleted);
            if (tenant == null)
                return Unauthorized(new { success = false, message = "查無此帳號" });

            // Step 3：密碼驗證
            if (!VerifyTenantPassword(tenant, vm.txtPassword))
                return Unauthorized(new { success = false, message = "密碼錯誤" });

            // Step 4：判斷角色
            bool isLandlord = tenant.HIsLandlord;
            string role = isLandlord ? "landlord" : "tenant";

            // Step 5：寫入 Session
            SessionManager.SetLogin(HttpContext, tenant);

            // Step 6：回傳成功資訊
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


        [HttpPost("google-login")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest request)
        {
            try
            {
                // Step 1️⃣ 驗證 id_token 的有效性
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);

                // Step 2️⃣ 檢查 email 是否已驗證
                if (!payload.EmailVerified)
                    return Unauthorized(new { success = false, message = "Google 帳號尚未完成 Email 驗證" });

                // Step 3️⃣ 查詢是否已存在對應的 HSso 紀錄
                var aud = (payload.Audience as IEnumerable<string>)?.FirstOrDefault() ?? "";
                var sso = _db.HSsos
                    .Include(s => s.HTenant)
                    .FirstOrDefault(s => s.HSub == payload.Subject && s.HAud == aud);

                HTenant tenant;

                if (sso != null)
                {
                    // ✅ 已存在 → 更新 EmailVerified、HIat、HExp
                    tenant = sso.HTenant;

                    sso.HEmailverified = payload.EmailVerified;
                    sso.HIat = payload.IssuedAtTimeSeconds.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(payload.IssuedAtTimeSeconds.Value).DateTime
                        : DateTime.Now;
                    sso.HExp = payload.ExpirationTimeSeconds.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(payload.ExpirationTimeSeconds.Value).DateTime
                        : DateTime.Now.AddHours(1);

                    _db.HSsos.Update(sso);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // ✅ 尚未存在 SSO → 確認是否已有帳號
                    tenant = _db.HTenants.FirstOrDefault(t => t.HEmail == payload.Email && !t.HIsDeleted);

                    if (tenant == null)
                    {
                        // 新增 HTenant
                        tenant = new HTenant
                        {
                            HUserName = payload.Name ?? payload.Email.Split('@')[0],
                            HEmail = payload.Email,
                            HPhoneNumber = null,
                            HIsTenant = true,
                            HIsLandlord = false,
                            HStatus = "已驗證",
                            HCreatedAt = DateTime.Now,
                            HUpdateAt = DateTime.Now,
                            HIsDeleted = false,
                            HLoginFailCount = 0,
                            HPassword = null,
                            HSalt = null
                        };

                        _db.HTenants.Add(tenant);
                        await _db.SaveChangesAsync();
                    }

                    // 補建一筆 SSO 紀錄
                    sso = new HSso
                    {
                        HTenantId = tenant.HTenantId,
                        HSub = payload.Subject,
                        HAud = aud,
                        HUserEmail = payload.Email,
                        HEmailverified = payload.EmailVerified,
                        HIat = payload.IssuedAtTimeSeconds.HasValue
                            ? DateTimeOffset.FromUnixTimeSeconds(payload.IssuedAtTimeSeconds.Value).DateTime
                            : DateTime.Now,
                        HExp = payload.ExpirationTimeSeconds.HasValue
                            ? DateTimeOffset.FromUnixTimeSeconds(payload.ExpirationTimeSeconds.Value).DateTime
                            : DateTime.Now.AddHours(1)
                    };

                    _db.HSsos.Add(sso);
                    await _db.SaveChangesAsync();
                }

                // Step 6️⃣ 寫入登入 Session
                SessionManager.SetLogin(HttpContext, tenant);

                // Step 7️⃣ 回傳登入資訊
                return Ok(new
                {
                    success = true,
                    message = "Google 登入成功",
                    user = tenant.HEmail,
                    userName = tenant.HUserName,
                    tenantId = tenant.HTenantId,
                    isLandlord = tenant.HIsLandlord,
                    role = tenant.HIsLandlord ? "landlord" : "tenant"
                });
            }
            catch (InvalidJwtException ex)
            {
                return Unauthorized(new { success = false, message = "Token 驗證失敗", error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Google 登入錯誤: " + ex.ToString());
                return StatusCode(500, new
                {
                    success = false,
                    message = "登入失敗",
                    error = ex.ToString()
                });
            }
        }



        //reCAPTCHA 驗證方法
        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            var secretKey = "6Ldt9T4rAAAAAFGgF9KDgBXyz46god-1q6VVxKtN"; 
            using var client = new HttpClient();

            var parameters = new Dictionary<string, string>
    {
        { "secret", secretKey },
        { "response", token }
    };

            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(parameters));
            var json = await response.Content.ReadAsStringAsync();

            var result = System.Text.Json.JsonSerializer.Deserialize<RecaptchaResult>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result != null && result.Success && result.Score >= 0.5 && result.Action == "login";
        }




    }
}
