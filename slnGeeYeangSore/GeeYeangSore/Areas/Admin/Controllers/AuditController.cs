using GeeYeangSore.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]

    public class AuditController : SuperController
    {

        private readonly Models.GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public AuditController(Models.GeeYeangSoreContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
            //0
        }


        public IActionResult Index()
        {
            //0
            return View();
        }

        //https://localhost:7022/Admin/Audit/Audit
        public IActionResult Audit()
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                //如果沒有權限就會顯示NoPermission頁面
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var contact = _db.HAudits.ToList();
            return View(contact);
        }
        [HttpPost]
        public IActionResult Audit(int HAuditId,string typeString)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                //如果沒有權限就會顯示NoPermission頁面
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            //return View("Audit");
            //（待審核/通過/退件）
            var audit = _db.HAudits.FirstOrDefault(item => item.HAuditId == HAuditId);
            if (audit != null)
            {
                    audit.HStatus = typeString;
                    audit.HReviewedAt = DateTime.Now;
                _db.SaveChanges();
            }
            var contact = _db.HAudits.ToList();
            return View(contact);

        }

        //[HttpPost]
        //public IActionResult AcceptApply(int )
        //{

        //}
        //[HttpPost]
        //public IActionResult RejectApply()
        //{

        //}
    }
}
