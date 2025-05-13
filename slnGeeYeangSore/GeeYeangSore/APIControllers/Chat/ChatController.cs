using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
            //自動檢查登入、黑名單、房東身分
            var access = CheckAccess();
            if (access != null) return access;

            var tenant = GetCurrentTenant();
            if (tenant == null)
                return Unauthorized(new { success = false, message = "未登入" });
            var userId = tenant.HTenantId;

            // 方法1：先查詢所有訊息再分組，適合少量資料
            var allMessages = await _db.HMessages
                .Where(m => m.HReceiverId == userId)
                .OrderByDescending(m => m.HTimestamp)
                .ToListAsync();

            var latestMessages = allMessages
                .GroupBy(m => m.HSenderId)
                .Select(g => g.First())
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

            // 依據HSenderId join HTenant取得名稱
            var contactIds = latestMessages.Select(m => m.HSenderId).Distinct().ToList();
            var contactNames = _db.HTenants
                .Where(t => contactIds.Contains(t.HTenantId))
                .ToDictionary(t => t.HTenantId, t => t.HUserName);

            var result = latestMessages.Select(m => new
            {
                m.HMessageId,
                m.HSenderId,
                //將id改成username
                SenderName = contactNames.ContainsKey(m.HSenderId ?? 0) ? contactNames[m.HSenderId ?? 0] : $"聯絡人{m.HSenderId}",
                m.HContent,
                m.HTimestamp,
                m.HReceiverId
            }).ToList();

            return Ok(new { success = true, data = result });
        }

        // 取得與指定對象的聊天紀錄
        [HttpGet("history/{otherId}")]
        public async Task<IActionResult> GetChatHistory(int otherId)
        {
            //自動檢查登入、黑名單、房東身分
            var access = CheckAccess();
            if (access != null) return access;

            var tenant = GetCurrentTenant();
            var userId = tenant.HTenantId;

            // 查詢與指定對象的所有訊息（雙向）
            var messages = await _db.HMessages
                .Where(m => (m.HSenderId == userId && m.HReceiverId == otherId) || (m.HSenderId == otherId && m.HReceiverId == userId))
                .OrderBy(m => m.HTimestamp)
                .ToListAsync();

            return Ok(new { success = true, data = messages });
        }
    }
}
