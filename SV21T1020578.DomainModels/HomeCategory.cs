using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.DomainModels
{
    public class HomeCategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public required string IconClass { get; set; }
        public int ProductCount { get; set; }

    }
}
