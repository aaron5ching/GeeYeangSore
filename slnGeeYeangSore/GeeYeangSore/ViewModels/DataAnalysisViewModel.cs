namespace GeeYeangSore.ViewModels
{
    public class DataAnalysisViewModel
    {

        // 小卡資訊
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        

        // 圓餅圖: 合租 vs 一般租房
        public int SharedPropertiesCount { get; set; }
        public int NonSharedPropertiesCount { get; set; }

        // 圓餅圖: 房東 vs 房客人數比例
        public int TotalLandlords { get; set; }
        public int TotalTenants { get; set; }

        // 柱狀圖: 各地區物件數量
        public Dictionary<string, int> PropertiesByDistrict { get; set; }

        // 折線圖: 每月新增物件
        public List<MonthlyPropertyData> MonthlyPropertyData { get; set; }

        // 折線圖: 平台廣告收益
        public List<MonthlyRevenueData> MonthlyRevenueData { get; set; }

        // 房源特色統計
        public Dictionary<string, int> FeaturesCount { get; set; }
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
