using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.Faq;

[ApiController]
[Route("api/[controller]")]
public class FaqController: ControllerBase
{
    private readonly GeeYeangSoreContext _db;

    public FaqController(GeeYeangSoreContext db)

    {
        _db = db;
    }
    
    [HttpGet("faq")]
    public IActionResult GetNews()
    {
        var news = _db.HAbouts.ToList();
        
        return Ok(new { response = news});
    }
    
}