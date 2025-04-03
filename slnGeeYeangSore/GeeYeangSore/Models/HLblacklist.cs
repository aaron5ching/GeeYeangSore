using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HLblacklist
{
    public int HLblacklistId { get; set; }

    public int HLandlordId { get; set; }

    public string? HEntityType { get; set; }

    public string? HReason { get; set; }

    public DateTime? HAddedDate { get; set; }

    public DateTime? HExpirationDate { get; set; }

    public virtual HLandlord HLandlord { get; set; } = null!;
}
