using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
