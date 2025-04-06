using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HMatch
{
    public int HMatchId { get; set; }

    public int? HTenantId { get; set; }

    public int? HMatchEduserId { get; set; }

    public string? HMatchUserIntro { get; set; }

    public string? HMatchReason { get; set; }

    public DateTime? HMatchdate { get; set; }

    public DateTime? HLastUpdated { get; set; }

    public double? HCompatibilityScore { get; set; }

    public string? HStatus { get; set; }

    public string? HPreferredCity { get; set; }

    public string? HPreferreDistrict { get; set; }

    public int? HBudget { get; set; }

    public string? HSleepschedule { get; set; }

    public int? HAcceptsSmoking { get; set; }

    public int? HAcceptsPets { get; set; }

    public string? HPreferredRoommateGender { get; set; }

    public string? HPreferredRoommateAge { get; set; }

    public string? HPreferredRoommateOccupation { get; set; }

    public virtual HTenant? HMatchEduser { get; set; }

    public virtual HTenant? HTenant { get; set; }
}
