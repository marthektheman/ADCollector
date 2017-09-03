using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class NetworkInterface
    {
        public string Ip { get; set; }
        public string IPEnabled { get; set; }
        public string Subnet { get; set; }
        public string Gateway { get; set; }
        public string Macaddress { get; set; }
        public string Dhcpenabled { get; set; }
        public string Description { get; set; }

        public NetworkInterface(string ip, string ipenabled, string subnet, string gateway, string macaddress, string dhcpenabled, string description)
        {
            Ip = ip;
            IPEnabled = ipenabled;
            Subnet = subnet;
            Gateway = gateway;
            Macaddress = macaddress;
            Dhcpenabled = dhcpenabled;
            Description = description;
        }
        public NetworkInterface(string ip)
        {
            Ip = ip;
        }
        public NetworkInterface() { }
    }
}
