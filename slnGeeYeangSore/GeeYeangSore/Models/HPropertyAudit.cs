using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HPropertyAudit
{
    public int HAuditId { get; set; }

    public int? HPropertyId { get; set; }

    public int? HLandlordId { get; set; }

    public int? HAuditorId { get; set; }

    public string? HAuditStatus { get; set; }

    public string? HAuditNotes { get; set; }

    public DateTime? HAuditDate { get; set; }

    public bool? HIsDelete { get; set; }

    public virtual HLandlord? HLandlord { get; set; }

    public virtual HProperty? HProperty { get; set; }
}
