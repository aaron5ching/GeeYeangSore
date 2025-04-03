using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HTenant
{
    public int HTenantId { get; set; }

    public string? HUserName { get; set; }

    public DateTime? HBirthday { get; set; }

    public bool? HGender { get; set; }

    public string? HAddress { get; set; }

    public string? HPhoneNumber { get; set; }

    public string? HEmail { get; set; }

    public string? HPassword { get; set; }

    public string? HImages { get; set; }

    public string? HToken { get; set; }

    public string? HStatus { get; set; }

    public bool? HIsTenant { get; set; }

    public bool? HIsLandlord { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public DateTime? HUpdateAt { get; set; }

    public virtual HAudit? HAudit { get; set; }

    public virtual ICollection<HFavorite> HFavorites { get; set; } = new List<HFavorite>();

    public virtual ICollection<HLandlord> HLandlords { get; set; } = new List<HLandlord>();

    public virtual ICollection<HMatch> HMatchHMatchEdusers { get; set; } = new List<HMatch>();

    public virtual ICollection<HMatch> HMatchHTenants { get; set; } = new List<HMatch>();

    public virtual ICollection<HMblacklist> HMblacklists { get; set; } = new List<HMblacklist>();

    public virtual ICollection<HNotify> HNotifies { get; set; } = new List<HNotify>();
}
