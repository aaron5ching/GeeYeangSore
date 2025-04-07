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
    /// </summary>
    [Area("Admin")]
    public class PrivateMessagesController : SuperController
    {
        // 注入資料庫上下文
        private readonly GeeYeangSoreContext _context;
        // 設定每頁顯示10筆資料
        private const int PageSize = 10;

        // 建構函數，通過依賴注入獲取資料庫上下文
        public PrivateMessagesController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 顯示私人訊息列表
        /// </summary>
        /// <param name="searchString">搜尋關鍵字</param>
        /// <param name="page">當前頁碼，預設為第1頁</param>
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 建立基礎查詢，只顯示有效的私人對話
            var query = _context.HMessages
                .Where(m =>
                    m.HChatId == null && // 只顯示私人對話
                    m.HReceiverId != null && m.HReceiverId != 0); // 接收者必須存在且不為0

            // 如果有搜尋關鍵字，則進行篩選
            if (!string.IsNullOrEmpty(searchString?.Trim()))
            {
                var trimmedSearch = searchString.Trim();
                query = query.Where(m =>
                    m.HContent.Contains(trimmedSearch) ||      // 搜尋訊息內容
                    m.HSenderRole.Contains(trimmedSearch) ||   // 搜尋發送者角色
                    m.HReceiverRole.Contains(trimmedSearch)    // 搜尋接收者角色
                );
            }

            // 計算總筆數和總頁數
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // 取得分頁後的資料
            var messages = await query
                .OrderByDescending(m => m.HTimestamp)  // 依時間降序排序
                .Skip((page - 1) * PageSize)          // 跳過前面頁數的資料
                .Take(PageSize)                       // 取得當前頁的資料
                .ToListAsync();

            // 設定ViewBag資料供視圖使用
            ViewBag.CurrentPage = page;               // 當前頁碼
            ViewBag.TotalPages = totalPages;          // 總頁數
            ViewBag.SearchString = searchString;      // 搜尋關鍵字

            // 返回視圖，並傳入訊息列表
            return View(messages);
        }

        [HttpPost]
        //防止跨站請求偽造攻擊
        [ValidateAntiForgeryToken]
        // async/await 非同步版delete寫法
        public async Task<IActionResult> Delete(int id)
        {

            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var message = await _context.HMessages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            //記錄「我要刪除這筆」，還沒有真的刪除到資料庫
            _context.HMessages.Remove(message);
            //把剛剛註冊的刪除動作，真正同步到資料庫
            await _context.SaveChangesAsync();

            //刪除成功提示
            TempData["SuccessMessage"] = "刪除成功！";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("Admin/PrivateMessages/PrivateChat/{senderId}/{receiverId}")]
        public async Task<IActionResult> PrivateChat(int senderId, int receiverId)
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 獲取特定發送者和接收者之間的私人訊息（HChatId 為 null）
            var messages = await _context.HMessages
                .Where(m =>
                    m.HChatId == null && // 只顯示私人對話
                     m.HSenderId != null &&  // 發送者存在
                     m.HReceiverId != null && m.HReceiverId != 0 &&    // 接收者存在
                    ((m.HSenderId == senderId && m.HReceiverId == receiverId) ||
                    (m.HSenderId == receiverId && m.HReceiverId == senderId)))
                .OrderBy(m => m.HTimestamp)
                .ToListAsync();

            // 設定 ViewBag 資料
            ViewBag.SenderId = senderId;
            ViewBag.ReceiverId = receiverId;
            ViewBag.MessageCount = messages.Count;

            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //檢舉功能
        public async Task<IActionResult> Report(int messageId, string reason)
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 檢查訊息是否存在
            var message = await _context.HMessages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            // 創建新的檢舉記錄
            var report = new HReport
            {
                HMessageId = messageId,
                HAuthorId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"),
                HAuthorType = "Admin", // 檢舉者類型為管理員
                HReason = reason,
                HStatus = "Pending", // 初始狀態為待處理
                HCreatedAt = DateTime.Now,
                HReportType = "Private"
            };

            // 將檢舉記錄添加到資料庫
            _context.HReports.Add(report);
            await _context.SaveChangesAsync();

            // 設置成功訊息
            TempData["SuccessMessage"] = "檢舉已成功提交！";

            // 重定向回列表頁面
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // 處理檢舉
        public async Task<IActionResult> ProcessReport(int reportId, string status)
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 檢查檢舉記錄是否存在
            var report = await _context.HReports.FindAsync(reportId);
            if (report == null)
            {
                return NotFound();
            }

            // 更新檢舉狀態和處理資訊
            report.HStatus = status;
            report.HAdminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            report.HReviewedAt = DateTime.Now;

            // 保存更改
            await _context.SaveChangesAsync();

            // 設置成功訊息
            TempData["SuccessMessage"] = "檢舉已成功處理！";

            // 重定向回檢舉列表頁面
            return RedirectToAction(nameof(ReportList));
        }
        //檢舉列表頁面
        public async Task<IActionResult> ReportList()
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            //  獲取所有私人訊息的檢舉記錄
            var reports = await _context.HReports
                .Where(r => r.HReportType == "Private") // 只顯示私人訊息的檢舉
                .OrderByDescending(r => r.HCreatedAt)
                .ToListAsync();

            // 獲取所有相關的訊息
            var messageIds = reports.Select(r => r.HMessageId).ToList();
            var messages = await _context.HMessages
                .Where(m => messageIds.Contains(m.HMessageId))
                .ToDictionaryAsync(m => m.HMessageId);

            // 將訊息資訊存儲在 ViewBag 中
            ViewBag.Messages = messages;

            return View(reports);
        }

    }
}
