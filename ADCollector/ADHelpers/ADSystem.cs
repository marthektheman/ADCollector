using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class ADSystems : ObservableCollection<ADSystem> { }
    public class ADSystem
    {

        public string Host { get; set; }
        public string OS { get; set; }
        public string Description { get; set; }
        public string WhenCreated { get; set; }
        public string OU { get; set; }
      
        public ADSystem(string host, string os, string description, string whencreated, string ou)
        {
            Host = host;
            OS = os;
            Description = description;
            WhenCreated = whencreated;
            OU = ou;
        }       
        public ADSystem() { }
    }
}
