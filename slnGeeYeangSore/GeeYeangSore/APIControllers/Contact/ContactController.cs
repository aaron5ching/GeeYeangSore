using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.Contact;


[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly GeeYeangSoreContext _db;

    public ContactController(GeeYeangSoreContext db)
    {
        _db = db;
    }

    [HttpPost("contact")]
    public IActionResult GetContact([FromBody] HContact data)
    {
        Console.WriteLine("666");
        data.HCreatedAt=DateTime.UtcNow;
        Console.WriteLine($"Email: {data.HEmail}, Phone: {data.HPhoneNumber}, Title: {data.HTitle}, Message: {data.HReplyContent}");
        
        _db.HContacts.Add(data);

        _db.SaveChangesAsync();
        
        return Ok(new { response = "資料庫寫入完成", data });
    }
    
}