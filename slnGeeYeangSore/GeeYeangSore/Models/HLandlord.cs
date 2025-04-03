using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HLandlord
{
    public int HLandlordId { get; set; }

    public int HTenantId { get; set; }

    public string? HLandlordName { get; set; }

    public string? HBankName { get; set; }

    public string? HBankAccount { get; set; }

    public string? HIdCardFrontUrl { get; set; }

    public string? HIdCardBackUrl { get; set; }

    public int? HRating { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public DateTime? HUpdateAt { get; set; }

    public string? HStatus { get; set; }

    public virtual ICollection<HAd> HAds { get; set; } = new List<HAd>();

    public virtual ICollection<HLblacklist> HLblacklists { get; set; } = new List<HLblacklist>();

    public virtual ICollection<HProperty> HProperties { get; set; } = new List<HProperty>();

    public virtual HTenant HTenant { get; set; } = null!;
}
