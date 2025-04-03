namespace GeeYeangSore.Areas.Admin.ViewModels
{
    public class CUserViewModels
    {

        public int TenantId { get; set; }
        public string LandlordId { get; set; }
        public string Name { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Status { get; set; }
        public bool IsTenant { get; set; }
        public bool IsLandlord { get; set; }


    }
}
