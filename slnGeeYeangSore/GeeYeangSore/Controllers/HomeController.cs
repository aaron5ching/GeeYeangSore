using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;

namespace GeeYeangSore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError($"首頁載入失敗: {ex.Message}");
            return RedirectToAction("Error");
        }
    }

    public IActionResult Privacy()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError($"隱私權政策頁面載入失敗: {ex.Message}");
            return RedirectToAction("Error");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        try
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        catch (Exception ex)
        {
            _logger.LogError($"錯誤頁面載入失敗: {ex.Message}");
            // 如果連錯誤頁面都載入失敗，返回最基本的錯誤訊息
            return Content("系統發生錯誤，請稍後再試。");
        }
    }
}
