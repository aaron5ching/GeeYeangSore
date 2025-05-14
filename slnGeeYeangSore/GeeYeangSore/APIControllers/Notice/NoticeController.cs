using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.News;

[ApiController]
[Route("api/[controller]")]
public class NoticeController : ControllerBase
{
    private readonly GeeYeangSoreContext _db;

    public NoticeController(GeeYeangSoreContext db)
    {
        _db = db;
    }

    [HttpGet("news")]
    public IActionResult GetNews()
    {
        var news = _db.HNews.ToList();
        
        return Ok(new { response = news});
    }
    
    
}