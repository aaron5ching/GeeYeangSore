using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HReply
{
    public int HReplyId { get; set; }

    public int? HPostId { get; set; }

    public int? HAuthorId { get; set; }

    public string? HAuthorType { get; set; }

    public string? HContent { get; set; }

    public string? HStatus { get; set; }

    public DateTime? HCreatedAt { get; set; }
}
