using GeeYeangSore.APIControllers.Session;
using GeeYeangSore.Models;
using GeeYeangSore.DTO.User;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace GeeYeangSore.APIControllers.userhome;

[Route("api/[controller]")]
[ApiController]
public class UserProfileController : BaseController
{
    private readonly IWebHostEnvironment _env;

    public UserProfileController(GeeYeangSoreContext db, IWebHostEnvironment env) : base(db)
    {
        _env = env;
    }

    // ① 取得目前登入者資料
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        try
        {
            var tenant = GetCurrentTenant();
            if (tenant == null) return Unauthorized();

            var data = new
            {
                tenant.HUserName,
                tenant.HBirthday,
                Gender = tenant.HGender == true ? "male" : "female",
                Address = tenant.HAddress,
                tenant.HPhoneNumber,
                tenant.HEmail,
                Avatar = tenant.HImages,
                IsGoogleLogin = tenant.HPassword == null
            };

            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得個人資料時發生錯誤", error = ex.Message });
        }
    }

    // ② 上傳暫存頭像（只存至 wwwroot/temp-avatars）
    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "未提供檔案" });

            // 驗證檔案類型
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedTypes.Contains(ext))
                return BadRequest(new { message = "不支援的檔案格式，請上傳 JPG、PNG 或 GIF 圖片" });

            // 驗證檔案大小（限制 5MB）
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "檔案大小不能超過 5MB" });

            var fileName = $"temp_{Guid.NewGuid()}{ext}";
            var folderPath = Path.Combine(_env.WebRootPath, "temp-avatars");
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/temp-avatars/{fileName}";
            return Ok(new { avatarUrl = url });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "上傳頭像時發生錯誤", error = ex.Message });
        }
    }

    // ③ 儲存變更（包含密碼驗證與頭像）
    [HttpPost("save-profile")]
    public IActionResult SaveProfile([FromBody] UpdateProfileRequest dto)
    {
        try
        {
            var tenant = GetCurrentTenant();
            if (tenant == null) return Unauthorized();

            // 驗證手機號碼格式
            if (!Regex.IsMatch(dto.Phone, @"^09\d{8}$"))
                return BadRequest(new { message = "手機號碼格式不正確" });

            // 驗證生日日期
            if (dto.Birthday.HasValue && DateTime.TryParse(dto.Birthday.Value.ToString("yyyy/MM/dd"), out DateTime birthday))
            {
                if (birthday > DateTime.Now || birthday < DateTime.Now.AddYears(-120))
                    return BadRequest(new { message = "生日日期不正確" });

                // 將解析後的 DateTime 值賦值給 tenant.HBirthday
                tenant.HBirthday = birthday;
            }
            else
            {
                return BadRequest(new { message = "生日日期格式不正確" });
            }

            // 密碼驗證
            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password != dto.ConfirmPassword)
                    return BadRequest(new { message = "密碼與確認密碼不一致" });

                // 密碼強度驗證 - 與註冊時相同
                if (!Regex.IsMatch(dto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[^\s]{10,}$"))
                    return BadRequest(new { message = "密碼需至少10字元，包含大小寫英文字母、數字、特殊符號，且不得包含空白" });

                // 使用與註冊相同的密碼雜湊方法
                var salt = PasswordHasher.GenerateSalt();
                var hash = PasswordHasher.HashPassword(dto.Password, salt);
                tenant.HPassword = hash;
                tenant.HSalt = salt;
            }

            tenant.HUserName = dto.Name;
            tenant.HBirthday = birthday;
            tenant.HGender = dto.Gender == "male";
            tenant.HAddress = $"{dto.Address}".Trim();
            tenant.HPhoneNumber = dto.Phone;
            tenant.HImages = dto.Avatar;
            tenant.HUpdateAt = DateTime.Now;

            _db.SaveChanges();

            return Ok(new { message = "更新成功" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新個人資料時發生錯誤", error = ex.Message });
        }
    }

    // ④ 刪除帳號
    [HttpDelete("delete-account")]
    public IActionResult DeleteAccount()
    {
        try
        {
            var tenant = GetCurrentTenant();
            if (tenant == null) return Unauthorized();

            // 軟刪除：將帳號標記為已刪除
            tenant.HIsDeleted = true;
            _db.SaveChanges();

            return Ok(new { message = "帳號已成功刪除" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "刪除帳號時發生錯誤", error = ex.Message });
        }
    }
}
