using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HＭblacklist
{
    public int HＭblacklistId { get; set; }

    public int HTenantId { get; set; }

    public string? HEntityType { get; set; }

    public string? HReason { get; set; }

    public DateTime? HAddedDate { get; set; }

    public DateTime? HExpirationDate { get; set; }

    public virtual HTenant HTenant { get; set; } = null!;
}
