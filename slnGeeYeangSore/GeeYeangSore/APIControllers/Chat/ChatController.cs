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

        // 取得目前登入者的聊天室列表
        [HttpGet("chatlist")]
        public async Task<IActionResult> GetChatList()
        {
            try
            {
                //自動檢查登入、黑名單、房東身分
                var access = CheckAccess();
                if (access != null) return access;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });
                var userId = tenant.HTenantId;

                // 方法1：先查詢所有訊息再分組，適合少量資料
                var allMessages = await _db.HMessages
    .Where(m => m.HReceiverId == userId || m.HSenderId == userId)
    .OrderByDescending(m => m.HTimestamp)
    .ToListAsync();

                var latestMessages = allMessages
     .GroupBy(m => m.HSenderId == userId ? m.HReceiverId : m.HSenderId)
     .Select(g => g.OrderByDescending(m => m.HTimestamp).First())
     .OrderByDescending(m => m.HTimestamp)
     .ToList();

                //方法二（用匿名型別包裹），適合大量資料
                // var latestMessages = await _db.HMessages
                //     .Where(m => m.HReceiverId == userId)
                //     .GroupBy(m => m.HSenderId)
                //     .Select(g => new
                //     {
                //         Message = g.OrderByDescending(m => m.HTimestamp).FirstOrDefault()
                //     })
                //     .ToListAsync();

                // var result = latestMessages
                //     .Select(x => x.Message)
                //     .OrderByDescending(m => m.HTimestamp)
                //     .ToList();

                // 依據HSenderId join HTenant取得名稱
                var contactIds = latestMessages
    .Select(m => m.HSenderId == userId ? m.HReceiverId : m.HSenderId)
    .Distinct()
    .ToList();

                var contactNames = _db.HTenants
                    .Where(t => contactIds.Contains(t.HTenantId))
                    .ToDictionary(t => t.HTenantId, t => t.HUserName);

                var result = latestMessages.Select(m =>
                {
                    var targetId = m.HSenderId == userId ? m.HReceiverId : m.HSenderId;
                    return new
                    {
                        m.HMessageId,
                        HSenderId = targetId,
                        SenderName = contactNames.ContainsKey(targetId ?? 0) ? contactNames[targetId ?? 0] : $"聯絡人{targetId}",
                        m.HContent,
                        m.HTimestamp,
                        m.HReceiverId
                    };
                }).ToList();

                return Ok(new { success = true, data = result, selfId = userId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }

        // 取得與指定對象的聊天紀錄
        [HttpGet("history/{otherId}")]
        public async Task<IActionResult> GetChatHistory(int otherId)
        {
            try
            {
                //自動檢查登入、黑名單、房東身分
                var access = CheckAccess();
                if (access != null) return access;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });
                var userId = tenant.HTenantId;

                // 查詢與指定對象的所有訊息（雙向）
                var messages = await _db.HMessages
                    .Where(m => (m.HSenderId == userId && m.HReceiverId == otherId) || (m.HSenderId == otherId && m.HReceiverId == userId))
                    .OrderBy(m => m.HTimestamp)
                    .ToListAsync();

                return Ok(new { success = true, data = messages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }
    }
}
