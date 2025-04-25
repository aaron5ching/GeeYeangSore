using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;
using GeeYeangSore.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace GeeYeangSore.Areas.Admin.Controllers.Messages
{
    /// <summary>
    /// 私人訊息管理控制器
    /// 負責處理私人聊天的相關功能，包括：
    /// 1. 私人對話列表顯示
    /// 2. 私人聊天內容查看
    /// 3. 私人訊息刪除
    /// 4. 訊息檢舉功能
    /// 5. 檢舉列表管理
    /// </summary>
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class PrivateMessagesController : SuperController
    {
        private readonly GeeYeangSoreContext _context;
        private const int PageSize = 10;

        public PrivateMessagesController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 私人對話列表頁面
        /// 顯示所有私人對話，支援分頁和搜尋功能
        /// </summary>
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 只顯示私人對話，並且一併載入檢舉資料
            var query = _context.HMessages
                .Include(m => m.HReports)  // 載入每則訊息的檢舉
                .Where(m => m.HChatId == null && m.HReceiverId != null && m.HReceiverId != 0);

            // 如果有搜尋關鍵字，過濾訊息內容
            if (!string.IsNullOrEmpty(searchString?.Trim()))
            {
                var trimmedSearch = searchString.Trim();

                // 先查出名字符合的發送者/接收者 Id（Tenant + Landlord）
                var tenantMatches = await _context.HTenants
                    .Where(t => t.HUserName.Contains(trimmedSearch) && t.HStatus == "已驗證")
                    .Select(t => t.HTenantId)
                    .ToListAsync();

                var landlordMatches = await _context.HLandlords
                    .Where(l => l.HLandlordName.Contains(trimmedSearch) && l.HStatus == "已驗證")
                    .Select(l => l.HTenantId) // 注意這裡 key 是 HTenantId
                    .ToListAsync();

                var matchedUserIds = tenantMatches.Concat(landlordMatches).Distinct().ToList();

                // 套用搜尋條件（訊息內容 + 發送者或接收者為匹配者）
                query = query.Where(m =>
                    m.HContent.Contains(trimmedSearch) ||
                    (m.HSenderId.HasValue && matchedUserIds.Contains(m.HSenderId.Value)) ||
                    (m.HReceiverId.HasValue && matchedUserIds.Contains(m.HReceiverId.Value))
                );
            }

            // 計算總頁數
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // 獲取當前頁的私人對話列表
            var privateChats = await query
                .OrderByDescending(m => m.HTimestamp)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();


            // 只掃一次 privateChats，分類收集 Sender 和 Receiver 的 Id
            var userIds = new HashSet<int>();

            foreach (var chat in privateChats)
            {
                if (chat.HSenderId.HasValue)
                    userIds.Add(chat.HSenderId.Value);
                if (chat.HReceiverId.HasValue)
                    userIds.Add(chat.HReceiverId.Value);
            }

            // 查 Landlords（用 h_TenantId當key）
            var landlords = await _context.HLandlords
                .Where(l => userIds.Contains(l.HTenantId) && l.HStatus == "已驗證")
                .ToDictionaryAsync(l => l.HTenantId, l => l.HLandlordName);

            // 查 Tenants（正常）
            var tenants = await _context.HTenants
                .Where(t => userIds.Contains(t.HTenantId) && t.HStatus == "已驗證")
                .ToDictionaryAsync(t => t.HTenantId, t => t.HUserName);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.Tenants = tenants;
            ViewBag.Landlords = landlords;

            Console.WriteLine($"ViewBag.Tenants：{string.Join(",", ViewBag.Tenants.Keys)}");
            Console.WriteLine($"ViewBag.Landlords：{string.Join(",", ViewBag.Landlords.Keys)}");
            return View(privateChats);
        }

        /// <summary>
        /// 私人聊天內容頁面
        /// 顯示特定用戶之間的聊天訊息
        /// </summary>
        [HttpGet]
        [Route("Admin/PrivateMessages/PrivateChat/{senderId}/{receiverId}")]
        public async Task<IActionResult> PrivateChat(int senderId, int receiverId, string senderRole = null, string receiverRole = null)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var messages = await _context.HMessages
                .Where(m => m.HChatId == null &&
                    ((m.HSenderId == senderId && m.HReceiverId == receiverId) ||
                     (m.HSenderId == receiverId && m.HReceiverId == senderId)))
                .Include(m => m.HReports)
                .OrderBy(m => m.HTimestamp)
                .ToListAsync();

            // 🛠 這裡新加：收集 SenderId / ReceiverId
            var userIds = messages
                .SelectMany(m => new[] { m.HSenderId, m.HReceiverId })
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var tenants = await _context.HTenants
                .Where(t => userIds.Contains(t.HTenantId) && t.HStatus == "已驗證")
                .ToDictionaryAsync(t => t.HTenantId, t => t.HUserName);

            var landlords = await _context.HLandlords
                .Where(l => userIds.Contains(l.HTenantId) && l.HStatus == "已驗證")
                .ToDictionaryAsync(l => l.HTenantId, l => l.HLandlordName);

            ViewBag.Tenants = tenants;
            ViewBag.Landlords = landlords;

            ViewBag.SenderId = senderId;
            ViewBag.ReceiverId = receiverId;
            ViewBag.SenderRole = senderRole;
            ViewBag.ReceiverRole = receiverRole;
            ViewBag.MessageCount = messages.Count;

            return View(messages);
        }

        /// <summary>
        /// 刪除私人訊息
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                var message = await _context.HMessages.FindAsync(id);
                if (message == null)
                {
                    return Json(new { success = false, message = "訊息不存在" });
                }

                // 將訊息標記為已刪除
                message.HIsDeleted = 1;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "訊息已成功刪除" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除失敗：" + ex.Message });
            }
        }

        /// <summary>
        /// 提交訊息檢舉
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int messageId, string reason)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var message = await _context.HMessages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            var report = new HReport
            {
                HMessageId = messageId,
                HAuthorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                HAuthorType = "Admin",
                HReason = reason,
                HStatus = "待處理",
                HCreatedAt = DateTime.Now,
                HReportType = "Private"
            };

            _context.HReports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "檢舉已成功提交！";
            return RedirectToAction("PrivateChat", new { senderId = message.HSenderId, receiverId = message.HReceiverId });
        }
        [HttpPost]
        public async Task<IActionResult> ProcessReport(int reportId, string status)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return Json(new { success = false, message = "無權限" });

            try
            {
                var report = await _context.HReports.FindAsync(reportId);
                if (report == null)
                {
                    return Json(new { success = false, message = "檢舉不存在" });
                }

                //  改用 Session 抓登入者帳號
                var account = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
                var admin = await _context.HAdmins.FirstOrDefaultAsync(a => a.HAccount == account);

                if (admin == null)
                {
                    return Json(new { success = false, message = "管理員不存在" });
                }

                // 更新檢舉狀態
                report.HStatus = status;
                report.HAdminId = admin.HAdminId;
                report.HReviewedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "檢舉已成功處理" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "處理失敗：" + ex.Message });
            }
        }

        /// <summary>
        /// 檢舉列表頁面
        /// 顯示所有私人訊息的檢舉記錄
        /// </summary>
        [HttpGet]
        [Route("~/Admin/PrivateMessages/ReportList")]
        public async Task<IActionResult> ReportList(int page = 1)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var query = _context.HReports
                .Include(r => r.HMessage)
                .Where(r => r.HReportType == "Private");

            // 計算總頁數
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // 獲取當前頁的檢舉列表
            var reports = await query
                .OrderByDescending(r => r.HCreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // 收集所有相關的用戶ID
            var userIds = new HashSet<int>();
            foreach (var report in reports)
            {
                if (report.HMessage?.HSenderId.HasValue == true)
                    userIds.Add(report.HMessage.HSenderId.Value);
                if (report.HMessage?.HReceiverId.HasValue == true)
                    userIds.Add(report.HMessage.HReceiverId.Value);
                if (report.HAuthorId.HasValue)
                    userIds.Add(report.HAuthorId.Value);
                if (report.HAdminId.HasValue)
                    userIds.Add(report.HAdminId.Value);
            }

            // 查詢用戶資訊
            var tenants = await _context.HTenants
                .Where(t => userIds.Contains(t.HTenantId) && t.HStatus == "已驗證")
                .ToDictionaryAsync(t => t.HTenantId, t => t.HUserName);

            var landlords = await _context.HLandlords
                .Where(l => userIds.Contains(l.HTenantId) && l.HStatus == "已驗證")
                .ToDictionaryAsync(l => l.HTenantId, l => l.HLandlordName);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Tenants = tenants;
            ViewBag.Landlords = landlords;

            return View(reports);
        }

        /// <summary>
        /// 根據訊息ID進入私人聊天
        /// </summary>
        [HttpGet]
        [Route("Admin/PrivateMessages/PrivateChatByMessage/{messageId}")]
        public async Task<IActionResult> PrivateChatByMessage(int messageId)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var message = await _context.HMessages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            return RedirectToAction("PrivateChat", new
            {
                senderId = message.HSenderId,
                receiverId = message.HReceiverId,
                senderRole = message.HSenderRole,
                receiverRole = message.HReceiverRole
            });
        }
    }
}
