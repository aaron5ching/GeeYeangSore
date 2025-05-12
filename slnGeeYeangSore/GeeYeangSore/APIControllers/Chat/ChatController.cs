using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GeeYeangSore.APIControllers.Chat
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly GeeYeangSoreContext _db;

        public ChatController(GeeYeangSoreContext db)
        {
            _db = db;
        }

        // 取得目前登入者的聊天室列表（只用HMessage，依發送者分組）
        [HttpGet("chatlist")]
        public async Task<IActionResult> GetChatList()
        {
            var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { success = false, message = "未登入" });
            var tenant = await _db.HTenants.FirstOrDefaultAsync(t => t.HEmail == email && !t.HIsDeleted);
            if (tenant == null)
                return Unauthorized(new { success = false, message = "帳號不存在" });
            var userId = tenant.HTenantId;

            // 查詢所有接收者是自己(userId)的訊息，依發送者分組，取每組最新一則
            var latestMessages = await _db.HMessages
                .Where(m => m.HReceiverId == userId)
                .GroupBy(m => m.HSenderId)
                .Select(g => g.OrderByDescending(m => m.HTimestamp).FirstOrDefault())
                .OrderByDescending(m => m.HTimestamp)
                .ToListAsync();

            return Ok(new { success = true, data = latestMessages });
        }

        // 取得與指定對象的聊天紀錄
        [HttpGet("history/{otherId}")]
        public async Task<IActionResult> GetChatHistory(int otherId)
        {
            var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { success = false, message = "未登入" });
            var tenant = await _db.HTenants.FirstOrDefaultAsync(t => t.HEmail == email && !t.HIsDeleted);
            if (tenant == null)
                return Unauthorized(new { success = false, message = "帳號不存在" });
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
