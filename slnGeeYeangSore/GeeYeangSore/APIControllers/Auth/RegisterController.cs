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
            // 基本欄位檢查
            if (string.IsNullOrWhiteSpace(dto.UserName))
                return BadRequest(new { success = false, message = "請輸入姓名" });

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { success = false, message = "請輸入完整資訊" });

            if (!dto.IsAgreePolicy)
                return BadRequest(new { success = false, message = "請先勾選同意隱私權政策" });

            // 信箱重複檢查
            if (_db.HTenants.Any(t => t.HEmail == dto.Email))
                return BadRequest(new { success = false, message = "此信箱已註冊" });

            // 驗證碼比對（簡易範例）
            //var token = _db.HEmailTokens.FirstOrDefault(t =>
            //    t.HUserEmail == dto.Email &&
            //    t.HEmailToken1 == dto.VerificationCode &&
            //    t.HResetExpiresAt > DateTime.UtcNow &&
            //    !t.HIsUsed);

            //if (token == null)
            //    return BadRequest(new { success = false, message = "驗證碼錯誤或已過期" });

            // 雜湊密碼
            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword(dto.Password, salt);

            HTenant newTenant = new HTenant
            {
                HEmail = dto.Email,
                HPassword = hash,
                HSalt = salt,
                HUserName = dto.UserName,
                HPhoneNumber = dto.Phone,
                HStatus = "已驗證", // ✅ 建議使用狀態欄位
                HIsTenant = true,
                HIsLandlord = false,
                HCreatedAt = DateTime.UtcNow,
                HUpdateAt = DateTime.UtcNow,
                HIsDeleted = false,
                HLoginFailCount = 0
            };

            _db.HTenants.Add(newTenant);

            // 標記驗證碼為已使用
            //token.HIsUsed = true;
            //token.HUsedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(new { success = true, message = "註冊成功" });
        }
    }

}
