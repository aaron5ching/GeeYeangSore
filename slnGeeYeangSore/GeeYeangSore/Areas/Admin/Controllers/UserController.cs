using GeeYeangSore.Areas.Admin.ViewModels;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly GeeYeangSoreContext _context;

        public UserController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        // 初始頁面：顯示所有使用者
        public IActionResult UserManagement(int page = 1)
        {
            int pageSize = 15; // 每頁 15 筆
            var allUsers = _context.HTenants
                .Include(t => t.HLandlords)
                .AsEnumerable()
                .Select(t =>
                {
                    var landlord = t.HLandlords.FirstOrDefault();
                    return new CUserViewModels
                    {
                        TenantId = t.HTenantId,
                        TenantStatus = t.HStatus?.Trim() ?? "未設定",
                        LandlordId = landlord?.HLandlordId.ToString() ?? "-",
                        LandlordStatus = landlord?.HStatus?.Trim() ?? "未驗證",
                        Name = t.HUserName ?? "未填寫",
                        RegisterDate = t.HCreatedAt ?? DateTime.MinValue,
                        IsTenant = t.HIsTenant ?? false,
                        IsLandlord = t.HIsLandlord ?? false
                    };
                })
                .ToPagedList(page, pageSize); // ✅ 分頁處理

            return View(allUsers);
        }

        // AJAX 搜尋使用者
        [HttpPost]
        public IActionResult SearchUser([FromBody] CUserSearchViewModel query, int page = 1)
        {
            if (query == null)
            {
                return BadRequest("查詢條件為空");
            }

            int pageSize = 15;

            var result = _context.HTenants
                .Include(t => t.HLandlords)
                .AsEnumerable()
                .Select(t =>
                {
                    var landlord = t.HLandlords.FirstOrDefault();
                    return new CUserViewModels
                    {
                        TenantId = t.HTenantId,
                        TenantStatus = t.HStatus?.Trim() ?? "未設定",
                        LandlordId = landlord?.HLandlordId.ToString() ?? "-",
                        LandlordStatus = landlord?.HStatus?.Trim() ?? "未驗證",
                        Name = t.HUserName ?? "未填寫",
                        RegisterDate = t.HCreatedAt ?? DateTime.MinValue,
                        IsTenant = t.HIsTenant ?? false,
                        IsLandlord = t.HIsLandlord ?? false
                    };
                })
                .Where(u =>
                    (string.IsNullOrEmpty(query.UserId) || u.TenantId.ToString().Contains(query.UserId) || u.LandlordId.Contains(query.UserId)) &&
                    (string.IsNullOrEmpty(query.Name) || u.Name.Contains(query.Name)) &&
                    (string.IsNullOrEmpty(query.Status) || u.TenantStatus == query.Status || u.LandlordStatus == query.Status) &&
                    (!query.StartDate.HasValue || u.RegisterDate >= query.StartDate.Value) &&
                    (!query.EndDate.HasValue || u.RegisterDate <= query.EndDate.Value) &&
                    (!query.IsLandlord.HasValue || u.IsLandlord == query.IsLandlord.Value)
                )
                .ToPagedList(page, pageSize); // ✅ 分頁處理

            return PartialView("~/Areas/Admin/Partials/_UserListPartial.cshtml", result);
        }



        // AJAX 載入編輯視窗（Partial View）
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var tenant = _context.HTenants
                .Include(t => t.HLandlords)
                    .ThenInclude(l => l.HProperties)
                .FirstOrDefault(t => t.HTenantId == id);

            if (tenant == null)
                return NotFound();

            return PartialView("~/Areas/Admin/Partials/_EditUserPartial.cshtml", tenant);
        }

        // AJAX 儲存編輯內容
        [HttpPost]
        public IActionResult Edit([FromBody] HTenant updatedTenant)
        {

            if (!ModelState.IsValid)
            {
                Console.WriteLine("模型驗證失敗！");
                foreach (var kvp in ModelState)
                {
                    foreach (var err in kvp.Value.Errors)
                    {
                        Console.WriteLine($"[欄位 {kvp.Key}]：{err.ErrorMessage}");
                    }
                }
                return BadRequest("模型驗證失敗");
            }


            // 🐥 Step 1：除錯輸出接收到的 JSON 內容
            Console.WriteLine("收到前端傳入的 updatedTenant：");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(updatedTenant));

            // 🐥 Step 2：查找現有資料
            var existing = _context.HTenants
                .Include(t => t.HLandlords)
                .FirstOrDefault(t => t.HTenantId == updatedTenant.HTenantId);

            if (existing == null)
            {
                Console.WriteLine("查無對應的 HTenantId：" + updatedTenant.HTenantId);
                return NotFound();
            }

            // 🐥 Step 3：印出前後比對值（看是否真的有差異）
            Console.WriteLine($"房客原本姓名：{existing.HUserName}，更新為：{updatedTenant.HUserName}");
            Console.WriteLine($"原本照片檔名：{existing.HImages}，更新為：{updatedTenant.HImages}");

            // 🧍 更新房客資料
            existing.HUserName = updatedTenant.HUserName;
            existing.HStatus = updatedTenant.HStatus;
            existing.HBirthday = updatedTenant.HBirthday;
            existing.HGender = updatedTenant.HGender;
            existing.HAddress = updatedTenant.HAddress;
            existing.HPhoneNumber = updatedTenant.HPhoneNumber;
            existing.HEmail = updatedTenant.HEmail;
            existing.HPassword = updatedTenant.HPassword;
            // ✅ 若前端傳來圖片為 null，就保留原本資料
            existing.HImages = string.IsNullOrWhiteSpace(updatedTenant.HImages) ? existing.HImages : updatedTenant.HImages;

            // 🪪 更新房東資料（只取第一位）
            var existingLandlord = existing.HLandlords.FirstOrDefault();
            var updatedLandlord = updatedTenant.HLandlords.FirstOrDefault();

            if (existingLandlord != null && updatedLandlord != null)
            {
                Console.WriteLine("🪪 房東資料更新內容：");
                Console.WriteLine($"▶️ 房東本名：{existingLandlord.HLandlordName} → {updatedLandlord.HLandlordName}");
                Console.WriteLine($"▶️ 身份狀態：{existingLandlord.HStatus} → {updatedLandlord.HStatus}");
                Console.WriteLine($"▶️ 銀行名稱：{existingLandlord.HBankName} → {updatedLandlord.HBankName}");
                Console.WriteLine($"▶️ 銀行帳戶：{existingLandlord.HBankAccount} → {updatedLandlord.HBankAccount}");
                Console.WriteLine($"▶️ 正面：{existingLandlord.HIdCardFrontUrl} → {updatedLandlord.HIdCardFrontUrl}");
                Console.WriteLine($"▶️ 反面：{existingLandlord.HIdCardBackUrl} → {updatedLandlord.HIdCardBackUrl}");

                // ✅ 更新房東欄位
                existingLandlord.HLandlordName = updatedLandlord.HLandlordName;
                existingLandlord.HStatus = updatedLandlord.HStatus;
                existingLandlord.HBankName = updatedLandlord.HBankName;
                existingLandlord.HBankAccount = updatedLandlord.HBankAccount;
                existingLandlord.HIdCardFrontUrl = updatedLandlord.HIdCardFrontUrl;
                existingLandlord.HIdCardBackUrl = updatedLandlord.HIdCardBackUrl;

                _context.HLandlords.Update(existingLandlord);
            }
            else
            {
                Console.WriteLine("⚠️ updatedTenant.HLandlords 為空或 existing.HLandlords 為空！");
            }

            // ✅ 同樣使用 Update 而不是手動設定狀態
            _context.HTenants.Update(existing);

            try
            {
                int affected = _context.SaveChanges(); // ✅ 執行資料庫儲存
                Console.WriteLine($"🟢 實際寫入資料筆數：{affected}");
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 儲存失敗，錯誤訊息：{ex.Message}");
                return StatusCode(500, "資料儲存失敗，請稍後再試");
            }
        }




        // AJAX 刪除使用者
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var tenant = _context.HTenants.FirstOrDefault(t => t.HTenantId == id);
            if (tenant != null)
            {
                _context.HTenants.Remove(tenant);
                _context.SaveChanges();
                return Ok();
            }
            return NotFound();
        }

        // 上傳房客照片
        [HttpPost]
        public IActionResult UploadTenantPhoto(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("未選擇檔案");

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            var savePath = Path.Combine("wwwroot/images/User", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                photo.CopyTo(stream);
            }

            return Ok("/images/User/" + fileName);
        }

        // 上傳房東身分證正面
        [HttpPost]
        public IActionResult UploadLandlordIdFront(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("未選擇檔案");

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            var savePath = Path.Combine("wwwroot/images/User", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                photo.CopyTo(stream);
            }

            return Ok("/images/User/" + fileName);
        }

        // 上傳房東身分證反面
        [HttpPost]
        public IActionResult UploadLandlordIdBack(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("未選擇檔案");

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            var savePath = Path.Combine("wwwroot/images/User", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                photo.CopyTo(stream);
            }

            return Ok("/images/User/" + fileName);
        }
    }
}
