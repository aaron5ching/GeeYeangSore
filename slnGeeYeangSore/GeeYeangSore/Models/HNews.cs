using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HNews
{
    public int HNewsId { get; set; }

    public string HTitle { get; set; } = null!;

    public string HContent { get; set; } = null!;

    public string? HImagePath { get; set; }

    public DateTime HCreatedAt { get; set; }

    public DateTime HUpdatedAt { get; set; }
}
