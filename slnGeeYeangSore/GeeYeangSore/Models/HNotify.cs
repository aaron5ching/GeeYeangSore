using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HNotify
{
    public int HNotifyId { get; set; }

    public int HTenantId { get; set; }

    public string? HTitle { get; set; }

    public string? HContent { get; set; }

    public string? HType { get; set; }

    public bool? HStatus { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public virtual HTenant HTenant { get; set; } = null!;
}
