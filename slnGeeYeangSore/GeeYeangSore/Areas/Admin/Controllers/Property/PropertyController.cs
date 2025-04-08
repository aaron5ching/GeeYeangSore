using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace GeeYeangSore.Areas.Admin.Controllers.Property
{
    /// <summary>
    /// 房源管理控制器
    /// </summary>
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class PropertyController : Controller
    {
        // 資料庫上下文
        private readonly GeeYeangSoreContext _context;
        private readonly ILogger<PropertyController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // 建構函式，注入資料庫上下文
        public PropertyController(GeeYeangSoreContext context, ILogger<PropertyController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 房源列表頁面，支援搜尋功能
        /// </summary>
        /// <param name="searchString">搜尋關鍵字</param>
        /// <param name="searchType">搜尋類型</param>
        /// <param name="page">當前頁碼</param>
        /// <returns>房源列表視圖</returns>
        public async Task<IActionResult> Index(string searchString, string searchType, int page = 1)
        {
            try
            {
                int pageSize = 15; // 每頁15筆資料

                var properties = _context.HProperties
                    .Include(p => p.HLandlord)
                    .Where(p => p.HLandlord.HStatus == "已驗證")  // 只顯示已驗證房東的房源
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim();
                    switch (searchType?.ToLower())
                    {
                        case "title":
                            properties = properties.Where(p => p.HPropertyTitle.Contains(searchString));
                            break;
                        case "address":
                            properties = properties.Where(p => p.HAddress.Contains(searchString));
                            break;
                        case "city":
                            properties = properties.Where(p => p.HCity.Contains(searchString));
                            break;
                        case "district":
                            properties = properties.Where(p => p.HDistrict.Contains(searchString));
                            break;
                        case "landlord":
                            properties = properties.Where(p => p.HLandlord.HLandlordName.Contains(searchString));
                            break;
                        default: // "all" or any other value
                            properties = properties.Where(p => 
                                p.HPropertyTitle.Contains(searchString) ||
                                p.HAddress.Contains(searchString) ||
                                p.HCity.Contains(searchString) ||
                                p.HDistrict.Contains(searchString) ||
                                p.HLandlord.HLandlordName.Contains(searchString));
                            break;
                    }
                }

                ViewData["CurrentFilter"] = searchString;
                ViewData["SearchType"] = searchType;

                var result = await properties.ToListAsync();
                return View(result.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving properties");
                return View("Error");
            }
        }

        /// <summary>
        /// 顯示房源詳細資訊
        /// </summary>
        /// <param name="id">房源ID</param>
        /// <returns>房源詳細資訊視圖</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var property = await _context.HProperties
                    .Include(p => p.HLandlord)
                    .FirstOrDefaultAsync(m => m.HPropertyId == id);
                
                if (property == null)
                {
                    return NotFound();
                }

                return View(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving property details");
                return View("Error");
            }
        }

        /// <summary>
        /// 顯示新增房源頁面
        /// </summary>
        public IActionResult Create()
        {
            try
            {
                ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing create view");
                return View("Error");
            }
        }

        /// <summary>
        /// 處理新增房源的請求
        /// </summary>
        /// <param name="property">房源資料</param>
        /// <param name="features">房源特色</param>
        /// <param name="images">房源照片</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("HLandlordId,HPropertyTitle,HRentPrice,HAddress,HCity,HDistrict,HZipcode,HPropertyType,HRoomCount,HBathroomCount,HArea,HFloor,HTotalFloors,HDescription,HAvailabilityStatus,HBuildingType,HScore,HIsVip,HIsShared")] HProperty property,
            HPropertyFeature features,
            List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 設置創建時間和更新時間
                    property.HPublishedDate = DateTime.Now;
                    property.HLastUpdated = DateTime.Now;
                    
                    // 設置其他預設值
                    property.HScore = string.IsNullOrEmpty(property.HScore) ? "0" : property.HScore;
                    property.HIsVip = property.HIsVip ?? false;
                    property.HIsShared = property.HIsShared ?? false;
                    property.HAvailabilityStatus = string.IsNullOrEmpty(property.HAvailabilityStatus) ? "可租" : property.HAvailabilityStatus;

                    // 驗證房東是否存在且已驗證
                    var landlord = await _context.HLandlords
                        .FirstOrDefaultAsync(l => l.HLandlordId == property.HLandlordId && l.HStatus == "已驗證");
                    
                    if (landlord == null)
                    {
                        ModelState.AddModelError("HLandlordId", "選擇的房東不存在或未驗證");
                        ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName");
                        return View(property);
                    }

                    // 開始交易
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // 保存房源基本資訊
                            _context.Add(property);
                            await _context.SaveChangesAsync();

                            // 設置房源特色的關聯
                            features.HPropertyId = property.HPropertyId;
                            features.HLandlordId = property.HLandlordId;

                            // 保存房源特色
                            _context.Add(features);
                            await _context.SaveChangesAsync();

                            // 處理照片上傳
                            if (images != null && images.Count > 0)
                            {
                                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Property");
                                if (!Directory.Exists(uploadsFolder))
                                {
                                    Directory.CreateDirectory(uploadsFolder);
                                }

                                foreach (var image in images)
                                {
                                    if (image.Length > 0)
                                    {
                                        // 生成唯一的檔案名
                                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                        // 保存檔案
                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await image.CopyToAsync(fileStream);
                                        }

                                        // 創建圖片記錄
                                        var propertyImage = new HPropertyImage
                                        {
                                            HPropertyId = property.HPropertyId,
                                            HLandlordId = property.HLandlordId,
                                            HImageUrl = "/images/Property/" + uniqueFileName,
                                            HUploadedDate = DateTime.Now,
                                            HLastUpDated = DateTime.Now
                                        };

                                        _context.HPropertyImages.Add(propertyImage);
                                    }
                                }
                                await _context.SaveChangesAsync();
                            }

                            // 提交交易
                            await transaction.CommitAsync();

                            // 設置成功訊息
                            TempData["SuccessMessage"] = "房源新增成功！";
                            return RedirectToAction(nameof(Index));
                        }
                        catch (Exception ex)
                        {
                            // 回滾交易
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Error occurred while creating property: {0}", ex.Message);
                            ModelState.AddModelError("", "創建房源時發生錯誤，請稍後再試");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating property: {0}", ex.Message);
                    ModelState.AddModelError("", "創建房源時發生錯誤，請稍後再試");
                }
            }
            else
            {
                // 如果有驗證錯誤，記錄錯誤信息
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("Validation error: {0}", error.ErrorMessage);
                    }
                }
            }

            // 如果新增失敗，重新載入房東列表並返回視圖
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName", property.HLandlordId);
            return View(property);
        }

        /// <summary>
        /// 顯示編輯房源頁面
        /// </summary>
        /// <param name="id">房源ID</param>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var property = await _context.HProperties
                    .Include(p => p.HLandlord)
                    .FirstOrDefaultAsync(m => m.HPropertyId == id);
                if (property == null)
                {
                    return NotFound();
                }
                ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName", property.HLandlordId);
                return View(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing edit view");
                return View("Error");
            }
        }

        /// <summary>
        /// 處理編輯房源的請求
        /// </summary>
        /// <param name="id">房源ID</param>
        /// <param name="property">更新後的房源資料</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HPropertyId,HLandlordId,HPropertyTitle,HRentPrice,HAddress,HCity,HDistrict,HZipcode,HPropertyType,HRoomCount,HBathroomCount,HArea,HFloor,HTotalFloors,HDescription,HAvailabilityStatus,HBuildingType,HScore,HIsVip,HIsShared")] HProperty property)
        {
            if (id != property.HPropertyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    property.HLastUpdated = DateTime.Now;
                    _context.Update(property);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!PropertyExists(property.HPropertyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error occurred while updating property");
                        ModelState.AddModelError("", "更新房源時發生錯誤，請稍後再試");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating property");
                    ModelState.AddModelError("", "更新房源時發生錯誤，請稍後再試");
                }
            }
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName", property.HLandlordId);
            return View(property);
        }

        /// <summary>
        /// 顯示刪除確認頁面
        /// </summary>
        /// <param name="id">房源ID</param>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var property = await _context.HProperties
                    .Include(p => p.HLandlord)
                    .FirstOrDefaultAsync(m => m.HPropertyId == id);
                if (property == null)
                {
                    return NotFound();
                }

                return View(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing delete view");
                return View("Error");
            }
        }

        /// <summary>
        /// 處理刪除房源的請求
        /// </summary>
        /// <param name="id">房源ID</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var property = await _context.HProperties.FindAsync(id);
                if (property == null)
                {
                    return NotFound();
                }

                _context.HProperties.Remove(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting property");
                return View("Error");
            }
        }

        /// <summary>
        /// 檢查房源是否存在
        /// </summary>
        /// <param name="id">房源ID</param>
        /// <returns>是否存在</returns>
        private bool PropertyExists(int id)
        {
            return _context.HProperties.Any(e => e.HPropertyId == id);
        }
    }
}

