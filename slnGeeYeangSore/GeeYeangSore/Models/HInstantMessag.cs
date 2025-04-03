using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HInstantMessag
{
    public int HInstantMessagId { get; set; }

    public int? HSender { get; set; }

    public string? HSenderType { get; set; }

    public int? HReceiver { get; set; }

    public string? HReceiverType { get; set; }

    public string? HMessage { get; set; }

    public bool? HRead { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public virtual ICollection<HScore> HScores { get; set; } = new List<HScore>();
}
