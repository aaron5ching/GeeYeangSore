using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HReportForum
{
    public int HReportForumId { get; set; }

    public string? HTargetType { get; set; }

    public int? HTargetId { get; set; }

    public int? HReporterId { get; set; }

    public string? HReporterType { get; set; }

    public string? HReason { get; set; }

    public string? HStatus { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public DateTime? HHandledAt { get; set; }

    public int? HAdminId { get; set; }

    public virtual HAdmin? HAdmin { get; set; }
}
