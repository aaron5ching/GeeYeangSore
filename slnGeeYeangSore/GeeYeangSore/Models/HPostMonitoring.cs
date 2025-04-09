using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HPostMonitoring
{
    public int HAbnormalId { get; set; }

    public int? HAuthorId { get; set; }

    public string? HAuthorType { get; set; }

    public int? HPostId { get; set; }

    public DateTime? HDetectedAt { get; set; }

    public string? HStatus { get; set; }

    public int? HAdminId { get; set; }

    public virtual HAdmin? HAdmin { get; set; }

    public virtual HPost? HPost { get; set; }
}
