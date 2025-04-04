using GeeYeangSore.Areas.Admin.ViewModels;
using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class UserController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public UserController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        public IActionResult UserManagement()
        {
            return View(); // 初始頁面只回傳空的 View，結果靠 AJAX 動態載入
        }

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
                        TenantStatus = string.IsNullOrWhiteSpace(t.HStatus) ? "未設定" : t.HStatus.Trim(),
                        LandlordId = landlord != null ? landlord.HLandlordId.ToString() : "未開通",
                        LandlordStatus = string.IsNullOrWhiteSpace(landlord?.HStatus) ? "未驗證" : landlord.HStatus.Trim(),
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

            return PartialView("_UserListPartial", result);
        }
    }

}
