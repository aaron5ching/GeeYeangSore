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
        var about = _db.HAbouts.ToList();
        
        var username = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);

        var email = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
        var tenantId = HttpContext.Session.GetInt32("TenantId");

        Console.WriteLine(username);
        
        Console.WriteLine(email);
        
        Console.WriteLine(tenantId);
        return Ok(new { response = about});
    }
    
}