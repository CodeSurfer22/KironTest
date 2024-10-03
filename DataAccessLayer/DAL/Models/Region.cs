using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Region
{
    public int RegionId { get; set; }

    public string RegionName { get; set; } = null!;

    public virtual ICollection<RegionHoliday> RegionHolidays { get; set; } = new List<RegionHoliday>();
}
