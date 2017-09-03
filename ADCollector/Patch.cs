using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ADCollector
{
    public class Patch
    {
        public string Caption { get; set; }
        public string Description { get; set; }
        public string HotfixID { get; set; }
        public string InstalledOn { get; set; }
        public string InstalledBy { get; set; }
        public Patch(string caption, string description, string hotfixid, string installedon, string installedby)
        {
            Caption = caption;
            Description = description;
            HotfixID = hotfixid;
            InstalledOn = installedon;
            InstalledBy = installedby;
        }
        public Patch() { }
    }
}
