using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HMblacklist
{
    public int HＭblacklistId { get; set; }

    public int HTenantId { get; set; }

    public string HEntityType { get; set; } = null!;

    public string HReason { get; set; } = null!;

    public DateTime HAddedDate { get; set; }

    public DateTime? HExpirationDate { get; set; }

    public virtual HTenant HTenant { get; set; } = null!;
}
