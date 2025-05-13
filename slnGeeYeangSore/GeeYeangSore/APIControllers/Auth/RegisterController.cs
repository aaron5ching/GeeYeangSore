using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using GeeYeangSore.DTO.User;
using System;
using System.Linq;

namespace GeeYeangSore.APIControllers.Auth
{
    [ApiController]
    [Route("api/[controller]")] // 對應為 /api/register
    public class RegisterController : BaseController
    {
        public RegisterController(GeeYeangSoreContext db) : base(db) { }

        [HttpPost]
        public IActionResult Register([FromBody] RegisterRequestDto dto)
        {
            try
            {
                // ✅ 基本欄位檢查
                if (string.IsNullOrWhiteSpace(dto.UserName))
                    return BadRequest(new { success = false, message = "請輸入姓名" });

                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new { success = false, message = "請輸入完整資訊" });

                if (!dto.IsAgreePolicy)
                    return BadRequest(new { success = false, message = "請先勾選同意隱私權政策" });

                // ✅ 信箱重複檢查
                if (_db.HTenants.Any(t => t.HEmail == dto.Email))
                    return BadRequest(new { success = false, message = "此信箱已註冊" });

                // ✅ 雜湊密碼
                var salt = PasswordHasher.GenerateSalt();
                var hash = PasswordHasher.HashPassword(dto.Password, salt);

                HTenant newTenant = new HTenant
                {
                    HEmail = dto.Email,
                    HPassword = hash,
                    HSalt = salt,
                    HUserName = dto.UserName,
                    HPhoneNumber = dto.Phone,
                    HStatus = "已驗證",
                    HIsTenant = true,
                    HIsLandlord = false,
                    HCreatedAt = DateTime.UtcNow,
                    HUpdateAt = DateTime.UtcNow,
                    HIsDeleted = false,
                    HLoginFailCount = 0
                };

                _db.HTenants.Add(newTenant);
                _db.SaveChanges();

                return Ok(new { success = true, message = "註冊成功" });
            }
            catch (Exception ex)
            {
                // ✅ 回傳詳細錯誤訊息給前端
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤",
                    error = ex.Message
                });
            }
        }


    }
}
