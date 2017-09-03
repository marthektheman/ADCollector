using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class Drive
    {
        public string Name { get; set; }
        public double Size { get; set; }
        public double Free { get; set; }
        public string DriveType { get; set; }
        public string Label { get; set; }
        public string FileSystem { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }

        public Drive(string name, double size, double free, string drivetype, string label, string filesystem, string serialnumber)
        {
            Name = name;
            Size = size;
            Free = free;
            DriveType = drivetype;
            Label = label;
            FileSystem = filesystem;
            SerialNumber = serialnumber;            
        }        
        public Drive() { }
    }
}
