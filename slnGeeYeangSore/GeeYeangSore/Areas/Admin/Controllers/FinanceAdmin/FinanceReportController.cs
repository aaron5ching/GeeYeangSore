using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using GeeYeangSore.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GeeYeangSore.Areas.Admin.Controllers.FinanceAdmin
{
    [Area("Admin")]
    public class FinanceReportController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public FinanceReportController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        // 顯示報表首頁
        public IActionResult Index()
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            return View("~/Areas/Admin/Views/FinanceAdmin/ReportIndex.cshtml");
        }

        // 生成每日報表
        [HttpPost]
        public async Task<IActionResult> GenerateDailyReport(DateTime reportDate)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                var transactions = await _context.HTransactions
                    .Where(t => t.HPaymentDate != null &&
                           t.HPaymentDate.Value.Date == reportDate.Date &&
                           (t.HRtnMsg == "付款成功" || t.HRtnMsg == "待處理"))
                    .ToListAsync();

                var paymentMethods = transactions
                    .GroupBy(t => t.HPaymentType ?? "未知")
                    .Select(g => new
                    {
                        method = g.Key,
                        amount = g.Sum(t => t.HAmount ?? 0),
                        count = g.Count()
                    })
                    .ToDictionary(x => x.method, x => new { x.amount, x.count });

                var report = new HRevenueReport
                {
                    HReportDate = reportDate,
                    HTotalTransactions = transactions.Count,
                    HTotalIncome = transactions.Sum(t => t.HAmount ?? 0),
                    HDailyIncome = transactions.Sum(t => t.HAmount ?? 0),
                    HMonthlyIncome = 0, // 日報表不需要月收入
                    HGrowthRate = 0, // 日報表不需要增長率
                    HPaymentMethods = JsonSerializer.Serialize(paymentMethods),
                    HGeneratedAt = DateTime.Now,
                    HReportPeriod = reportDate
                };

                // 檢查是否已存在報表
                var existingReport = await _context.HRevenueReports
                    .FirstOrDefaultAsync(r => r.HReportDate != null &&
                                            r.HReportDate.Value.Date == reportDate.Date);

                if (existingReport != null)
                {
                    // 更新現有報表
                    existingReport.HTotalTransactions = report.HTotalTransactions;
                    existingReport.HTotalIncome = report.HTotalIncome;
                    existingReport.HDailyIncome = report.HDailyIncome;
                    existingReport.HMonthlyIncome = report.HMonthlyIncome;
                    existingReport.HGrowthRate = report.HGrowthRate;
                    existingReport.HPaymentMethods = report.HPaymentMethods;
                    existingReport.HGeneratedAt = report.HGeneratedAt;
                    existingReport.HReportPeriod = report.HReportPeriod;
                }
                else
                {
                    // 新增報表
                    _context.HRevenueReports.Add(report);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 生成月報表
        [HttpPost]
        public async Task<IActionResult> GenerateMonthlyReport(DateTime monthStart)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var transactions = await _context.HTransactions
                    .Where(t => t.HPaymentDate != null &&
                           t.HPaymentDate.Value >= monthStart &&
                           t.HPaymentDate.Value <= monthEnd &&
                           (t.HRtnMsg == "付款成功" || t.HRtnMsg == "待處理"))
                    .ToListAsync();

                var paymentMethods = transactions
                    .GroupBy(t => t.HPaymentType ?? "未知")
                    .Select(g => new
                    {
                        method = g.Key,
                        amount = g.Sum(t => t.HAmount ?? 0),
                        count = g.Count()
                    })
                    .ToDictionary(x => x.method, x => new { x.amount, x.count });

                // 計算增長率
                var lastMonthEnd = monthStart.AddDays(-1);
                var lastMonthStart = lastMonthEnd.AddMonths(-1).AddDays(1);
                var lastMonthIncome = await _context.HTransactions
                    .Where(t => t.HPaymentDate != null &&
                           t.HPaymentDate.Value >= lastMonthStart &&
                           t.HPaymentDate.Value <= lastMonthEnd &&
                           (t.HRtnMsg == "付款成功" || t.HRtnMsg == "待處理"))
                    .SumAsync(t => t.HAmount ?? 0m);

                var currentMonthIncome = transactions.Sum(t => t.HAmount ?? 0m);
                var growthRate = lastMonthIncome > 0
                    ? Math.Min(Math.Max(((currentMonthIncome - lastMonthIncome) / lastMonthIncome) * 100m, -999.99m), 999.99m)
                    : 0m;

                // 檢查是否已存在月報表
                var existingReport = await _context.HRevenueReports
                    .FirstOrDefaultAsync(r => r.HReportDate != null &&
                                            r.HReportDate.Value.Year == monthStart.Year &&
                                            r.HReportDate.Value.Month == monthStart.Month);

                if (existingReport != null)
                {
                    // 更新現有報表
                    existingReport.HTotalTransactions = transactions.Count;
                    existingReport.HTotalIncome = currentMonthIncome;
                    existingReport.HMonthlyIncome = currentMonthIncome;
                    existingReport.HDailyIncome = 0;
                    existingReport.HGrowthRate = growthRate;
                    existingReport.HPaymentMethods = JsonSerializer.Serialize(paymentMethods);
                    existingReport.HGeneratedAt = DateTime.Now;
                    existingReport.HReportPeriod = monthStart.Date;
                }
                else
                {
                    // 新增報表
                    var report = new HRevenueReport
                    {
                        HReportDate = monthStart.Date,
                        HTotalTransactions = transactions.Count,
                        HTotalIncome = currentMonthIncome,
                        HMonthlyIncome = currentMonthIncome,
                        HDailyIncome = 0,
                        HGrowthRate = growthRate,
                        HPaymentMethods = JsonSerializer.Serialize(paymentMethods),
                        HGeneratedAt = DateTime.Now,
                        HReportPeriod = monthStart.Date
                    };
                    _context.HRevenueReports.Add(report);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateException dbEx)
                {
                    var innerMessage = dbEx.InnerException?.Message ?? "無詳細錯誤信息";
                    var errorMessage = $"資料庫更新錯誤：{dbEx.Message}\n內部錯誤：{innerMessage}";
                    return Json(new { success = false, message = errorMessage });
                }
                catch (Exception dbEx)
                {
                    var innerMessage = dbEx.InnerException?.Message ?? "無詳細錯誤信息";
                    var errorMessage = $"資料庫錯誤：{dbEx.Message}\n內部錯誤：{innerMessage}";
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "無詳細錯誤信息";
                var errorMessage = $"系統錯誤：{ex.Message}\n內部錯誤：{innerMessage}";
                return Json(new { success = false, message = errorMessage });
            }
        }

        // 獲取報表列表
        [HttpGet]
        public async Task<IActionResult> GetReports(DateTime? startDate, DateTime? endDate)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                var query = _context.HRevenueReports.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.HReportDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(r => r.HReportDate <= endDate.Value);

                var reports = await query
                    .OrderByDescending(r => r.HReportDate)
                    .ToListAsync();

                return Json(new { success = true, data = reports });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 匯出報表
        [HttpGet]
        public async Task<IActionResult> ExportReport(int reportId)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "財務管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                var report = await _context.HRevenueReports
                    .FirstOrDefaultAsync(r => r.HReportId == reportId);

                if (report == null)
                    return NotFound();

                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}