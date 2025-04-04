using GeeYeangSore.Areas.Admin.ViewModels;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult UserManagement()
        {
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
                        LandlordId = landlord?.HLandlordId.ToString() ?? "未開通",
                        LandlordStatus = landlord?.HStatus?.Trim() ?? "未驗證",
                        Name = t.HUserName ?? "未填寫",
                        RegisterDate = t.HCreatedAt ?? DateTime.MinValue,
                        IsTenant = t.HIsTenant ?? false,
                        IsLandlord = t.HIsLandlord ?? false
                    };
                })
                .ToList();

            return View(allUsers);
        }

        // AJAX 搜尋使用者
        [HttpPost]
        public IActionResult SearchUser([FromBody] CUserSearchViewModel query)
        {
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
                        LandlordId = landlord?.HLandlordId.ToString() ?? "未開通",
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
                    (!query.IsTenant.HasValue || u.IsTenant == query.IsTenant.Value) &&
                    (!query.IsLandlord.HasValue || u.IsLandlord == query.IsLandlord.Value)
                )
                .ToList();

            return PartialView("~/Areas/Admin/Partials/_UserListPartial.cshtml", result);
        }

        // AJAX 載入編輯視窗（Partial View）
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // 載入房客資料，包含房東與房源
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
        public IActionResult Edit(HTenant updatedTenant)
        {
            var existing = _context.HTenants.FirstOrDefault(t => t.HTenantId == updatedTenant.HTenantId);
            if (existing == null)
                return NotFound();

            existing.HUserName = updatedTenant.HUserName;
            existing.HStatus = updatedTenant.HStatus;
            _context.SaveChanges();

            return Ok();
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
    }
}
