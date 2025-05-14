using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.About;

[ApiController]
[Route("api/[controller]")]
public class AboutController: ControllerBase
{
    private readonly GeeYeangSoreContext _db;

    public AboutController(GeeYeangSoreContext db)

    {
        _db = db;
    }
    
    [HttpGet("about")]
    public IActionResult GetNews()
    {
        var news = _db.HAbouts.ToList();
        Console.WriteLine(news);
        return Ok(new { response = news});
    }
    
}