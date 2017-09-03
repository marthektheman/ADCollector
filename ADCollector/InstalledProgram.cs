using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{  
    public class InstalledProgram
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string InstallState { get; set; }
        public string Description { get; set; }
        public string IdentifyingNumber { get; set; }
        public string InstallDate { get; set; }
        public string InstallSource { get; set; }
        public string PackageName { get; set; }
        public string Vendor { get; set; }
        public string Language { get; set; }
        public InstalledProgram(string name, string version, string installstate, string description, string identifyingnumber, string installdate, string installsource, string packagename, string vendor, string language)
        {
            Name = name;
            Version = version;
            InstallState = installstate;
            Description = description;
            IdentifyingNumber = identifyingnumber;
            InstallDate = installdate;
            InstallSource = installsource;
            PackageName = packagename;
            Vendor = vendor;
            Language = language;
        }
        public InstalledProgram() { }        
    }
}
