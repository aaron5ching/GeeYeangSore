using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HPropertyImage
{
    public int HImageId { get; set; }

    public int? HPropertyId { get; set; }

    public int? HLandlordId { get; set; }

    public string? HImageUrl { get; set; }

    public string? HCaption { get; set; }

    public DateTime? HUploadedDate { get; set; }

    public DateTime? HLastUpDated { get; set; }
}
