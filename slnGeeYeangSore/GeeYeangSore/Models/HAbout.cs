using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HAbout
{
    public int HAboutId { get; set; }

    public string HTitle { get; set; } = null!;

    public string HContent { get; set; } = null!;

    public DateTime HCreatedAt { get; set; }

    public DateTime HUpdatedAt { get; set; }
}
