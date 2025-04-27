using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using GeeYeangSore.Models;

namespace GeeYeangSore.Controllers
{
    public class SuperController : Controller
    {
        public string? LoginedUser => HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
        public string? LoginedRole => HttpContext.Session.GetString(CDictionary.SK_LOGINED_ROLE);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!HttpContext.Session.Keys.Contains(CDictionary.SK_LOGINED_USER))
            {
                // 未登入的用戶重定向到首頁，而不是登入頁面
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    area = "Admin",
                    controller = "Home",
                    action = "Index"
                }));
            }
        }

        //  權限檢查：單一角色
        public bool HasRole(string roleName)
        {
            return LoginedRole == roleName;
        }

        //  權限檢查：任一角色符合即可
        public bool HasAnyRole(params string[] roles)
        {
            return roles.Contains(LoginedRole);
        }
    }
}
