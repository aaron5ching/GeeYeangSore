using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GeeYeangSore.APIControllers.Landlord
{
    [ApiController]
    [Route("api/[controller]")]
    public class BecomeLandlordController : ControllerBase
    {
        private readonly GeeYeangSoreContext _context;
        private readonly IWebHostEnvironment _environment;

        public BecomeLandlordController(GeeYeangSoreContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] LandlordRegistrationModel model)
        {
            try
            {
                // 從 Session 取得登入的 Email
                var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { message = "未登入，請先登入後再申請成為房東。" });
                }
                var tenant = await _context.HTenants.FirstOrDefaultAsync(t => t.HEmail == email && !t.HIsDeleted);
                if (tenant == null)
                {
                    return Unauthorized(new { message = "找不到對應的租客帳號，請重新登入。" });
                }
                var tenantId = tenant.HTenantId;

                // 處理圖片上傳
                string idCardFrontPath = "";
                string idCardBackPath = "";

                if (model.IdCardFront != null)
                    idCardFrontPath = await SaveImage(model.IdCardFront, "User");
                if (model.IdCardBack != null)
                    idCardBackPath = await SaveImage(model.IdCardBack, "User");

                // 新增房東資料
                var landlord = new HLandlord
                {
                    HTenantId = tenantId,
                    HLandlordName = model.Username,
                    HBankName = model.BankName,
                    HBankAccount = model.Bank,
                    HIdCardFrontUrl = idCardFrontPath,
                    HIdCardBackUrl = idCardBackPath,
                    HStatus = "驗證中",
                    HCreatedAt = DateTime.Now,
                    HUpdateAt = DateTime.Now,
                    HIsDeleted = false
                };

                _context.HLandlords.Add(landlord);
                await _context.SaveChangesAsync();

                return Ok(new { message = "註冊成功，等待審核" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task<string> SaveImage(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return "";

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.WebRootPath, "images", folder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }

    public class LandlordRegistrationModel
    {
        public string Username { get; set; }
        public string BankName { get; set; }
        public string Bank { get; set; }
        public IFormFile IdCardFront { get; set; }
        public IFormFile IdCardBack { get; set; }
    }
}
