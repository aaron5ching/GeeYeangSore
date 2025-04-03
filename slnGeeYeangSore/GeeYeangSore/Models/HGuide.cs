using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HGuide
{
    public int HGuideId { get; set; }

    public string HTitle { get; set; } = null!;

    public string HContent { get; set; } = null!;

    public string? HImagePath { get; set; }

    public DateTime HCreatedAt { get; set; }

    public DateTime HUpdatedAt { get; set; }
}
