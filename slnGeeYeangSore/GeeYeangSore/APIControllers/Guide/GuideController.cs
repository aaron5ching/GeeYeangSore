using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.Guide;

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


        for (int i = 0; i < data.Count; i++)
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, data[i].HImagePath.TrimStart('/'));
            var imageBytes = System.IO.File.ReadAllBytes(filePath);
            data[i].HImagePath = System.Convert.ToBase64String(imageBytes);
        }

        Console.WriteLine(data);

        return Ok(new { response = data });
    }
     [HttpGet]
    public IActionResult GetGuideImage()
    {
        var data = _db.HGuides.ToList();

        return Ok(new { response = data });
    }
}