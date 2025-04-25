using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using GeeYeangSore.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace GeeYeangSore.Areas.Admin.Controllers.FinanceAdmin
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class FinanceController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public FinanceController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        [Route("Index")]
        [HttpGet]
        public IActionResult Index()
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            return View("~/Areas/Admin/Views/FinanceAdmin/FinanceIndex.cshtml");
        }

        /// <summary>
        /// 獲取每日交易數據（用於圖表）
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>JSON 格式的每日交易數據</returns>
        [Route("GetDailyStats")]
        [HttpGet]
        public async Task<IActionResult> GetDailyStats(DateTime startDate, DateTime endDate)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var dailyStats = await (from t in _context.HTransactions
                                    where t.HPaymentDate >= startDate && t.HPaymentDate <= endDate
                                    && (t.HRtnMsg == "付款成功" || t.HRtnMsg == "待處理")
                                    group t by t.HPaymentDate.Value.Date into g
                                    orderby g.Key
                                    select new
                                    {
                                        Date = g.Key.ToString("yyyy-MM-dd"),
                                        TotalAmount = g.Sum(t => t.HAmount ?? 0),
                                        TransactionCount = g.Count()
                                    }).ToListAsync();

            return Json(new { success = true, data = dailyStats });
        }

        [Route("GetStats")]
        [HttpGet]
        public async Task<IActionResult> GetStats(DateTime? startDate, DateTime? endDate)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                if (!startDate.HasValue)
                    startDate = DateTime.Now.AddDays(-30);
                if (!endDate.HasValue)
                    endDate = DateTime.Now;

                var query = from t in _context.HTransactions
                            where t.HPaymentDate >= startDate && t.HPaymentDate <= endDate
                            select t;

                var transactions = await query.ToListAsync();
                var validTransactions = transactions.Where(t =>
                    t.HRtnMsg == "付款成功" || t.HRtnMsg == "待處理").ToList();

                // 基本統計
                var totalCount = validTransactions.Count;
                var totalAmount = validTransactions.Sum(t => t.HAmount ?? 0);
                var completedTransactions = validTransactions.Count(t => t.HRtnMsg == "付款成功");
                var completedAmount = validTransactions.Where(t => t.HRtnMsg == "付款成功")
                    .Sum(t => t.HAmount ?? 0);
                var pendingTransactions = validTransactions.Count(t => t.HRtnMsg == "待處理");
                var pendingAmount = validTransactions.Where(t => t.HRtnMsg == "待處理")
                    .Sum(t => t.HAmount ?? 0);

                // 付款方式統計
                var paymentMethods = validTransactions
                    .GroupBy(t => t.HPaymentType ?? "未知")
                    .Select(g => new
                    {
                        method = g.Key,
                        count = g.Count(),
                        percentage = Math.Round((double)g.Count() / totalCount * 100, 1)
                    })
                    .ToList();

                // 計算城市分布
                var cityDistribution = validTransactions
                    .Where(t => !string.IsNullOrEmpty(t.HRegion))
                    .GroupBy(t => t.HRegion)
                    .Select(g => new
                    {
                        city = g.Key,
                        count = g.Count(),
                        percentage = Math.Round((double)g.Count() / totalCount * 100, 2)
                    })
                    .OrderByDescending(x => x.count)
                    .ToList();

                // 如果有交易沒有區域資訊，加入"未知"分類
                var unknownCityCount = validTransactions.Count(t => string.IsNullOrEmpty(t.HRegion));
                if (unknownCityCount > 0)
                {
                    cityDistribution.Add(new
                    {
                        city = "未知",
                        count = unknownCityCount,
                        percentage = Math.Round((double)unknownCityCount / totalCount * 100, 2)
                    });
                }

                // 每日趨勢
                var dailyTrends = validTransactions
                    .GroupBy(t => t.HPaymentDate.Value.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("MM/dd"),
                        count = g.Count(),
                        amount = g.Sum(t => t.HAmount ?? 0)
                    })
                    .OrderBy(x => x.date)
                    .ToList();

                var result = new
                {
                    success = true,
                    data = new
                    {
                        totalTransactions = totalCount,
                        totalAmount = totalAmount,
                        completedTransactions = completedTransactions,
                        completedAmount = completedAmount,
                        pendingTransactions = pendingTransactions,
                        pendingAmount = pendingAmount,
                        paymentMethods = paymentMethods,
                        cityDistribution = cityDistribution,
                        dailyTrends = dailyTrends
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}