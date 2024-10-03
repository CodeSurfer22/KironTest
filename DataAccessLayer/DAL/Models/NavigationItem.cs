using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class NavigationItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int ParentId { get; set; }
        public List<NavigationItem> Children { get; set; } = new List<NavigationItem>();
    }
}
