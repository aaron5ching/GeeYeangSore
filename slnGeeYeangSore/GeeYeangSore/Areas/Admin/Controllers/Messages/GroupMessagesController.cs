using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GeeYeangSore.Models;
using GeeYeangSore.Controllers;
using System.Collections.Generic;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    /// <summary>
    /// 群組訊息控制器
    /// 負責處理群組聊天的相關功能，包括：
    /// 1. 群組列表顯示
    /// 2. 群組聊天內容查看
    /// 3. 群組訊息刪除
    /// </summary>
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")]
    public class GroupMessagesController : SuperController
    {
        private readonly GeeYeangSoreContext _context;
        private const int PageSize = 10;

        public GroupMessagesController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 群組列表頁面
        /// </summary>
        /// <param name="searchString">搜尋關鍵字</param>
        /// <param name="page">當前頁碼</param>
        /// <returns>群組列表視圖</returns>
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 只顯示群組對話
            var query = _context.HMessages
                .Where(m => m.HChatId != null);

            // 如果有搜尋關鍵字，過濾訊息內容
            if (!string.IsNullOrEmpty(searchString?.Trim()))
            {
                var trimmedSearch = searchString.Trim();
                query = query.Where(m => m.HContent.Contains(trimmedSearch));
            }

            // 計算總頁數
            var totalItems = await query.Select(m => m.HChatId).Distinct().CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // 獲取當前頁的群組列表
            var chatGroups = await query
                .GroupBy(m => m.HChatId)
                .Select(g => new
                {
                    ChatId = g.Key,
                    LastMessageTime = g.Max(m => m.HTimestamp)
                })
                .OrderByDescending(g => g.LastMessageTime)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(chatGroups);
        }

        /// <summary>
        /// 群組聊天內容頁面
        /// </summary>
        /// <param name="chatId">群組ID</param>
        /// <returns>群組聊天內容視圖</returns>
        [HttpGet]
        [Route("Admin/GroupMessages/GroupChat/{chatId}")]
        public async Task<IActionResult> GroupChat(int chatId)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 獲取該群組的所有訊息
            var messages = await _context.HMessages
                .Where(m => m.HChatId == chatId)
                .OrderBy(m => m.HTimestamp)
                .ToListAsync();

            // 獲取所有發送者的名稱
            var senderNames = new Dictionary<int, string>();
            foreach (var message in messages)
            {
                if (message.HSenderId.HasValue)
                {
                    // 先檢查是否為房客
                    var tenant = await _context.HTenants
                        .FirstOrDefaultAsync(t => t.HTenantId == message.HSenderId);

                    // 再檢查是否為房東
                    var landlord = await _context.HLandlords
                        .FirstOrDefaultAsync(l => l.HLandlordId == message.HSenderId);

                    // 根據角色和找到的資料決定顯示的名稱
                    if (message.HSenderRole == "Tenant" && tenant != null)
                    {
                        senderNames[message.HMessageId] = tenant.HUserName;
                    }
                    else if (message.HSenderRole == "Landlord" && landlord != null)
                    {
                        senderNames[message.HMessageId] = landlord.HLandlordName;
                    }
                    else if (tenant != null)
                    {
                        senderNames[message.HMessageId] = tenant.HUserName;
                    }
                    else if (landlord != null)
                    {
                        senderNames[message.HMessageId] = landlord.HLandlordName;
                    }
                    else
                    {
                        senderNames[message.HMessageId] = "系統";
                    }
                }
                else
                {
                    senderNames[message.HMessageId] = "系統";
                }
            }

            ViewBag.ChatId = chatId;
            ViewBag.MessageCount = messages.Count;
            ViewBag.SenderNames = senderNames;

            return View(messages);
        }

        /// <summary>
        /// 刪除群組訊息
        /// </summary>
        /// <param name="chatId">要刪除的群組ID</param>
        /// <returns>JSON格式的操作結果</returns>
        [HttpPost]
        [Route("Admin/GroupMessages/Delete/{chatId}")]
        public async Task<IActionResult> Delete(int chatId)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return Json(new { success = false, message = "權限不足" });

            try
            {
                // 先刪除該群組的所有訊息
                var messages = await _context.HMessages
                    .Where(m => m.HChatId == chatId)
                    .ToListAsync();

                _context.HMessages.RemoveRange(messages);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "群組已成功刪除" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除失敗：" + ex.Message });
            }
        }
    }
}
