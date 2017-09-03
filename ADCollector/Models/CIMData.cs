using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class CIMData
    {
        public ADServer Server { get; set; }
        public List<NetworkInterface> Networkinterfacelist { get; set; }
        public List<Drive> Drives { get; set; }
        public List<Patch> Patches { get; set; }
        public List<InstalledProgram> Programs { get; set; }
        public string Scannotes { get; set; }
        public CIMData(ADServer server, List<NetworkInterface> networkinterfacelist, List<Drive> drives, List<Patch> patches, List<InstalledProgram> programs, string scannotes)
        {
            Server = server;
            Networkinterfacelist = networkinterfacelist;
            Drives = drives;
            Patches = patches;
            Programs = programs;
            Scannotes = scannotes;
        }
        public CIMData() { }
    }
}
