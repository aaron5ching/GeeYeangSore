namespace GeeYeangSore.Areas.Admin.ViewModels.UserManagement
{
    public class CUserViewModels
    {
        public int TenantId { get; set; }

        // ✅ 顯示房客狀態（例如：啟用、停用、黑名單）
        public string TenantStatus { get; set; }

        public string LandlordId { get; set; }

        // ✅ 顯示房東狀態（例如：啟用、停用、黑名單）
        public string LandlordStatus { get; set; }

        public string Name { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool IsTenant { get; set; }
        public bool IsLandlord { get; set; }
        public double? HRating { get; set; } // 房東評價（用來顯示星星）

    }
}
