using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HTransaction
{
    public int HPaymentId { get; set; }

    public string? HMerchantTradeNo { get; set; }

    public string? HTradeNo { get; set; }

    public decimal? HAmount { get; set; }

    public string? HItemName { get; set; }

    public string? HPaymentType { get; set; }

    public DateTime? HPaymentDate { get; set; }

    public string? HTradeStatus { get; set; }

    public string? HRtnMsg { get; set; }

    public int? HIsSimulated { get; set; }

    public DateTime? HCreateTime { get; set; }

    public DateTime? HUpdateTime { get; set; }

    public string? HRawJson { get; set; }

    public int? HPropertyId { get; set; }

    public string? HRegion { get; set; }

    public int? HAdId { get; set; }

    public virtual HAd? HAd { get; set; }

    public virtual HProperty? HProperty { get; set; }
}
