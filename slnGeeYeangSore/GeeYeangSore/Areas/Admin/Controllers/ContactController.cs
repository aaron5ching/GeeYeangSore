using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class ContactController : Controller
    {
        private readonly Models.GeeYeangSoreContext _db;


        public ContactController(Models.GeeYeangSoreContext db)
        {
            _db = db;

        }
        public IActionResult Index()
        {
            return View();
        }

        //https://localhost:7022/Admin/Contact/Contact
        public IActionResult Contact()
        {

            var contact = _db.HContacts.ToList();
            return View(contact);
        }

        [HttpPost]
        public IActionResult Contact(int HContactId,string HReplyContent)
        {
            Console.WriteLine("replay");

            var contact = _db.HContacts.FirstOrDefault(n => n.HContactId == HContactId);

            if (contact != null)
            {

                contact.HReplyAt= DateTime.Now;
                contact.HReplyContent = HReplyContent;
                contact.HStatus = true;


                _db.SaveChanges();
            }

            //確保不為model 不為null
            var contacts = _db.HContacts?.ToList() ?? new List<HContact>();
            return View(contacts);
        }
    }
}
