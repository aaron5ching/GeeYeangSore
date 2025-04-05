// CUserSearchViewModel.cs
namespace GeeYeangSore.Areas.Admin.ViewModels
{
    public class CUserSearchViewModel
    {
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public bool? IsLandlord { get; set; }
    }
}
