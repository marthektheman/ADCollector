using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class SQLFile
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
        public string Used { get; set; }
        public string Free { get; set; }
        public string Percent { get; set; }
        public string Growth { get; set; }

        public SQLFile(string type, string name, string group, string location, string size, string used, string free, string percent, string growth)
        {
            Type = type;
            Name = name;
            Group = group;
            Location = location;
            Size = size;
            Used = used;
            Free = free;
            Percent = percent;
            Growth = growth;
        }
        public SQLFile() { }
    }
}