namespace GeeYeangSore.Areas.Admin.ViewModels
{
    // 租客需求
    public class TenantDemandTableViewModel
    {
        public List<FeatureDemandItem> FeatureDemands { get; set; } = new();
        public int TotalCount { get; set; }

        public int FavoritePropertyCount { get; set; }  // 被收藏的房源數
        public int TotalFeatureCount { get; set; }

        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new();
    }

    public class FeatureDemandItem
    {
        public string FeatureName { get; set; } = "";
        public int Count { get; set; }
    }

}
