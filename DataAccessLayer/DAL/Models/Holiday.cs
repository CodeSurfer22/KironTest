using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Holiday
{
    public int HolidayId { get; set; }

    public string Title { get; set; } = null!;

    public DateOnly HolidayDate { get; set; }

    public string? Notes { get; set; }

    public bool? Bunting { get; set; }

    public virtual ICollection<RegionHoliday> RegionHolidays { get; set; } = new List<RegionHoliday>();
}
