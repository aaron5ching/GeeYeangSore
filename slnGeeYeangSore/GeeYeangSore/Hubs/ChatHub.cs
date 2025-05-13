using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace GeeYeangSore.Hubs
{
    public class ChatHub : Hub
    {
        // 用戶連線 ID 列表
        public static List<string> ConnIDList = new List<string>();

        /// <summary>
        /// 連線事件，驗證 Session 是否已登入
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var email = httpContext?.Session.GetString("SK_LOGINED_USER");
                if (string.IsNullOrEmpty(email))
                {
                    Context.Abort();
                    return;
                }
                if (ConnIDList.Where(p => p == Context.ConnectionId).FirstOrDefault() == null)
                {
                    ConnIDList.Add(Context.ConnectionId);
                }
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                // 記錄伺服器錯誤
                Console.WriteLine($"[SignalR] OnConnectedAsync 伺服器錯誤: {ex.Message}");
                // 回傳友善錯誤訊息給自己
                await Clients.Caller.SendAsync("ReceiveError", "伺服器發生錯誤，請稍後再試。");
            }
        }

        /// <summary>
        /// 離線事件
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            try
            {
                string id = ConnIDList.Where(p => p == Context.ConnectionId).FirstOrDefault();
                if (id != null)
                {
                    ConnIDList.Remove(id);
                }
                await base.OnDisconnectedAsync(ex);
            }
            catch (Exception ex2)
            {
                // 記錄伺服器錯誤
                Console.WriteLine($"[SignalR] OnDisconnectedAsync 伺服器錯誤: {ex2.Message}");
                await Clients.Caller.SendAsync("ReceiveError", "伺服器發生錯誤，請稍後再試。");
            }
        }

        /// <summary>
        /// 發送訊息（僅支援一對一），未登入不處理
        /// </summary>
        /// <param name="fromId">發送者ID</param>
        /// <param name="toId">接收者ID</param>
        /// <param name="text">訊息內容</param>
        public async Task SendMessage(string fromId, string toId, string text)
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var email = httpContext?.Session.GetString("SK_LOGINED_USER");
                if (string.IsNullOrEmpty(email))
                {
                    // 未登入，不處理
                    return;
                }
                var msgObj = new
                {
                    id = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), // 前端用 id
                    from = fromId,
                    to = toId,
                    text = text,
                    time = System.DateTime.Now.ToString("HH:mm")
                };
                if (!string.IsNullOrEmpty(toId))
                {
                    // 傳給對方
                    await Clients.User(toId).SendAsync("ReceiveMessage", msgObj);
                    // 傳給自己（同步顯示）
                    await Clients.User(fromId).SendAsync("ReceiveMessage", msgObj);
                }
            }
            catch (Exception ex)
            {
                // 記錄伺服器錯誤
                Console.WriteLine($"[SignalR] SendMessage 伺服器錯誤: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", "伺服器發生錯誤，請稍後再試。");
            }
        }
    }
}