using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HChat
{
    public int HChatId { get; set; }

    public string? HChatType { get; set; }

    public int? HPropertyId { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public int? HAuthorId { get; set; }

    public string? HAuthorType { get; set; }

    public string? HRole { get; set; }

    public DateTime? HJoinedAt { get; set; }
}
