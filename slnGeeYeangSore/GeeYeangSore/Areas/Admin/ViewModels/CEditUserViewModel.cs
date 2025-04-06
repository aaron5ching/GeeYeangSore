namespace GeeYeangSore.Areas.Admin.ViewModels
{
    public class CEditLandlordViewModel
    {
        public string? HLandlordName { get; set; }
        public string? HStatus { get; set; }
        public string? HBankName { get; set; }
        public string? HBankAccount { get; set; }
        public string? HIdCardFrontUrl { get; set; }
        public string? HIdCardBackUrl { get; set; }
    }

    public class CEditUserViewModel
    {
        public int HTenantId { get; set; }
        public string? HUserName { get; set; }
        public DateTime? HBirthday { get; set; }
        public bool? HGender { get; set; }
        public string? HAddress { get; set; }
        public string? HPhoneNumber { get; set; }
        public string? HEmail { get; set; }
        public string? HPassword { get; set; }
        public string? HImages { get; set; }
        public string? HStatus { get; set; }

        public List<CEditLandlordViewModel> HLandlords { get; set; } = new();
    }
}
