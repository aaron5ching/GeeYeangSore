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
            if (!HasAnyRole("超級管理員", "系統管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var data = _context.HTenants
                .Include(t => t.HLandlords)
                .AsEnumerable() // ⛳ 將 IQueryable 轉成記憶體 Enumerable，讓我們可以使用 `?.` 和其他 C# 功能
                .Select(t =>
                {
                    var landlord = t.HLandlords.FirstOrDefault(); // 取得對應的房東（可能為 null）

                    return new CUserViewModels
                    {
                        TenantId = t.HTenantId,

                        // ✅ 使用 IsNullOrWhiteSpace 避免空字串 + Trim() 移除左右空白
                        TenantStatus = string.IsNullOrWhiteSpace(t.HStatus) ? "未設定" : t.HStatus.Trim(),
                        LandlordId = landlord != null ? landlord.HLandlordId.ToString() : "未開通",
                        LandlordStatus = string.IsNullOrWhiteSpace(landlord?.HStatus) ? "未驗證" : landlord.HStatus.Trim(),

                        Name = t.HUserName ?? "未填寫",
                        RegisterDate = t.HCreatedAt ?? DateTime.MinValue,
                        IsTenant = t.HIsTenant ?? false,
                        IsLandlord = t.HIsLandlord ?? false
                    };
                }).ToList();

            return View(data);
        }
    }
}
