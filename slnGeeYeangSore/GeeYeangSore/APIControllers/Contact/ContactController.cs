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
    public IActionResult GetNews(object data)
    {

        Console.WriteLine(data);
        return Ok(new { response = data});
    }
    
    
}