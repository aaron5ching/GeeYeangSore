namespace GeeYeangSore.Areas.Admin.ViewModels
{
    public class DataAnalysisViewModel
    {

        // 小卡資訊
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }


        // 圓餅圖: VIP廣告分布
        public Dictionary<string, int> VipCategoryDistribution { get; set; } = new();

        // 圓餅圖: 房東 vs 房客人數比例
        public int TotalLandlords { get; set; }
        public int TotalTenants { get; set; }

        // 柱狀圖: 各地區物件數量
        public Dictionary<string, int> PropertiesByCity { get; set; }

        // 折線圖: 每月新增物件
        public List<MonthlyPropertyData> MonthlyPropertyData { get; set; }

        // 折線圖: 平台廣告收益
        public List<MonthlyRevenueData> MonthlyRevenueData { get; set; }

        // 橫條圖: 房源類型比例
        public Dictionary<string, int> PropertyTypeCounts { get; set; }

        // 房源特色統計
        public Dictionary<string, int> FeaturesCount { get; set; }

        // 使用者選擇的年份
        public int SelectedYear { get; set; }

        // 可供選擇的所有年份列表
        public List<int> AvailableYears { get; set; }

        public List<int> PropertyAvailableYears { get; set; } = new();
        public List<int> RevenueAvailableYears { get; set; } = new();

        // 當月數據
        public int CurrentMonthProperties { get; set; }
        public int CurrentMonthVipAds { get; set; }
        public int CurrentMonthUsers { get; set; }
        public decimal CurrentMonthRevenue { get; set; }

        
    }

    public class MonthlyPropertyData
    {
        public string Month { get; set; }
        public int PropertyCount { get; set; }
    }

    public class MonthlyRevenueData
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
    }

    


}
