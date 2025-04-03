using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult NoPermission()
        {
            return View();
        }
    }
}
