using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.Contact;


[ApiController]
[Route("api/[controller]")]
public class ContactController : BaseController
{
    private readonly GeeYeangSoreContext _db;

    public ContactController(GeeYeangSoreContext db) : base(db)
    {
        
    }
    
    [HttpPost("contact")]
    public IActionResult GetContact([FromBody] HContact data)
    {
        data.HCreatedAt=DateTime.UtcNow;
        Console.WriteLine($"Email: {data.HEmail}, Phone: {data.HPhoneNumber}, Title: {data.HTitle}, Message: {data.HReplyContent}");
        
        _db.HContacts.Add(data);

        _db.SaveChangesAsync();
        
        return Ok(new { response = "資料庫寫入完成", data });
    }
    
}