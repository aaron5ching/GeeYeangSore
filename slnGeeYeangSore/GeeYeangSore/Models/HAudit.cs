using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GeeYeangSore.Models;

public partial class HAudit
{
    
    
    [Key]
    public int HAuditId { get; set; }

    public int HTenantId { get; set; }

    public string HIdCardFrontPath { get; set; } = null!;

    public string HIdCardBackPath { get; set; } = null!;

    public string HBankAccount { get; set; } = null!;

    public string HBankName { get; set; } = null!;

    public string HStatus { get; set; } = null!;

    public string? HReviewNote { get; set; }

    public DateTime HSubmittedAt { get; set; }

    public DateTime? HReviewedAt { get; set; }
}
