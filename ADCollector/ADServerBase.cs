using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ADCollector
{
    public class AllADServersBase : ObservableCollection<ADServerBase> { }
    public class ADServerBase
    {
        public string HostName { get; set; }
        public string SamAccountName { get; set; }
        public string Description { get; set; }
        public string Hardware { get; set; }
        public string OU { get; set; }
        public string ScanNotes { get; set; }
        public DateTime? UpdateTimeStamp { get; set; }
         public ADServerBase(string hostname)
        {
            HostName = hostname;
        }
        public ADServerBase(string hostname, string samaccountname, string description, string hardware, string ou, string scannotes, DateTime? updatetimestamp)
        {
            HostName = hostname;
            SamAccountName = samaccountname;
            Description = description;
            Hardware = hardware;
            OU = ou;
            ScanNotes = scannotes;
            UpdateTimeStamp = updatetimestamp;
            
        }
        public ADServerBase() { }
    }
}
