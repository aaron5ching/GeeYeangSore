using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HContact
{
    public int HContactId { get; set; }

    public int HTenantId { get; set; }

    public int HAdminId { get; set; }

    public string HPhoneNumber { get; set; } = null!;

    public string HEmail { get; set; } = null!;

    public bool HStatus { get; set; }

    public string HTitle { get; set; } = null!;

    public string HContent { get; set; } = null!;

    public DateTime HCreatedAt { get; set; }

    public DateTime HReplyAt { get; set; }

    public string? HReplyContent { get; set; }

    public virtual HAdmin HAdmin { get; set; } = null!;
}
