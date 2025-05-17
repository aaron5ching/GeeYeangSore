using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GeeYeangSore.APIControllers.Chat
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : BaseController
    {
        public ChatController(GeeYeangSoreContext db) : base(db) { }
        [HttpGet("chatlist")]
        public async Task<IActionResult> GetChatList()
        {
            try
            {
                // 驗證登入與權限
                var access = CheckAccess();
                if (access != null) return access;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });

                var userId = tenant.HTenantId;

                // 查詢：與我有關的訊息 → 分組每個對話對象 → 各取最新一筆
                var latestMessages = await _db.HMessages
                    .Where(m => m.HSenderId == userId || m.HReceiverId == userId)
                    .GroupBy(m => m.HSenderId == userId ? m.HReceiverId : m.HSenderId)
                    .Select(g => g.OrderByDescending(m => m.HTimestamp).Select(m => new
                    {
                        m.HMessageId,
                        ContactId = m.HSenderId == userId ? m.HReceiverId : m.HSenderId,
                        m.HContent,
                        m.HTimestamp
                    }).FirstOrDefault())
                    .OrderByDescending(m => m.HTimestamp)
                    .ToListAsync();

                // 擷取所有聯絡人 ID，查詢對應名稱
                var contactIds = latestMessages
                    .Select(m => m.ContactId ?? 0)
                    .Distinct()
                    .ToList();

                var contactNames = await _db.HTenants
                    .Where(t => contactIds.Contains(t.HTenantId))
                    .ToDictionaryAsync(t => t.HTenantId, t => t.HUserName);

                // 組合回傳結果
                var result = latestMessages.Select(m => new
                {
                    m.HMessageId,
                    HSenderId = m.ContactId,
                    SenderName = contactNames.ContainsKey(m.ContactId ?? 0)
                        ? contactNames[m.ContactId ?? 0]
                        : $"聯絡人{m.ContactId}",
                    m.HContent,
                    m.HTimestamp
                }).ToList();

                return Ok(new { success = true, data = result, selfId = userId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }

        [HttpGet("history/{otherId}")]
        public async Task<IActionResult> GetChatHistory(int otherId, int? skip = 0, int? take = 20)
        {
            try
            {
                var access = CheckAccess();
                if (access != null) return access;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });

                var userId = tenant.HTenantId;
                var skipCount = Math.Max(skip ?? 0, 0);
                var takeCount = Math.Clamp(take ?? 20, 10, 100); // 限制最大查詢筆數

                // 查詢雙向訊息（先抓最新 N 筆，前端再排序成舊到新）
                var messages = await _db.HMessages
                    .Where(m =>
                        (m.HSenderId == userId && m.HReceiverId == otherId) ||
                        (m.HSenderId == otherId && m.HReceiverId == userId))
                    .OrderByDescending(m => m.HTimestamp) // 最新在前
                    .Skip(skipCount)
                    .Take(takeCount)
                    .Select(m => new
                    {
                        m.HMessageId,
                        m.HSenderId,
                        m.HReceiverId,
                        m.HMessageType,
                        m.HContent,
                        m.HAttachmentUrl,
                        m.HTimestamp,
                        m.HIsRead,
                        m.HReplyToMessageId,
                        m.HIsEdited
                    })
                    .ToListAsync();

                // 前端顯示順序：從舊到新（所以這裡要 reverse）
                var orderedMessages = messages.OrderBy(m => m.HTimestamp).ToList();

                return Ok(new
                {
                    success = true,
                    data = orderedMessages,
                    hasMore = messages.Count == takeCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var access = CheckAccess();
                if (access != null) return access;

                var user = GetCurrentTenant();
                if (user == null) return Unauthorized(new { success = false, message = "未登入" });

                var userId = user.HTenantId;

                var unreadCounts = await _db.HMessages
                    .Where(m => m.HReceiverId == userId && m.HIsRead == 0)
                    .GroupBy(m => m.HSenderId)
                    .Select(g => new
                    {
                        SenderId = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = unreadCounts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] int senderId)
        {
            try
            {
                var access = CheckAccess();
                if (access != null) return access;

                var user = GetCurrentTenant();
                if (user == null) return Unauthorized(new { success = false, message = "未登入" });

                var userId = user.HTenantId;

                var unreadMessages = await _db.HMessages
                    .Where(m => m.HSenderId == senderId && m.HReceiverId == userId && m.HIsRead == 0)
                    .ToListAsync();

                foreach (var msg in unreadMessages)
                    msg.HIsRead = 1;

                await _db.SaveChangesAsync();

                return Ok(new { success = true, updated = unreadMessages.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }
    }
}
