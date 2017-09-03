using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public partial class ADInventory : DataContext
    {
        public Table<Location> Locations;
        public ADInventory(string connection) : base(connection) { }
    }
}
