using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class RegionHoliday
{
    public int RegionHolidayId { get; set; }

    public int HolidayId { get; set; }

    public int RegionId { get; set; }

    public virtual Holiday Holiday { get; set; } = null!;

    public virtual Region Region { get; set; } = null!;
}
