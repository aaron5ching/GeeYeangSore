using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GuideController : ControllerBase
{
    private readonly GeeYeangSoreContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public GuideController(GeeYeangSoreContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet("guide")]
    public IActionResult GetGuideData()
    {
        var data = _db.HGuides.ToList();

        foreach (var item in data)
        {
            if (!string.IsNullOrEmpty(item.HImagePath))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, item.HImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    var imageBytes = System.IO.File.ReadAllBytes(filePath);
                    item.HImagePath = Convert.ToBase64String(imageBytes);
                }
            }
        }

        return Ok(new { response = data });
    }
    [HttpGet]
    public IActionResult GetGuideImage()
    {
        var data = _db.HGuides.ToList();

        return Ok(new { response = data });
    }
}