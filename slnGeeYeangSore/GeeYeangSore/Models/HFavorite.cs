using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HFavorite
{
    public int HFavoriteId { get; set; }

    public int HTenantId { get; set; }

    public int HPropertyId { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public virtual HProperty HProperty { get; set; } = null!;

    public virtual HTenant HTenant { get; set; } = null!;
}
