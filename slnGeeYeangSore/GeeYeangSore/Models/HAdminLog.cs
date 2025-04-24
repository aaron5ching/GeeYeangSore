using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HAdminLog
{
    public int HAdminLogId { get; set; }

    public int HAdminId { get; set; }

    public string HOperationType { get; set; } = null!;

    public string HDescription { get; set; } = null!;

    public DateTime HCreatedAt { get; set; }

    public virtual HAdmin HAdmin { get; set; } = null!;
}
