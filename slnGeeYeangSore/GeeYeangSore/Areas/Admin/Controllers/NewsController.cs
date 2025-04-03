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


        [HttpPost]
        public IActionResult UpdateNews(string H, string HContent, IFormFile image)
        {

        }

        [HttpPost]
        public IActionResult AddNews(string HTitle, string HContent, IFormFile image)
        {
            const string imagePath= "\\images\\News";
            string imageName = Guid.NewGuid() + Path.GetExtension(image.FileName);

            string rootPath = _env.WebRootPath+ imagePath;

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            string filePath = Path.Combine(rootPath, imageName);

            string sqlPath = $"wwwroot\\News\\{imageName}";
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }

            HNews news = new HNews
            {
                HTitle = HTitle,
                HContent = HContent,
                HImagePath = imagePath +"\\"+ imageName,
                HCreatedAt = DateTime.Now,
                HUpdatedAt = DateTime.Now
            };
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
