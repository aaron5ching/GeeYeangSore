using System;
using System.Collections.Generic;

namespace GeeYeang.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public DateTime? OrderDate { get; set; }

    public int? CustomerId { get; set; }
}
