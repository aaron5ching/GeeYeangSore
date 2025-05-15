using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.Landlord
{
    public class Landlord : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
