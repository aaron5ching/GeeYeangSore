using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]

    public class NewsController : Controller
    {
        private readonly Models.GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public NewsController(Models.GeeYeangSoreContext db, IWebHostEnvironment env)
        {
            _db=db;
            _env = env;

        }

        //https://localhost:7022/Admin/News/News
        public IActionResult News()
        {
            var news = _db.HNews.ToList();
            return View(news);
        }

        //[HttpPost]
        //public IActionResult DeleteNews(int HNewsId)
        //{

        //}


        [HttpPost]
        public IActionResult UpdateNews(int HNewsId, string HContent, IFormFile image,string type)
        {
            Console.WriteLine("TEST!");
            var contact = _db.HNews.FirstOrDefault(n => n.HNewsId == HNewsId);
            if (type== "修改文章")
            {


                if (contact != null)
                {

                    contact.HContent = HContent;
                    contact.HUpdatedAt = DateTime.Now;

                    if (image != null)
                    {

                        var filePath = Path.Combine("wwwroot", "images", image.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }

                        contact.HImagePath = filePath;
                    }


                }

            }
            else
            {
                _db.Remove(contact);
                
            }

            _db.SaveChanges();

            return Redirect("News");

        }

        [HttpPost]
        public IActionResult News(string HTitle, string HContent, IFormFile image)
        {
            HNews news = new HNews
            {
                HTitle = HTitle,
                HContent = HContent,
                //HImagePath = imagePath +"\\"+ imageName,
                HCreatedAt = DateTime.Now,
                HUpdatedAt = DateTime.Now
            };

            if (image!=null)
            {
                const string imagePath = "/images/News";
                string imageName = Guid.NewGuid() + Path.GetExtension(image.FileName);

                string rootPath = _env.WebRootPath + imagePath;

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                string filePath = Path.Combine(rootPath, imageName);

                string sqlPath = $"wwwroot/News/{imageName}";

                if (image != null)
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }
                }
                news.HImagePath = imagePath + "/" + imageName;
            }
            
            _db.HNews.Add(news);
            _db.SaveChanges();
            return Redirect("News");
        }

        //https://localhost:7022/Admin/News/Test
        public void Test()
        {
            Console.WriteLine("Test!!");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
