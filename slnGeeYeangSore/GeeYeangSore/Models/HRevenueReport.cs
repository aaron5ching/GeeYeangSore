using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HRevenueReport
{
    public int HReportId { get; set; }

    public DateTime? HReportDate { get; set; }

    public int? HTotalTransactions { get; set; }

    public decimal? HTotalIncome { get; set; }

    public decimal? HDailyIncome { get; set; }

    public decimal? HMonthlyIncome { get; set; }

    public decimal? HGrowthRate { get; set; }

    public string? HPaymentMethods { get; set; }

    public DateTime? HGeneratedAt { get; set; }

    public DateTime? HReportPeriod { get; set; }
}
