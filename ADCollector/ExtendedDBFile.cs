using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ADCollector
{
    public class ExtendedDBFiles : ObservableCollection<ExtendedDBFile> { }
    public class ExtendedDBFile
    {
        public string FileName { get; set; }
        public string Type { get; set; }
        public string FileGroup { get; set; }
        public string FileLocation { get; set; }
        public string Size_MB { get; set; }
        public string Used_MB { get; set; }
        public string Free_MB { get; set; }
        public string Free_Percent { get; set; }
        public string AutoGrow { get; set; }

        public ExtendedDBFile(string type, string name, string group, string location, string size, string used,string free, string percent, string growth)
        {
            Type = type;
            FileName = name;
            FileGroup = group;
            FileLocation = location;
            Size_MB = size;
            Used_MB = used;
            Free_MB = free;
            Free_Percent = percent;
            AutoGrow = growth;
        }
        public ExtendedDBFile() { }
    }
}
