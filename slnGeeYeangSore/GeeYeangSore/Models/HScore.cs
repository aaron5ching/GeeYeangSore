using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HScore
{
    public int HScoreId { get; set; }

    public int? HInstantMessagId { get; set; }

    public int? HSender { get; set; }

    public string? HSenderType { get; set; }

    public int? HReceiver { get; set; }

    public string? HReceiverType { get; set; }

    public int? HScoreValue { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public virtual HInstantMessag? HInstantMessag { get; set; }
}
