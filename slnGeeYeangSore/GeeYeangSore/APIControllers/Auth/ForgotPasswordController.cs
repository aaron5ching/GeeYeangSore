using GeeYeangSore.Models;
using GeeYeangSore.DTO.User;
using GeeYeangSore.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GeeYeangSore.APIControllers.Auth;

[ApiController]
[Route("api/forgot-password")]
public class ForgotPasswordController : ControllerBase
{
    private readonly GeeYeangSoreContext _context;
    private readonly SmtpSettings _smtp;

    public ForgotPasswordController(GeeYeangSoreContext context, IOptions<SmtpSettings> smtpOptions)
    {
        _context = context;
        _smtp = smtpOptions.Value;
    }

    // ① 發送驗證碼
    // ① 發送驗證碼
    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] SendResetCodeDto dto)
    {
        try
        {
            // ✅ 檢查是否為註冊使用者
            var user = _context.HTenants.FirstOrDefault(u => u.HEmail == dto.Email);
            if (user == null)
                return NotFound(new { success = false, message = "信箱不存在" });

            // ✅ 產生 6 位數驗證碼
            string code = new Random().Next(100000, 999999).ToString();

            // ✅ 查詢是否已有該 email 的 ResetPassword 類型資料
            var existingToken = _context.HEmailTokens
                .Where(x => x.HUserEmail == dto.Email && x.HTokenType == "ResetPassword")
                .OrderByDescending(x => x.HCreatedAt)
                .FirstOrDefault();

            if (existingToken != null)
            {
                // ✅ 若存在，更新原有資料
                existingToken.HEmailToken1 = code;
                existingToken.HResetExpiresAt = DateTime.Now.AddMinutes(10);
                existingToken.HCreatedAt = DateTime.Now;
                existingToken.HIsUsed = false; // 重設為未使用
            }
            else
            {
                // ✅ 若不存在，新增一筆新資料
                _context.HEmailTokens.Add(new HEmailToken
                {
                    HUserEmail = dto.Email,
                    HEmailToken1 = code,
                    HResetExpiresAt = DateTime.Now.AddMinutes(10),
                    HCreatedAt = DateTime.Now,
                    HIsUsed = false,
                    HTokenType = "ResetPassword"
                });
            }

            // ✅ 儲存資料庫變更
            await _context.SaveChangesAsync();

            // ✅ 寄送驗證信
            using var smtpClient = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.FromEmail, _smtp.AppPassword),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_smtp.FromEmail, "租屋平台"),
                Subject = "重設密碼驗證碼",
                Body = $@"
<div style='font-family:Arial,sans-serif; max-width:600px; margin:auto; padding:20px; border:1px solid #ddd; border-radius:10px;'>
    <h2 style='color:#2c3e50;'>居研所租屋平台</h2>
    <p>親愛的使用者您好，</p>
    <p>您申請了密碼重設，請使用以下驗證碼完成身分確認：</p>
    <div style='font-size:32px; font-weight:bold; color:#e74c3c; margin:20px 0;'>{code}</div>
    <p>請於 <strong>10 分鐘內</strong> 完成輸入驗證。</p>
    <p style='font-size:14px; color:#888;'>※ 此為系統自動發送信件，請勿直接回覆。</p>
    <hr/>
    <p style='font-size:12px; color:#aaa;'>居研所租屋平台 © {DateTime.Now.Year}</p>
</div>",
                IsBodyHtml = true
            };

            mail.To.Add(dto.Email);
            await smtpClient.SendMailAsync(mail);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "寄送失敗", error = ex.Message });
        }
    }


    // ② 驗證驗證碼
    [HttpPost("verify-code")]
    public IActionResult VerifyCode([FromBody] VerifyResetCodeDto dto)
    {
        var token = _context.HEmailTokens
            .Where(t => t.HUserEmail == dto.Email &&
                        t.HTokenType == "ResetPassword" &&
                        !t.HIsUsed &&
                        t.HResetExpiresAt > DateTime.Now)
            .OrderByDescending(t => t.HCreatedAt)
            .FirstOrDefault();

        if (token == null || token.HEmailToken1 != dto.Code)
            return BadRequest(new { success = false, message = "驗證碼錯誤" });

        if (token.HResetExpiresAt < DateTime.Now)
            return BadRequest(new { success = false, message = "驗證碼已過期" });

        return Ok(new { success = true });
    }

    // ③ 重設密碼
    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var token = _context.HEmailTokens
            .Where(t => t.HUserEmail == dto.Email &&
                        t.HTokenType == "ResetPassword" &&
                        !t.HIsUsed &&
                        t.HResetExpiresAt > DateTime.Now)
            .OrderByDescending(t => t.HCreatedAt)
            .FirstOrDefault();

        if (token == null || token.HEmailToken1 != dto.Code || token.HResetExpiresAt < DateTime.Now)
            return BadRequest(new { success = false, message = "驗證失敗或已過期" });

        var user = _context.HTenants.FirstOrDefault(u => u.HEmail == dto.Email);
        if (user == null)
            return NotFound(new { success = false, message = "查無使用者" });

        // 🐥 雜湊密碼方法（同註冊邏輯）
        string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }

        // ✅ 執行密碼重設
        string newSalt = PasswordHasher.GenerateSalt();
        string hashedPassword = PasswordHasher.HashPassword(dto.NewPassword, newSalt);


        user.HPassword = hashedPassword;
        user.HSalt = newSalt;
        token.HIsUsed = true;

        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
