using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HReport
{
    public int HReportId { get; set; }

    public int? HMessageId { get; set; }

    public int? HAuthorId { get; set; }

    public string? HAuthorType { get; set; }

    public string? HReason { get; set; }

    public string? HStatus { get; set; }

    public int? HAdminId { get; set; }

    public DateTime? HReviewedAt { get; set; }

    public DateTime? HCreatedAt { get; set; }
}
