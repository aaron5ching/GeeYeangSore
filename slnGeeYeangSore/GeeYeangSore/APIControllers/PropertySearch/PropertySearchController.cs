using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.APIControllers;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.DTO.PropertySearch;

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
                .Where(p => p.HAvailabilityStatus == "未出租" && p.HStatus == "已驗證" && p.HIsDelete == false);

            // 關鍵字
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(p =>
                    p.HPropertyTitle.Contains(filter.Keyword) ||
                    p.HCity.Contains(filter.Keyword) ||
                    p.HDistrict.Contains(filter.Keyword) ||
                    p.HAddress.Contains(filter.Keyword)) ;
            }

            // 城市 / 區域
            if (!string.IsNullOrWhiteSpace(filter.City) && filter.City != "不限")
            {
                query = query.Where(p => p.HCity == filter.City);
            }

            if (!string.IsNullOrWhiteSpace(filter.District) && filter.District != "不限")
            {
                query = query.Where(p => p.HDistrict == filter.District);
            }

            // 類型與型態
            if (!string.IsNullOrWhiteSpace(filter.Type) && filter.Type != "不限")
            {
                query = query.Where(p => p.HPropertyType == filter.Type);
            }

            if (!string.IsNullOrWhiteSpace(filter.BuildingType) && filter.BuildingType != "不限")
            {
                query = query.Where(p => p.HBuildingType == filter.BuildingType);
            }

            // 租金
            if (filter.RentMin.HasValue)
                query = query.Where(p => p.HRentPrice >= filter.RentMin.Value);

            if (filter.RentMax.HasValue)
                query = query.Where(p => p.HRentPrice <= filter.RentMax.Value);

            // 坪數
            if (filter.AreaMin.HasValue)
                query = query.Where(p => p.HArea >= filter.AreaMin.Value);

            if (filter.AreaMax.HasValue)
                query = query.Where(p => p.HArea <= filter.AreaMax.Value);

            // 特色篩選
            if (filter.Features != null && filter.Features.Any())
            {
                foreach (var feature in filter.Features)
                {
                    if (feature == "可養狗")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAllowsDogs != null && f.HAllowsDogs == true));
                    else if (feature == "可養貓")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAllowsCats != null && f.HAllowsCats == true));
                    else if (feature == "可養其他寵物")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAllowsAnimals != null && f.HAllowsAnimals == true));
                    else if (feature == "可開伙")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAllowsCooking != null && f.HAllowsCooking == true));
                    else if (feature == "有家具")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HHasFurniture != null && f.HHasFurniture == true));
                    else if (feature == "有床")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HBed != null && f.HBed == true));
                    else if (feature == "有陽台")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HBalcony != null && f.HBalcony == true));
                    else if (feature == "公設")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HPublicEquipment != null && f.HPublicEquipment == true));
                    else if (feature == "車位")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HParking != null && f.HParking == true));
                    else if (feature == "有冷氣")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAirConditioning != null && f.HAirConditioning == true));
                    else if (feature == "有電視")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HTv != null && f.HTv == true));
                    else if (feature == "有冰箱")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HRefrigerator != null && f.HRefrigerator == true));
                    else if (feature == "有洗衣機")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HWashingMachine != null && f.HWashingMachine == true));
                    else if (feature == "有飲水機")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HWaterDispenser != null && f.HWaterDispenser == true));
                    else if (feature == "有熱水器")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HWaterHeater != null && f.HWaterHeater == true));
                    else if (feature == "有天然瓦斯")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HGasStove != null && f.HGasStove == true));
                    else if (feature == "有網路")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HInternet != null && f.HInternet == true));
                    else if (feature == "有第四台")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HCableTv != null && f.HCableTv == true));
                    else if (feature == "公家電費")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HPublicElectricity != null && f.HPublicElectricity == true));
                    else if (feature == "公家水費")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HPublicWatercharges != null && f.HPublicWatercharges == true));
                    else if (feature == "短期租賃")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HShortTermRent != null && f.HShortTermRent == true));
                    else if (feature == "社會住宅")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HSocialHousing != null && f.HSocialHousing == true));
                    else if (feature == "房東同住")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HLandlordShared != null && f.HLandlordShared == true));
                }
            }

            var result = query
                .Select(p => new
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
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetPropertyById(int id)
        {
            var property = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Include(p => p.HPropertyFeatures)
                .Include(p => p.HLandlord)
                    .ThenInclude(l => l.HTenant)
                .FirstOrDefault(p => p.HPropertyId == id && p.HIsDelete == false && p.HAvailabilityStatus == "未出租" && p.HStatus == "已驗證");

            if (property == null)
            {
                return NotFound(new { message = "找不到該房源" });
            }

            var feature = property.HPropertyFeatures.FirstOrDefault(f => f.HIsDelete == false);
            if (feature == null)
            {
                return NotFound(new { message = "該房源無有效的特色資料" });
            }

            var featureMap = new Dictionary<string, bool?>
            {
                { "dog", feature.HAllowsDogs },
                { "cat", feature.HAllowsCats },
                { "other_pet", feature.HAllowsAnimals },
                { "cooking", feature.HAllowsCooking },
                { "furniture", feature.HHasFurniture },
                { "bed", feature.HBed },
                { "balcony", feature.HBalcony },
                { "public_facility", feature.HPublicEquipment },
                { "parking", feature.HParking },
                { "aircon", feature.HAirConditioning },
                { "tv", feature.HTv },
                { "fridge", feature.HRefrigerator },
                { "washer", feature.HWashingMachine },
                { "water_dispenser", feature.HWaterDispenser },
                { "heater", feature.HWaterHeater },
                { "gas", feature.HGasStove },
                { "internet", feature.HInternet },
                { "cable_tv", feature.HCableTv },
                { "public_elec", feature.HPublicElectricity },
                { "public_water", feature.HPublicWatercharges },
                { "short_term", feature.HShortTermRent },
                { "social_housing", feature.HSocialHousing },
                { "landlord_live", feature.HLandlordShared }
            };

            var features = featureMap
                .Where(kv => kv.Value.HasValue && kv.Value.Value)
                .Select(kv => kv.Key)
                .ToList();

            var result = new
            {
                property = new
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
                    publishedDate = property.HPublishedDate,
                    features = features
                },
                images = property.HPropertyImages
                    .Where(i => i.HIsDelete == false)
                    .OrderBy(i => i.HUploadedDate)
                    .Select(i => "https://localhost:7022" + i.HImageUrl)
                    .ToList(),
                landlord = new
                {
                    id = property.HLandlord.HLandlordId,
                    name = property.HLandlord.HTenant.HUserName,
                    phone = property.HLandlord.HTenant.HPhoneNumber,
                    avatar = string.IsNullOrEmpty(property.HLandlord.HTenant.HImages)
                        ? "https://localhost:7022/images/User/default.png"
                        : "https://localhost:7022/images/User/" + property.HLandlord.HTenant.HImages
                }
            };

            return Ok(result);
        }

        [HttpGet("recommendedProperties")]
        public IActionResult GetRecommendedProperties()
        {
            var data = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Where(p =>
                    p.HAvailabilityStatus == "未出租" &&
                    p.HStatus == "已驗證" &&
                    _db.HAds.Any(ad =>
                        ad.HPropertyId == p.HPropertyId &&
                        (ad.HCategory == "VIP1" || ad.HCategory == "VIP2" || ad.HCategory == "VIP3") &&
                        ad.HStatus == "進行中"
                    )
                )
                .OrderByDescending(p => Guid.NewGuid()) // 隨機
                .Take(6)
                .Select(p => new
                {
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
    }
}
