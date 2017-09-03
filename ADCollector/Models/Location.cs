using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq.Mapping;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    [Table(Name = "Location")]
    public class Location
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int LocationID { get; set; }
        [Column]
        public string RegionName { get; set; }
        [Column]
        public string RegionCode { get; set; }
        [Column]
        public string AffiliateName { get; set; }
        [Column]
        public string SiteCode { get; set; }
        [Column]
        public string Address { get; set; }
        [Column]
        public string SiteContact { get; set; }
        [Column]
        public string Notes { get; set; }
        public Location() { }
    }
}