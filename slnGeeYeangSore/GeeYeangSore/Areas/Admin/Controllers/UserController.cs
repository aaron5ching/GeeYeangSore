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
                .Select(t => new CUserViewModels
                {
                    TenantId = t.HTenantId,
                    LandlordId = t.HLandlords.FirstOrDefault() != null ? t.HLandlords.First().HLandlordId.ToString() : "未啟用",
                    Name = t.HUserName ?? "未填寫",
                    RegisterDate = t.HCreatedAt ?? DateTime.MinValue,
                    Status = t.HStatus ?? "未設定",
                    IsTenant = t.HIsTenant ?? false,
                    IsLandlord = t.HIsLandlord ?? false
                }).ToList();

            return View(data);
        }












    }
}
