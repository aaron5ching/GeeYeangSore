using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]

    public class AuditController : Controller
    {

        private readonly Models.GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public AuditController(Models.GeeYeangSoreContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;

        }


        public IActionResult Index()
        {
            return View();
        }

        //https://localhost:7022/Admin/Audit/Audit
        public IActionResult Audit()
        {

            var contact = _db.HAudits.ToList();
            return View(contact);
        }
        [HttpPost]
        public IActionResult Audit(int HAuditId,string typeString)
        {
            Console.WriteLine(HAuditId);
            Console.WriteLine(typeString);
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
