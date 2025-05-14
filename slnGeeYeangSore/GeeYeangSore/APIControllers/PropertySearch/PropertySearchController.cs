using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.APIControllers;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.APIControllers.PropertySearch.DTO;

namespace GeeYeangSore.APIControllers.PropertySearch
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertySearchController : BaseController
    {
        public PropertySearchController(GeeYeangSoreContext db) : base(db) { }

        [HttpGet("propertyList")]
        public IActionResult GetPropertiesList()
        {
            var result = _db.HProperties
                        .Include(p => p.HPropertyImages)
                        .Where(p => p.HAvailabilityStatus == "未出租" && p.HStatus == "已驗證" && p.HIsDelete == false)
                        .OrderByDescending(p => p.HPublishedDate)
                        .Select(p => new {
                            propertyId = p.HPropertyId,
                            title = p.HPropertyTitle,
                            rentPrice = p.HRentPrice,
                            city = p.HCity,
                            district = p.HDistrict,
                            address = p.HAddress,
                            roomCount = p.HRoomCount,
                            bathroomCount = p.HBathroomCount,
                            propertyType = p.HPropertyType,
                            image = p.HPropertyImages
                                    .Where(i => i.HIsDelete == false)
                                    .OrderBy(i => i.HUploadedDate)
                                    .Select(i => "https://localhost:7022" + i.HImageUrl)
                                    .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg"
                        })
                        .ToList();                  

            return Ok(result);
        }

        [HttpGet("featuredProperties")]
        public IActionResult GetFeaturedProperties()
        {
            var data = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Where(p =>
                    p.HAvailabilityStatus == "未出租" &&
                    p.HStatus == "已驗證" &&
                    _db.HAds.Any(ad =>
                        ad.HPropertyId == p.HPropertyId &&
                        ad.HCategory == "VIP3" &&
                        ad.HStatus == "進行中"
                    )
                )
                .OrderByDescending(p => Guid.NewGuid()) // 隨機
                .Take(6)
                .Select(p => new {
                    propertyId = p.HPropertyId,
                    title = p.HPropertyTitle,
                    rentPrice = p.HRentPrice,
                    city = p.HCity,
                    district = p.HDistrict,
                    address = p.HAddress,
                    roomCount = p.HRoomCount,
                    bathroomCount = p.HBathroomCount,
                    propertyType = p.HPropertyType,
                    image = p.HPropertyImages
                                .Where(i => i.HIsDelete == false)
                                .OrderBy(i => i.HUploadedDate)
                                .Select(i => "https://localhost:7022" + i.HImageUrl)
                                .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg"
                })
                .ToList();

            return Ok(data);
        }

        [HttpGet("landlordProperties")]
        public IActionResult GetLandlordProperties()
        {
            var data = (
                from p in _db.HProperties
                join l in _db.HLandlords on p.HLandlordId equals l.HLandlordId
                join t in _db.HTenants on l.HTenantId equals t.HTenantId
                join ad in _db.HAds on p.HPropertyId equals ad.HPropertyId
                where
                    p.HAvailabilityStatus == "未出租" &&
                    p.HStatus == "已驗證" &&
                    l.HStatus == "已驗證" &&
                    (ad.HCategory == "VIP2" || ad.HCategory == "VIP3") &&
                    ad.HStatus == "進行中"
                select new
                {
                    landlord = new
                    {
                        id = l.HLandlordId,
                        name = t.HUserName,
                        phone = t.HPhoneNumber,
                        avatar = string.IsNullOrEmpty(t.HImages)
                            ? "https://localhost:7022/images/User/default.png"
                            : "https://localhost:7022/images/User/" + t.HImages
                    },
                    property = new
                    {
                        propertyId = p.HPropertyId,
                        title = p.HPropertyTitle,
                        rentPrice = p.HRentPrice,
                        city = p.HCity,
                        district = p.HDistrict,
                        address = p.HAddress,
                        propertyType = p.HPropertyType,
                        roomCount = p.HRoomCount,
                        bathroomCount = p.HBathroomCount,
                        image = p.HPropertyImages
                                    .Where(i => i.HIsDelete == false)
                                    .OrderBy(i => i.HUploadedDate)
                                    .Select(i => "https://localhost:7022" + i.HImageUrl)
                                    .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg"
                    }
                })
                .OrderByDescending(x => Guid.NewGuid()) // 隨機
                .Take(6)
                .ToList();

            return Ok(data);
        }

        [HttpPost("filter")]
        public IActionResult FilterProperties([FromBody] PropertyFilterDTO filter)
        {
            var query = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Include(p => p.HPropertyFeatures)
                .Where(p => p.HAvailabilityStatus == "未出租" && p.HStatus == "已驗證");

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(p => p.HPropertyTitle.Contains(filter.Keyword) || p.HAddress.Contains(filter.Keyword));

            if (!string.IsNullOrWhiteSpace(filter.City))
                query = query.Where(p => p.HCity == filter.City);

            if (!string.IsNullOrWhiteSpace(filter.District))
                query = query.Where(p => p.HDistrict == filter.District);

            if (!string.IsNullOrWhiteSpace(filter.Type) && filter.Type != "不限")
                query = query.Where(p => p.HPropertyType == filter.Type);

            if (!string.IsNullOrWhiteSpace(filter.BuildingType) && filter.BuildingType != "不限")
                query = query.Where(p => p.HBuildingType == filter.BuildingType);

            if (filter.RentMin.HasValue)
                query = query.Where(p => p.HRentPrice >= filter.RentMin.Value);

            if (filter.RentMax.HasValue)
                query = query.Where(p => p.HRentPrice <= filter.RentMax.Value);

            if (filter.AreaMin.HasValue)
                query = query.Where(p => p.HArea >= filter.AreaMin.Value);

            if (filter.AreaMax.HasValue)
                query = query.Where(p => p.HArea <= filter.AreaMax.Value);

            // 執行查詢並載入記憶體
            var allData = query.ToList();

            // 如果有特色篩選，再在記憶體中處理
            if (filter.Features != null && filter.Features.Any())
            {
                allData = allData.Where(p =>
                {
                    var f = p.HPropertyFeatures.FirstOrDefault(); 

                    return filter.Features.All(feature =>
                        feature switch
                        {
                            "可養狗" => f?.HAllowsDogs == true,
                            "可養貓" => f?.HAllowsCats == true,
                            "可養其他寵物" => f?.HAllowsAnimals == true,
                            "可開伙" => f?.HAllowsCooking == true,
                            "有家具" => f?.HHasFurniture == true,
                            "有床" => f?.HBed == true,
                            "有陽台" => f?.HBalcony == true,
                            "公設" => f?.HPublicEquipment == true,
                            "車位" => f?.HParking == true,
                            "有冷氣" => f?.HAirConditioning == true,
                            "有電視" => f?.HTv == true,
                            "有冰箱" => f?.HRefrigerator == true,
                            "有洗衣機" => f?.HWashingMachine == true,
                            "有飲水機" => f?.HWaterDispenser == true,
                            "有熱水器" => f?.HWaterHeater == true,
                            "有天然瓦斯" => f?.HGasStove == true,
                            "有網路" => f?.HInternet == true,
                            "有第四台" => f?.HCableTv == true,
                            "公家電費" => f?.HPublicElectricity == true,
                            "公家水費" => f?.HPublicWatercharges == true,
                            "短期租賃" => f?.HShortTermRent == true,
                            "社會住宅" => f?.HSocialHousing == true,
                            "房東同住" => f?.HLandlordShared == true,
                            _ => true
                        });
                }).ToList();
            }

            
            var result = allData.Select(p => new
            {
                propertyId = p.HPropertyId,
                title = p.HPropertyTitle,
                rentPrice = p.HRentPrice,
                city = p.HCity,
                district = p.HDistrict,
                address = p.HAddress,
                propertyType = p.HPropertyType,
                roomCount = p.HRoomCount,
                bathroomCount = p.HBathroomCount,
                image = p.HPropertyImages
                    .Where(i => i.HIsDelete == false)
                    .OrderBy(i => i.HUploadedDate)
                    .Select(i => "https://localhost:7022" + i.HImageUrl)
                    .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg"
            }).ToList();

            return Ok(result);
        }
    }
}
