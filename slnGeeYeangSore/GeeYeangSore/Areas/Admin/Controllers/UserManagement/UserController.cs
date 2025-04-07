using GeeYeangSore.Areas.Admin.ViewModels.UserManagement;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace GeeYeangSore.Areas.Admin.Controllers.UserManagement
{
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")]
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

            return PartialView("~/Areas/Admin/Partials/UserManagement/_UserListPartial.cshtml", result);

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

            return PartialView("~/Areas/Admin/Partials/UserManagement/_EditUserPartial.cshtml", tenant);
        }

        // AJAX 儲存編輯內容（使用 ViewModel）
        [HttpPost]
        public IActionResult Edit([FromBody] CEditUserViewModel updated)
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

            // 🐥 Step 1：印出前端傳入內容
            Console.WriteLine("收到前端傳入的 updated ViewModel：");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(updated));

            // 🐥 Step 2：查詢資料庫原始房客資料
            var existing = _context.HTenants
                .Include(t => t.HLandlords)
                .FirstOrDefault(t => t.HTenantId == updated.HTenantId);

            if (existing == null)
            {
                Console.WriteLine("查無對應的 HTenantId：" + updated.HTenantId);
                return NotFound();
            }

            // 🧍 更新房客欄位
            existing.HUserName = updated.HUserName;
            existing.HStatus = updated.HStatus;
            existing.HBirthday = updated.HBirthday;
            existing.HGender = updated.HGender;
            existing.HAddress = updated.HAddress;
            existing.HPhoneNumber = updated.HPhoneNumber;
            existing.HEmail = updated.HEmail;
            existing.HPassword = updated.HPassword;
            existing.HImages = string.IsNullOrWhiteSpace(updated.HImages) ? existing.HImages : updated.HImages;

            // 🪪 更新房東欄位（僅取第一位）
            var updatedLandlord = updated.HLandlords.FirstOrDefault();
            var existingLandlord = existing.HLandlords.FirstOrDefault();

            if (updatedLandlord != null && existingLandlord != null)
            {
                Console.WriteLine("🪪 房東資料更新內容：");
                Console.WriteLine($"▶️ 房東本名：{existingLandlord.HLandlordName} → {updatedLandlord.HLandlordName}");
                Console.WriteLine($"▶️ 身份狀態：{existingLandlord.HStatus} → {updatedLandlord.HStatus}");
                Console.WriteLine($"▶️ 銀行名稱：{existingLandlord.HBankName} → {updatedLandlord.HBankName}");
                Console.WriteLine($"▶️ 銀行帳戶：{existingLandlord.HBankAccount} → {updatedLandlord.HBankAccount}");
                Console.WriteLine($"▶️ 正面：{existingLandlord.HIdCardFrontUrl} → {updatedLandlord.HIdCardFrontUrl}");
                Console.WriteLine($"▶️ 反面：{existingLandlord.HIdCardBackUrl} → {updatedLandlord.HIdCardBackUrl}");

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
                Console.WriteLine("⚠️ 房東資料為空，無法更新");
            }

            _context.HTenants.Update(existing);

            try
            {
                int affected = _context.SaveChanges();
                Console.WriteLine($"🟢 儲存成功，共更新 {affected} 筆資料");
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


        // ✅ GET：載入新增表單
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("~/Areas/Admin/Partials/UserManagement/_CreateUserPartial.cshtml", new CEditUserViewModel());
        }

        // ✅ POST：儲存新增資料
        [HttpPost]
        public IActionResult Create([FromBody] CEditUserViewModel newUser)
        {
            if (!ModelState.IsValid)
                return BadRequest("模型驗證失敗");

            var tenant = new HTenant
            {
                HUserName = newUser.HUserName,
                HBirthday = newUser.HBirthday,
                HGender = newUser.HGender,
                HPhoneNumber = newUser.HPhoneNumber,
                HEmail = newUser.HEmail,
                HPassword = newUser.HPassword,
                HAddress = newUser.HAddress,
                HStatus = newUser.HStatus ?? "未驗證",
                HImages = newUser.HImages,
                HCreatedAt = DateTime.Now,
                HIsTenant = true,
                HIsLandlord = false
            };

            _context.HTenants.Add(tenant);
            _context.SaveChanges();

            return Ok();
        }




    }
}
