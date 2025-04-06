using GeeYeangSore.Controllers;
using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.ViewModels;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    // 數據分析控制器
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")]
    public class DataAnalysisController : SuperController
    {
       
        private readonly GeeYeangSoreContext _context;

        public DataAnalysisController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard(string selectedMonth)
        {
  
                var dataAnalysis = new DataAnalysisViewModel
                {
                    TotalUsers = _context.HLandlords.Count() + _context.HTenants.Count(),
                    TotalRevenue = _context.HRevenueReports.Sum(r => r.HTotalIncome ?? 0),

                    // 合租 vs 一般租房
                    SharedPropertiesCount = _context.HProperties.Count(p => p.HIsShared == true),
                    NonSharedPropertiesCount = _context.HProperties.Count(p => !p.HIsShared == true),

                    // 房東 vs 房客人數比例
                    TotalLandlords = _context.HLandlords.Count(),
                    TotalTenants = _context.HTenants.Count(),

                    // 各地區物件數量
                    PropertiesByDistrict = _context.HProperties
                                            .Where(p => p.HDistrict != null)
                                            .GroupBy(p => p.HDistrict ?? "Unknown")
                                            .ToDictionary(g => g.Key, g => g.Count()),

                    // 月新增物件
                    MonthlyPropertyData = _context.HProperties
                                          .Where(p => p.HPublishedDate.HasValue)
                                          .GroupBy(p => new { p.HPublishedDate.Value.Year, p.HPublishedDate.Value.Month })
                                          .ToList() 
                                          .Where(g => $"{g.Key.Year}-{g.Key.Month}" == selectedMonth) 
                                          .Select(g => new MonthlyPropertyData
                                          {
                                              Month = $"{g.Key.Year}-{g.Key.Month}",
                                              PropertyCount = g.Count()
                                          })
                                          .OrderBy(m => m.Month)
                                          .ToList(),

                    // 平台廣告收益
                    MonthlyRevenueData = _context.HRevenueReports
                                         .Where(r => r.HReportDate.HasValue)
                                         .GroupBy(r => new { r.HReportDate.Value.Year, r.HReportDate.Value.Month })
                                         .ToList() 
                                         .Where(g => $"{g.Key.Year}-{g.Key.Month}" == selectedMonth) // 篩選所需的月份
                                         .Select(g => new MonthlyRevenueData
                                         {
                                             Month = $"{g.Key.Year}-{g.Key.Month}",
                                             Revenue = g.Sum(r => r.HTotalIncome ?? 0)
                                         })
                                         .OrderBy(m => m.Month)
                                         .ToList(),

                    // 房源特色統計
                    FeaturesCount = new Dictionary<string, int>
                    {
                        { "AllowsDogs", _context.HPropertyFeatures.Count(f => f.HAllowsDogs == true) },
                        { "AllowsCats", _context.HPropertyFeatures.Count(f => f.HAllowsCats == true) },
                        { "AllowsAnimals", _context.HPropertyFeatures.Count(f => f.HAllowsAnimals == true) },
                        { "AllowsCooking", _context.HPropertyFeatures.Count(f => f.HAllowsCooking == true) },
                        { "HasFurniture", _context.HPropertyFeatures.Count(f => f.HHasFurniture == true) },
                        { "Internet", _context.HPropertyFeatures.Count(f => f.HInternet == true) },
                        { "AirConditioning", _context.HPropertyFeatures.Count(f => f.HAirConditioning == true) },
                        { "SharedRental", _context.HPropertyFeatures.Count(f => f.HSharedRental == true) },
                        { "Tv", _context.HPropertyFeatures.Count(f => f.HTv == true) },
                        { "Refrigerator", _context.HPropertyFeatures.Count(f => f.HRefrigerator == true) },
                        { "WashingMachine", _context.HPropertyFeatures.Count(f => f.HWashingMachine == true) },
                        { "Bed", _context.HPropertyFeatures.Count(f => f.HBed == true) },
                        { "WaterHeater", _context.HPropertyFeatures.Count(f => f.HWaterHeater == true) },
                        { "GasStove", _context.HPropertyFeatures.Count(f => f.HGasStove == true) },
                        { "CableTv", _context.HPropertyFeatures.Count(f => f.HCableTv == true) },
                        { "WaterDispenser", _context.HPropertyFeatures.Count(f => f.HWaterDispenser == true) },
                        { "Parking", _context.HPropertyFeatures.Count(f => f.HParking == true) },
                        { "SocialHousing", _context.HPropertyFeatures.Count(f => f.HSocialHousing == true) },
                        { "ShortTermRent", _context.HPropertyFeatures.Count(f => f.HShortTermRent == true) },
                        { "PublicElectricity", _context.HPropertyFeatures.Count(f => f.HPublicElectricity == true) },
                        { "PublicWatercharges", _context.HPropertyFeatures.Count(f => f.HPublicWatercharges == true) },
                        { "LandlordShared", _context.HPropertyFeatures.Count(f => f.HLandlordShared == true) },
                        { "Balcony", _context.HPropertyFeatures.Count(f => f.HBalcony == true) },
                        { "PublicEquipment", _context.HPropertyFeatures.Count(f => f.HPublicEquipment == true) },
                    }
                };
            return View(dataAnalysis);
        }

        public IActionResult DataTable()
        {
            return View();
        }
    }
}
