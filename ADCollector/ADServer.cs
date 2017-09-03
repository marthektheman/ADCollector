using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ADCollector
{
    public class AllADServers : ObservableCollection<ADServer> { }   
    public class ADServer : ADServerBase
    {
        public string OS { get; set; }
        public string ServicePack { get; set; }
        public string SerialNumber { get; set; }
        public DateTime? InstallDate { get; set; }
        public DateTime? LastBootUpTime { get; set; }
        public string PhysicalMemory { get; set; }
        public string FreePhysicalMemory { get; set; }
        public string VirtualMemory { get; set; }
        public string FreeVirtualMemory { get; set; }
        public string Status { get; set; }
        public string Domain { get; set; }
        public string DomainRole { get; set; }
        public DateTime? WhenCreated { get; set; }
        public DateTime? WhenChanged { get; set; }
        public DateTime? LastLogon { get; set; }
        public int LogonCount { get; set; }
        public int BadLogonCount { get; set; }
        public bool? Disabled { get; set; }
        public string GUID { get; set; }
        public string SID { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Processors { get; set; }
        public string LogicalProcessors { get; set; }
        public string LoggedOnUser { get; set; }
        public string IP { get; set; }
        public string Gateway { get; set; }
        public string SubnetMask { get; set; }
        public string Macaddress { get; set; }
        public bool? DHCPEnabled { get; set; }
        public string NetworkDescription { get; set; }
 
        public ADServer(string host, string samaccountname, string ip, string os, string servicepack, 
            DateTime? installdate, DateTime? lastbootuptime, bool? disabled, 
            DateTime? whencreated, DateTime? whenchanged, DateTime? lastlogon,
            int badlogoncount, int logoncount, string guid, string sid,
            string physicalmemory, string freephysicalmemory, string virtualmemory, 
            string freevirtualmemory, string serialnumber, string status, string domain, 
            string domainrole, string ou, string description, string hardware, string manufacturer, 
            string model, string processors, string logicalprocessors, string loggedonuser, 
            string gateway, string macaddress,  string subnetmask, 
            bool? dhcpenabled, string networkdescription, string scannotes, DateTime? updatetimestamp)
        {
            HostName = host.ToUpper();
            SamAccountName = samaccountname;
            OS = os;
            ServicePack = servicepack;
            SerialNumber = serialnumber;
            InstallDate = installdate;
            LastBootUpTime = lastbootuptime;
            Status = status;            
            PhysicalMemory = physicalmemory;
            FreePhysicalMemory = freephysicalmemory;
            VirtualMemory = virtualmemory;
            FreeVirtualMemory = freevirtualmemory;
            Domain = domain;
            DomainRole = domainrole;           
            OU = ou;
            Description = description;
            Disabled = disabled;
            WhenCreated = whencreated;
            WhenChanged = whenchanged;
            LastLogon = lastlogon;
            BadLogonCount = badlogoncount;
            LogonCount = logoncount;
            GUID = guid;
            SID = sid;
            Hardware = hardware;
            Manufacturer = manufacturer;
            Model = model;
            Processors = processors;
            LogicalProcessors = logicalprocessors;
            LoggedOnUser = loggedonuser;
            IP = ip;
            SubnetMask = subnetmask;
            Gateway = gateway;
            Macaddress = macaddress;
            DHCPEnabled = dhcpenabled;
            NetworkDescription = networkdescription;
            ScanNotes = scannotes;
            UpdateTimeStamp = updatetimestamp;
        }         
        public ADServer(string samaccountname)
        {
            SamAccountName = samaccountname;
            HostName = SamAccountName.TrimEnd('$');           
        }   
    public ADServer() { }  
    }
}
