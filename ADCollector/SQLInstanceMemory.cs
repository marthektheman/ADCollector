using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{    
    public class SQLInstanceMemory
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double InUse { get; set; }
        
        public SQLInstanceMemory(double min, double max, double inuse)
        {
            Min = min;
            Max = max;
            InUse = inuse;
        }
        public SQLInstanceMemory() { }
    }
}
