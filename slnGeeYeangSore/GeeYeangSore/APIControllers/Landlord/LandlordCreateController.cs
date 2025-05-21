using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using GeeYeangSore.DTO.Landlord;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GeeYeangSore.APIControllers.Landlord
{
    [ApiController]
    [Route("api/landlord/property")]
    public class LandlordPropertyController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<LandlordPropertyController> _logger;

        public LandlordPropertyController(GeeYeangSoreContext db, IWebHostEnvironment webHostEnvironment, ILogger<LandlordPropertyController> logger) : base(db)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // 取得目前登入房東的所有物件
        [HttpGet("my")]
        public IActionResult GetMyProperties()
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            var tenant = GetCurrentTenant();
            var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
            if (landlord == null)
                return Unauthorized(new { success = false, message = "找不到房東資料" });

            var properties = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Include(p => p.HPropertyFeatures)
                .Include(p => p.HAds)
                .Where(p => p.HLandlordId == landlord.HLandlordId && p.HIsDelete == false)
                .Select(p => new
                {
                    HPropertyId = p.HPropertyId,
                    HLandlordId = p.HLandlordId,
                    HPropertyTitle = p.HPropertyTitle,
                    HDescription = p.HDescription,
                    HAddress = p.HAddress,
                    HCity = p.HCity,
                    HDistrict = p.HDistrict,
                    HZipcode = p.HZipcode,
                    HRentPrice = p.HRentPrice,
                    HPropertyType = p.HPropertyType,
                    HRoomCount = p.HRoomCount,
                    HBathroomCount = p.HBathroomCount,
                    HArea = p.HArea,
                    HFloor = p.HFloor,
                    HTotalFloors = p.HTotalFloors,
                    HAvailabilityStatus = p.HAvailabilityStatus,
                    HBuildingType = p.HBuildingType,
                    HScore = p.HScore,
                    HPublishedDate = p.HPublishedDate,
                    HLastUpdated = p.HLastUpdated,
                    HIsVip = p.HIsVip,
                    HIsShared = p.HIsShared,
                    HStatus = p.HStatus,
                    HIsDelete = p.HIsDelete,
                    HLatitude = p.HLatitude,
                    HLongitude = p.HLongitude,
                    Feature = p.HPropertyFeatures
                        .Where(f => f.HIsDelete == false)
                        .Select(f => new
                        {
                            f.HFeaturePropertyId,
                            f.HLandlordId,
                            f.HPropertyId,
                            f.HAllowsDogs,
                            f.HAllowsCats,
                            f.HAllowsAnimals,
                            f.HAllowsCooking,
                            f.HHasFurniture,
                            f.HInternet,
                            f.HAirConditioning,
                            f.HSharedRental,
                            f.HTv,
                            f.HRefrigerator,
                            f.HWashingMachine,
                            f.HBed,
                            f.HWaterHeater,
                            f.HGasStove,
                            f.HCableTv,
                            f.HWaterDispenser,
                            f.HParking,
                            f.HSocialHousing,
                            f.HShortTermRent,
                            f.HPublicElectricity,
                            f.HPublicWatercharges,
                            f.HLandlordShared,
                            f.HBalcony,
                            f.HPublicEquipment,
                            f.HIsDelete
                        }).FirstOrDefault(),
                    Images = p.HPropertyImages
                        .Where(i => i.HIsDelete == false)
                        .OrderBy(i => i.HUploadedDate)
                        .Select(i => new
                        {
                            i.HImageId,
                            i.HPropertyId,
                            i.HLandlordId,
                            HImageUrl = (i.HImageUrl.StartsWith("http") ? i.HImageUrl : "https://localhost:7022" + i.HImageUrl),
                            i.HCaption,
                            i.HUploadedDate,
                            i.HLastUpDated,
                            i.HIsDelete
                        }).ToList(),
                    Ads = p.HAds
                        .Where(a => a.HIsDelete == false)
                        .Select(a => new
                        {
                            a.HAdId,
                            a.HLandlordId,
                            a.HPropertyId,
                            a.HAdName,
                            a.HDescription,
                            a.HCategory,
                            a.HPriority,
                            a.HAdTag,
                            a.HTargetRegion,
                            a.HStatus,
                            a.HAdPrice,
                            a.HLinkUrl,
                            a.HImageUrl,
                            a.HStartDate,
                            a.HEndDate,
                            a.HIsDelete,
                            a.HCreatedDate,
                            a.HLastUpdated
                        }).ToList()
                })
                .ToList();

            return Ok(properties);
        }

        // 取得單一物件詳細資料（用於編輯）
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProperty(int id)
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            try
            {
                var tenant = GetCurrentTenant();
                var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized(new { success = false, message = "找不到房東資料" });

                var property = await _db.HProperties
                    .Include(p => p.HPropertyImages)
                    .Include(p => p.HPropertyFeatures)
                    .FirstOrDefaultAsync(p => p.HPropertyId == id && p.HLandlordId == landlord.HLandlordId && p.HIsDelete == false);

                if (property == null)
                    return NotFound(new { success = false, message = "找不到物件" });

                var feature = property.HPropertyFeatures.FirstOrDefault(f => f.HIsDelete == false);
                var images = property.HPropertyImages.Where(i => i.HIsDelete == false).ToList();

                var result = new
                {
                    propertyId = property.HPropertyId,
                    title = property.HPropertyTitle,
                    description = property.HDescription,
                    address = property.HAddress,
                    city = property.HCity,
                    district = property.HDistrict,
                    zipcode = property.HZipcode,
                    rentPrice = property.HRentPrice,
                    propertyType = property.HPropertyType,
                    roomCount = property.HRoomCount,
                    bathroomCount = property.HBathroomCount,
                    area = property.HArea,
                    floor = property.HFloor,
                    totalFloors = property.HTotalFloors,
                    buildingType = property.HBuildingType,
                    availabilityStatus = property.HAvailabilityStatus,
                    score = property.HScore,
                    isVip = property.HIsVip,
                    isShared = property.HIsShared,
                    latitude = property.HLatitude,
                    longitude = property.HLongitude,
                    features = feature != null ? new
                    {
                        allowsDogs = feature.HAllowsDogs,
                        allowsCats = feature.HAllowsCats,
                        allowsAnimals = feature.HAllowsAnimals,
                        allowsCooking = feature.HAllowsCooking,
                        hasFurniture = feature.HHasFurniture,
                        internet = feature.HInternet,
                        airConditioning = feature.HAirConditioning,
                        sharedRental = feature.HSharedRental,
                        tv = feature.HTv,
                        refrigerator = feature.HRefrigerator,
                        washingMachine = feature.HWashingMachine,
                        bed = feature.HBed,
                        waterHeater = feature.HWaterHeater,
                        gasStove = feature.HGasStove,
                        cableTv = feature.HCableTv,
                        waterDispenser = feature.HWaterDispenser,
                        parking = feature.HParking,
                        socialHousing = feature.HSocialHousing,
                        shortTermRent = feature.HShortTermRent,
                        publicElectricity = feature.HPublicElectricity,
                        publicWatercharges = feature.HPublicWatercharges,
                        landlordShared = feature.HLandlordShared,
                        balcony = feature.HBalcony,
                        publicEquipment = feature.HPublicEquipment
                    } : null,
                    images = images.Select(i => new
                    {
                        url = i.HImageUrl,
                        caption = i.HCaption
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得物件詳細資料時發生錯誤");
                return BadRequest(new { success = false, message = "取得物件詳細資料失敗：" + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProperty([FromForm] string property, [FromForm] string propertyFeature, [FromForm] string ad, List<IFormFile> images)
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            try
            {
                var tenant = GetCurrentTenant();
                var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized(new { success = false, message = "找不到房東資料" });

                // 解析 JSON 資料
                var propertyData = JsonConvert.DeserializeObject<HProperty>(property);
                var featureData = JsonConvert.DeserializeObject<HPropertyFeature>(propertyFeature);

                // 設定房東 ID
                propertyData.HLandlordId = landlord.HLandlordId;
                featureData.HLandlordId = landlord.HLandlordId;

                // 設定時間戳記
                var now = DateTime.Now;
                propertyData.HPublishedDate = now;
                propertyData.HLastUpdated = now;

                // 儲存物件資料
                _db.HProperties.Add(propertyData);
                await _db.SaveChangesAsync();

                // 設定關聯 ID  
                featureData.HPropertyId = propertyData.HPropertyId;

                // 儲存特徵資料
                _db.HPropertyFeatures.Add(featureData);

                // 只有 ad 有資料時才建立 HAd
                if (!string.IsNullOrWhiteSpace(ad) && ad != "{}")
                {
                    var adData = JsonConvert.DeserializeObject<HAd>(ad);
                    adData.HLandlordId = landlord.HLandlordId;
                    adData.HPropertyId = propertyData.HPropertyId;
                    adData.HCreatedDate = now;
                    adData.HLastUpdated = now;
                    _db.HAds.Add(adData);
                }

                // 處理圖片上傳
                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        var imagePath = await SaveImage(image, "Property");
                        var propertyImage = new HPropertyImage
                        {
                            HPropertyId = propertyData.HPropertyId,
                            HLandlordId = landlord.HLandlordId,
                            HImageUrl = imagePath,
                            HUploadedDate = now,
                            HLastUpDated = now,
                            HIsDelete = false
                        };
                        _db.HPropertyImages.Add(propertyImage);
                    }
                }

                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "物件建立成功", propertyId = propertyData.HPropertyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立物件時發生錯誤");
                return BadRequest(new { success = false, message = "建立物件失敗：" + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditProperty(int id, [FromForm] string propertyJson, [FromForm] string propertyFeatureJson, List<IFormFile> images)
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            try
            {
                var tenant = GetCurrentTenant();
                var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized(new { success = false, message = "找不到房東資料" });

                // 解析 JSON 資料
                var propertyData = JsonConvert.DeserializeObject<HProperty>(propertyJson);
                var featureData = JsonConvert.DeserializeObject<HPropertyFeature>(propertyFeatureJson);

                var existingProperty = await _db.HProperties
                      .Include(p => p.HPropertyFeatures)
                      .Include(p => p.HPropertyImages)
                     .FirstOrDefaultAsync(p => p.HPropertyId == id && p.HIsDelete == false);

                if (existingProperty == null)
                    return NotFound(new { success = false, message = "找不到物件" });

                if (existingProperty.HLandlordId != landlord.HLandlordId)
                    return Unauthorized(new { success = false, message = "您沒有權限編輯此物件" });

                // 在 EditProperty 方法中
                List<string>? deletedImageUrls = null;
                if (Request.Form.ContainsKey("deletedImageUrls"))
                {
                    var deletedImageUrlsRaw = Request.Form["deletedImageUrls"];
                    if (!string.IsNullOrEmpty(deletedImageUrlsRaw))
                    {
                        deletedImageUrls = JsonConvert.DeserializeObject<List<string>>(deletedImageUrlsRaw);
                    }
                }

                if (deletedImageUrls != null && deletedImageUrls.Any())
                {
                    foreach (var img in existingProperty.HPropertyImages)
                    {
                        var dbPath = img.HImageUrl?.Replace("\\", "/").TrimStart('/');
                        var deletePath = deletedImageUrls.Select(u => u.Replace("\\", "/").TrimStart('/')).ToList();

                        if (deletePath.Contains(dbPath))
                        {
                            img.HIsDelete = true;
                        }
                    }
                }

                // 更新物件資料
                existingProperty.HLandlordId = landlord.HLandlordId; // 直接用查到的
                existingProperty.HPropertyTitle = propertyData.HPropertyTitle;
                existingProperty.HDescription = propertyData.HDescription;
                existingProperty.HAddress = propertyData.HAddress;
                existingProperty.HCity = propertyData.HCity;
                existingProperty.HDistrict = propertyData.HDistrict;
                existingProperty.HZipcode = propertyData.HZipcode;
                existingProperty.HRentPrice = propertyData.HRentPrice;
                existingProperty.HPropertyType = propertyData.HPropertyType;
                existingProperty.HRoomCount = propertyData.HRoomCount;
                existingProperty.HBathroomCount = propertyData.HBathroomCount;
                existingProperty.HArea = propertyData.HArea;
                existingProperty.HFloor = propertyData.HFloor;
                existingProperty.HTotalFloors = propertyData.HTotalFloors;
                existingProperty.HAvailabilityStatus = propertyData.HAvailabilityStatus;
                existingProperty.HBuildingType = propertyData.HBuildingType;
                existingProperty.HIsVip = propertyData.HIsVip;
                existingProperty.HIsShared = propertyData.HIsShared;
                existingProperty.HStatus = string.IsNullOrEmpty(propertyData.HStatus) ? "已驗證" : propertyData.HStatus;
                existingProperty.HLongitude = propertyData.HLongitude?.ToString();
                existingProperty.HLatitude = propertyData.HLatitude?.ToString();
                existingProperty.HLastUpdated = DateTime.Now;

                // 更新特色資料
                var feature = existingProperty.HPropertyFeatures.FirstOrDefault(f => f.HIsDelete == false);
                if (feature == null)
                {
                    feature = new HPropertyFeature
                    {
                        HPropertyId = existingProperty.HPropertyId,
                        HLandlordId = landlord.HLandlordId,
                        HIsDelete = false
                    };
                    _db.HPropertyFeatures.Add(feature);
                }

                // 更新特色屬性
                feature.HLandlordId = landlord.HLandlordId; // 直接用查到的，不要用 featureData.HLandlordId
                feature.HPropertyId = existingProperty.HPropertyId; // 直接用查到的
                feature.HAllowsDogs = featureData.HAllowsDogs;
                feature.HAllowsCats = featureData.HAllowsCats;
                feature.HAllowsAnimals = featureData.HAllowsAnimals;
                feature.HAllowsCooking = featureData.HAllowsCooking;
                feature.HHasFurniture = featureData.HHasFurniture;
                feature.HInternet = featureData.HInternet;
                feature.HAirConditioning = featureData.HAirConditioning;
                feature.HSharedRental = featureData.HSharedRental;
                feature.HTv = featureData.HTv;
                feature.HRefrigerator = featureData.HRefrigerator;
                feature.HWashingMachine = featureData.HWashingMachine;
                feature.HBed = featureData.HBed;
                feature.HWaterHeater = featureData.HWaterHeater;
                feature.HGasStove = featureData.HGasStove;
                feature.HCableTv = featureData.HCableTv;
                feature.HWaterDispenser = featureData.HWaterDispenser;
                feature.HParking = featureData.HParking;
                feature.HSocialHousing = featureData.HSocialHousing;
                feature.HShortTermRent = featureData.HShortTermRent;
                feature.HPublicElectricity = featureData.HPublicElectricity;
                feature.HPublicWatercharges = featureData.HPublicWatercharges;
                feature.HLandlordShared = featureData.HLandlordShared;
                feature.HBalcony = featureData.HBalcony;
                feature.HPublicEquipment = featureData.HPublicEquipment;

                // 處理圖片上傳（新增）
                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        var imagePath = await SaveImage(image, "Property");
                        var propertyImage = new HPropertyImage
                        {
                            HPropertyId = existingProperty.HPropertyId,
                            HLandlordId = landlord.HLandlordId,
                            HImageUrl = imagePath,
                            HUploadedDate = DateTime.Now,
                            HLastUpDated = DateTime.Now,
                            HIsDelete = false
                        };
                        _db.HPropertyImages.Add(propertyImage);
                    }
                }

                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "物件更新成功" });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                    errorMsg += " | Inner: " + ex.InnerException.Message;
                _logger.LogError(ex, "更新物件時發生錯誤");
                return BadRequest(new { success = false, message = "更新物件失敗：" + errorMsg });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProperty(int id)
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            try
            {
                var tenant = GetCurrentTenant();
                var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized(new { success = false, message = "找不到房東資料" });

                var property = _db.HProperties
                    .FirstOrDefault(p => p.HPropertyId == id && p.HIsDelete == false);

                if (property == null)
                    return NotFound(new { success = false, message = "找不到物件" });

                if (property.HLandlordId != landlord.HLandlordId)
                    return Unauthorized(new { success = false, message = "您沒有權限刪除此物件" });

                // 軟刪除物件
                property.HIsDelete = true;
                _db.SaveChanges();

                return Ok(new { success = true, message = "物件刪除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除物件時發生錯誤");
                return BadRequest(new { success = false, message = "刪除物件失敗：" + ex.Message });
            }
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateProperty(int id)
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            try
            {
                var tenant = GetCurrentTenant();
                var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized(new { success = false, message = "找不到房東資料" });

                var property = await _db.HProperties
                    .FirstOrDefaultAsync(p => p.HPropertyId == id && p.HIsDelete == false);

                if (property == null)
                    return NotFound(new { success = false, message = "找不到物件" });

                if (property.HLandlordId != landlord.HLandlordId)
                    return Unauthorized(new { success = false, message = "您沒有權限操作此物件" });

                // 更新物件狀態為未驗證
                property.HStatus = "未驗證";
                property.HLastUpdated = DateTime.Now;

                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "物件下架成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下架物件時發生錯誤");
                return BadRequest(new { success = false, message = "下架物件失敗：" + ex.Message });
            }
        }

        [HttpPut("{id}/draft")]
        public async Task<IActionResult> SetPropertyToDraft(int id)
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            try
            {
                var tenant = GetCurrentTenant();
                var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized(new { success = false, message = "找不到房東資料" });

                var property = await _db.HProperties
                    .FirstOrDefaultAsync(p => p.HPropertyId == id && p.HIsDelete == false);

                if (property == null)
                    return NotFound(new { success = false, message = "找不到物件" });

                if (property.HLandlordId != landlord.HLandlordId)
                    return Unauthorized(new { success = false, message = "您沒有權限操作此物件" });

                // 更新物件狀態為草稿
                property.HStatus = "草稿";
                property.HLastUpdated = DateTime.Now;

                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "物件已設為草稿" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設為草稿時發生錯誤");
                return BadRequest(new { success = false, message = "設為草稿失敗：" + ex.Message });
            }
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateProperty(int id, [FromBody] ActivatePropertyDto dto)
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            try
            {
                var tenant = GetCurrentTenant();
                var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized(new { success = false, message = "找不到房東資料" });

                var property = await _db.HProperties
                    .FirstOrDefaultAsync(p => p.HPropertyId == id && p.HIsDelete == false);

                if (property == null)
                    return NotFound(new { success = false, message = "找不到物件" });

                if (property.HLandlordId != landlord.HLandlordId)
                    return Unauthorized(new { success = false, message = "您沒有權限操作此物件" });

                // 建立 HAd（廣告）資料
                //var ad = new HAd
                //{
                //    HLandlordId = landlord.HLandlordId,
                //    HPropertyId = id,
                //    HAdName = dto.HAdName,
                //    HCategory = dto.HCategory,
                //    HPlanId = dto.HPlanId,
                //    HCreatedDate = DateTime.Now,
                //    HStatus = "待付款" // 待付款
                //};
                //_db.HAds.Add(ad);
                await _db.SaveChangesAsync();

                // 更新物件狀態為已驗證和未出租
                property.HStatus = "已驗證";
                property.HAvailabilityStatus = "未出租";
                property.HLastUpdated = DateTime.Now;

                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "物件上架成功" });
                //, adId = ad.HAdId 串到AD資料表 加在物件上架成功後面
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上架物件時發生錯誤");
                return BadRequest(new { success = false, message = "上架物件失敗：" + ex.Message });
            }
        }

        private async Task<string> SaveImage(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return "";

            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", folder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{folder}/{fileName}";
        }
    }
}