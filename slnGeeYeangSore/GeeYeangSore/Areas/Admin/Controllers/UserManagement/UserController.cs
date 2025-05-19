using GeeYeangSore.Areas.Admin.ViewModels.UserManagement;
using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace GeeYeangSore.Areas.Admin.Controllers.UserManagement
{
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")]
    public class UserController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public UserController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        // 初始頁面：顯示所有使用者
        public IActionResult UserManagement(int page = 1)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "客服管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            int pageSize = 15;
            var allUsers = _context.HTenants
                .Where(t => !t.HIsDeleted) // 過濾未被刪除的使用者
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
                        RegisterDate = t.HCreatedAt,
                        IsTenant = t.HIsTenant,
                        IsLandlord = t.HIsLandlord
                    };
                })
                .ToPagedList(page, pageSize);

            return View(allUsers);
        }

        [HttpPost]
        public IActionResult SearchUser([FromBody] CUserSearchViewModel query, int page = 1)
        {
            if (query == null)
            {
                return BadRequest("查詢條件為空");
            }

            int pageSize = 15;

            var result = _context.HTenants
                .Where(t => !t.HIsDeleted) // 過濾未被刪除的使用者
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
                        RegisterDate = t.HCreatedAt,
                        IsTenant = t.HIsTenant,
                        IsLandlord = t.HIsLandlord
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
                .ToPagedList(page, pageSize);

            return PartialView("~/Areas/Admin/Partials/UserManagement/_UserListPartial.cshtml", result);
        }

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

            try
            {
                Console.WriteLine("收到前端傳入的 updated ViewModel：");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(updated));

                var existing = _context.HTenants
                    .Include(t => t.HLandlords)
                    .FirstOrDefault(t => t.HTenantId == updated.HTenantId);

                if (existing == null)
                {
                    Console.WriteLine("查無對應的 HTenantId：" + updated.HTenantId);
                    return NotFound();
                }

                // 檢查電話是否被其他使用者使用
                if (!string.IsNullOrEmpty(updated.HPhoneNumber) && 
                    existing.HPhoneNumber != updated.HPhoneNumber && 
                    _context.HTenants.Any(t => t.HPhoneNumber == updated.HPhoneNumber && t.HTenantId != updated.HTenantId && !t.HIsDeleted))
                {
                    return BadRequest("電話號碼已被其他使用者使用，請使用其他電話號碼");
                }

                // 檢查電子郵件是否被其他使用者使用
                if (!string.IsNullOrEmpty(updated.HEmail) && 
                    existing.HEmail != updated.HEmail && 
                    _context.HTenants.Any(t => t.HEmail == updated.HEmail && t.HTenantId != updated.HTenantId && !t.HIsDeleted))
                {
                    return BadRequest("電子郵件已被其他使用者使用，請使用其他電子郵件");
                }

                existing.HUserName = updated.HUserName ?? existing.HUserName;
                existing.HStatus = updated.HStatus ?? existing.HStatus;
                existing.HBirthday = updated.HBirthday;  // 可選欄位，直接賦值
                existing.HGender = updated.HGender;      // 可選欄位，直接賦值
                existing.HAddress = updated.HAddress;    // 可選欄位，直接賦值
                existing.HPhoneNumber = updated.HPhoneNumber ?? existing.HPhoneNumber;
                existing.HEmail = updated.HEmail ?? existing.HEmail;
                existing.HUpdateAt = DateTime.Now;       // 更新修改時間

                // 處理密碼更新，檢查是否需要更新密碼
                if (!string.IsNullOrWhiteSpace(updated.HPassword))
                {
                    // 檢查密碼是否與資料庫中的雜湊密碼不同
                    bool needsUpdate = true;
                    if (!string.IsNullOrEmpty(existing.HSalt))
                    {
                        // 如果已有鹽值，檢查提交的密碼是否與雜湊後的密碼相同
                        needsUpdate = !PasswordHasher.VerifyPassword(updated.HPassword, existing.HSalt, existing.HPassword);
                    }
                    else
                    {
                        // 如果沒有鹽值（舊帳號），檢查密碼是否與明文密碼相同
                        needsUpdate = updated.HPassword != existing.HPassword;
                    }

                    // 需要更新密碼
                    if (needsUpdate)
                    {
                        string salt = PasswordHasher.GenerateSalt();
                        existing.HPassword = PasswordHasher.HashPassword(updated.HPassword, salt);
                        existing.HSalt = salt;
                    }
                }

                existing.HImages = updated.HImages;  // 可選欄位，直接賦值

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

                    existingLandlord.HLandlordName = updatedLandlord.HLandlordName ?? existingLandlord.HLandlordName;
                    existingLandlord.HStatus = updatedLandlord.HStatus ?? existingLandlord.HStatus;
                    existingLandlord.HBankName = updatedLandlord.HBankName ?? existingLandlord.HBankName;
                    existingLandlord.HBankAccount = updatedLandlord.HBankAccount ?? existingLandlord.HBankAccount;
                    existingLandlord.HIdCardFrontUrl = string.IsNullOrWhiteSpace(updatedLandlord.HIdCardFrontUrl)
                        ? existingLandlord.HIdCardFrontUrl
                        : updatedLandlord.HIdCardFrontUrl;
                    existingLandlord.HIdCardBackUrl = string.IsNullOrWhiteSpace(updatedLandlord.HIdCardBackUrl)
                        ? existingLandlord.HIdCardBackUrl
                        : updatedLandlord.HIdCardBackUrl;



                    _context.HLandlords.Update(existingLandlord);
                }
                else
                {
                    Console.WriteLine("⚠️ 房東資料為空，無法更新");
                }

                _context.HTenants.Update(existing);
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

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var tenant = _context.HTenants.FirstOrDefault(t => t.HTenantId == id);
                if (tenant != null)
                {
                    tenant.HIsDeleted = true; // 設為軟刪除
                    tenant.HUpdateAt = DateTime.Now; // 更新修改時間
                    _context.HTenants.Update(tenant);
                    _context.SaveChanges();
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 軟刪除失敗：{ex.Message}");
                return StatusCode(500, "刪除失敗，請稍後再試");
            }
        }


        [HttpPost]
        public IActionResult UploadTenantPhoto(IFormFile photo)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 上傳照片失敗：{ex.Message}");
                return StatusCode(500, "上傳照片失敗");
            }
        }

        [HttpPost]
        public IActionResult UploadLandlordIdFront(IFormFile photo)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 上傳正面失敗：{ex.Message}");
                return StatusCode(500, "上傳失敗");
            }
        }

        [HttpPost]
        public IActionResult UploadLandlordIdBack(IFormFile photo)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 上傳反面失敗：{ex.Message}");
                return StatusCode(500, "上傳失敗");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("~/Areas/Admin/Partials/UserManagement/_CreateUserPartial.cshtml", new CEditUserViewModel());
        }

        [HttpPost]
        public IActionResult Create([FromBody] CEditUserViewModel newUser)
        {
            if (newUser == null)
                return BadRequest("未填寫資料");

            try
            {
                // 檢查電話是否重複
                if (!string.IsNullOrEmpty(newUser.HPhoneNumber) && 
                    _context.HTenants.Any(t => t.HPhoneNumber == newUser.HPhoneNumber && !t.HIsDeleted))
                {
                    return BadRequest("電話號碼已被使用，請使用其他電話號碼");
                }

                // 檢查電子郵件是否重複
                if (!string.IsNullOrEmpty(newUser.HEmail) && 
                    _context.HTenants.Any(t => t.HEmail == newUser.HEmail && !t.HIsDeleted))
                {
                    return BadRequest("電子郵件已被使用，請使用其他電子郵件");
                }

                // 生成鹽值與哈希密碼
                string salt = PasswordHasher.GenerateSalt();
                string hashedPassword = PasswordHasher.HashPassword(newUser.HPassword ?? "000000", salt);

                var tenant = new HTenant
                {
                    HUserName = newUser.HUserName ?? "未命名",          // 必填
                    HBirthday = newUser.HBirthday,                     // 可選
                    HGender = newUser.HGender,                         // 可選
                    HPhoneNumber = newUser.HPhoneNumber ?? "未填寫",    // 必填
                    HEmail = newUser.HEmail ?? "未填寫",               // 必填
                    HPassword = hashedPassword,                        // 可選
                    HSalt = salt,                                      // 可選
                    HAddress = newUser.HAddress,                       // 可選
                    HStatus = newUser.HStatus ?? "未驗證",             // 必填
                    HImages = newUser.HImages,                         // 可選
                    HCreatedAt = DateTime.Now,                         // 必填
                    HUpdateAt = DateTime.Now,                          // 必填
                    HIsTenant = true,                                  // 必填
                    HIsLandlord = false,                               // 必填
                    HIsDeleted = false,                                // 必填
                    HLoginFailCount = 0                                // 必填
                };

                _context.HTenants.Add(tenant);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 建立房客失敗：{ex.Message}");
                return StatusCode(500, "建立失敗，請稍後再試");
            }
        }

        // 檢查電話號碼是否存在
        [HttpGet]
        public IActionResult CheckPhoneExists(string phoneNumber, int? excludeId = null)
        {
            bool exists = _context.HTenants
                .Any(t => t.HPhoneNumber == phoneNumber && 
                         !t.HIsDeleted && 
                         (!excludeId.HasValue || t.HTenantId != excludeId.Value));
                         
            return Json(new { exists });
        }

        // 檢查電子郵件是否存在
        [HttpGet]
        public IActionResult CheckEmailExists(string email, int? excludeId = null)
        {
            bool exists = _context.HTenants
                .Any(t => t.HEmail == email && 
                         !t.HIsDeleted && 
                         (!excludeId.HasValue || t.HTenantId != excludeId.Value));
                         
            return Json(new { exists });
        }

    }
}
