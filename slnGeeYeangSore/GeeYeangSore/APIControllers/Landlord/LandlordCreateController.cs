using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using GeeYeangSore.Models.Dto;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GeeYeangSore.APIControllers.Landlord
{
    [Route("api/landlord/[controller]")]
    [ApiController]
    public class LandlordCreateController : ControllerBase
    {
        private readonly GeeYeangSoreContext _context;
        public LandlordCreateController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        [HttpPost("full-create")]
        public async Task<IActionResult> FullCreate(
            [FromForm] string property,
            [FromForm] string propertyFeature,
            [FromForm] string ad,
            [FromForm] List<IFormFile> images)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
                var landlord = _context.HLandlords.Include(l => l.HTenant).FirstOrDefault(l => l.HTenant.HEmail == email && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized("房東未登入");

                var propertyObj = JsonConvert.DeserializeObject<HProperty>(property);
                var featureObj = JsonConvert.DeserializeObject<HPropertyFeature>(propertyFeature);
                var adObj = JsonConvert.DeserializeObject<HAd>(ad);

                // 後端補齊必要欄位
                propertyObj.HLandlordId = landlord.HLandlordId;
                propertyObj.HPublishedDate = DateTime.Now;
                propertyObj.HLastUpdated = DateTime.Now;
                propertyObj.HIsDelete = false;
                propertyObj.HStatus = "Active";
                propertyObj.HAvailabilityStatus = propertyObj.HAvailabilityStatus ?? "未出租";
                propertyObj.HIsVip = false;
                propertyObj.HIsShared = false;
                propertyObj.HScore = "";
                _context.HProperties.Add(propertyObj);
                await _context.SaveChangesAsync();

                featureObj.HLandlordId = landlord.HLandlordId;
                featureObj.HPropertyId = propertyObj.HPropertyId;
                featureObj.HIsDelete = false;
                _context.HPropertyFeatures.Add(featureObj);

                foreach (var file in images)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var savePath = Path.Combine("wwwroot/uploads", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                    using (var stream = System.IO.File.Create(savePath))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var img = new HPropertyImage
                    {
                        HLandlordId = landlord.HLandlordId,
                        HPropertyId = propertyObj.HPropertyId,
                        HImageUrl = "/uploads/" + fileName,
                        HUploadedDate = DateTime.Now,
                        HIsDelete = false
                    };
                    _context.HPropertyImages.Add(img);
                }

                adObj.HLandlordId = landlord.HLandlordId;
                adObj.HPropertyId = propertyObj.HPropertyId;
                adObj.HCreatedDate = DateTime.Now;
                adObj.HLastUpdated = DateTime.Now;
                adObj.HIsDelete = false;
                _context.HAds.Add(adObj);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Debug.WriteLine($"[FullCreate] 成功: {JsonConvert.SerializeObject(propertyObj)}");
                return Ok(new { success = true, propertyId = propertyObj.HPropertyId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Debug.WriteLine($"[FullCreate] 失敗: {ex}");
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("my-properties")]
        public async Task<IActionResult> GetMyProperties()
        {
            var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { success = false, message = "未登入" });

            var landlord = await _context.HLandlords
                .Include(l => l.HTenant)
                .FirstOrDefaultAsync(l => l.HTenant.HEmail == email && !l.HIsDeleted);

            if (landlord == null)
                return NotFound(new { success = false, message = "找不到房東" });

            var properties = await _context.HProperties
                .Include(p => p.HPropertyImages)
                .Where(p => p.HLandlordId == landlord.HLandlordId && p.HIsDelete == false)
                .OrderByDescending(p => p.HPublishedDate)
                .ToListAsync();

            return Ok(properties);
        }

        [HttpPost("save-draft")]
        public async Task<IActionResult> SaveDraft([FromBody] HProperty draft)
        {
            try
            {
                draft.HStatus = "草稿";
                draft.HIsDelete = false;
                draft.HPublishedDate = DateTime.Now;
                _context.HProperties.Add(draft);
                await _context.SaveChangesAsync();
                Debug.WriteLine($"[SaveDraft] 成功: {System.Text.Json.JsonSerializer.Serialize(draft)}");
                return Ok(new { success = true, draftId = draft.HPropertyId });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SaveDraft] 失敗: {ex}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
