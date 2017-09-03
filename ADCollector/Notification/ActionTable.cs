using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq.Mapping;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    [Table(Name = "Actions")]
    public class ActionTable
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int actionID { get; set; }
        [Column]
        public string Username { get; set; }
        [Column]
        public string DisplayName { get; set; }
        [Column]
        public string Telephone { get; set; }
        [Column]
        public string ManagerName { get; set; }
        [Column]
        public string EmailAddress { get; set; }
        [Column]
        public string Department { get; set; }
        [Column]
        public string Activity { get; set; }
        [Column]
        public string Tool { get; set; }
        [Column]
        public DateTime? UpdateTimeStamp { get; set; }
        public ActionTable() { }
    }
}

