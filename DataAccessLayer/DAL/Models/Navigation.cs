using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Navigation
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int ParentId { get; set; }
}
