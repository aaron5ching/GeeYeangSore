using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HPasswordReset
{
    public int HPasswordResetId { get; set; }

    public int HTenantId { get; set; }

    public string HResetToken { get; set; } = null!;

    public DateTime HResetExpiresAt { get; set; }

    public bool HIsUsed { get; set; }

    public DateTime HCreatedAt { get; set; }

    public DateTime? HUsedAt { get; set; }

    public string? HRequestIp { get; set; }

    public virtual HTenant HTenant { get; set; } = null!;
}
