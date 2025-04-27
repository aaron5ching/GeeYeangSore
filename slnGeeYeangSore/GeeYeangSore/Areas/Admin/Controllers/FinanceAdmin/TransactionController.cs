using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using GeeYeangSore.Controllers;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Areas.Admin.Controllers.FinanceAdmin
{
    [Area("Admin")]
    public class TransactionController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public TransactionController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 顯示交易紀錄列表
        /// </summary>
        /// <param name="searchTerm">搜尋關鍵字（交易編號、房東姓名）</param>
        /// <param name="status">交易狀態篩選</param>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>交易紀錄列表視圖</returns>
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var query = _context.HTransactions
                .Include(t => t.HProperty)
                .AsQueryable();

            // 搜尋條件
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t =>
                    t.HMerchantTradeNo.Contains(searchTerm) ||
                    t.HTradeNo.Contains(searchTerm) ||
                    t.HItemName.Contains(searchTerm) ||
                    t.HProperty.HPropertyTitle.Contains(searchTerm)
                );
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.HTradeStatus == status);
            }

            // 日期範圍篩選
            if (startDate.HasValue)
            {
                query = query.Where(t => t.HPaymentDate >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(t => t.HPaymentDate < endDate.Value.Date.AddDays(1));
            }

            // 排序
            query = query.OrderByDescending(t => t.HPaymentDate);

            var transactions = await query.ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View("~/Areas/Admin/Views/FinanceAdmin/TransactionIndex.cshtml", transactions);
        }

        /// <summary>
        /// 查看交易詳情
        /// </summary>
        /// <param name="id">交易ID</param>
        /// <returns>交易詳情視圖</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.HTransactions
                .FirstOrDefaultAsync(m => m.HPaymentId == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View("~/Areas/Admin/Views/FinanceAdmin/TransactionDetails.cshtml", transaction);
        }

        /// <summary>
        /// 更新交易狀態
        /// </summary>
        /// <param name="id">交易ID</param>
        /// <param name="status">新狀態</param>
        /// <returns>JSON 回應</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var transaction = await _context.HTransactions.FindAsync(id);
            if (transaction == null)
            {
                return Json(new { success = false, message = "找不到交易記錄" });
            }

            transaction.HTradeStatus = status;
            transaction.HUpdateTime = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "狀態更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "更新失敗：" + ex.Message });
            }
        }
    }
}
