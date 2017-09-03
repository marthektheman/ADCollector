using System;
using System.Collections.ObjectModel;

namespace ADCollector
{
    public class AllADComputers : ObservableCollection<ADComputer> { }

    public class ADComputer
    {
        public string Host { get; set; }
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
        public string OU { get; set; }
        public string ADDescription { get; set; }
        public DateTime? WhenCreated { get; set; }
        public DateTime? WhenChanged { get; set; }
        public DateTime? LastLogon { get; set; }
        public DateTime? LastLogonTimeStamp { get; set; }
        public int LogonCount { get; set; }
        public int BadLogonCount { get; set; }
        public bool? Disabled { get; set; }
        public string GUID { get; set; }
        public string SID { get; set; }
        public string PhysicalLocation { get; set; }
        public string Hardware { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Processors { get; set; }
        public string LogicalProcessors { get; set; }
        public string LoggedOnUser { get; set; }
        public string IP { get; set; }
        public string Gateway { get; set; }
        public string SubnetMask { get; set; }
        public string Macaddress { get; set; }
        public string DHCPEnabled { get; set; }
        public string NetworkDescription { get; set; }
        public string ScanNotes { get; set; }
        public DateTime? UpdateTimeStamp { get; set; }

        public ADComputer(string host, string ip, string os, string servicepack,
            DateTime? installdate, DateTime? lastbootuptime, bool? disabled,
            DateTime? whencreated, DateTime? whenchanged, DateTime? lastlogon,
            DateTime? lastlogonTimeStamp, int badlogoncount, int logoncount, string guid, string sid,
            string physicalmemory, string freephysicalmemory, string virtualmemory,
            string freevirtualmemory, string serialnumber, string status, string domain,
            string domainrole, string ou, string addescription, string physicallocation,
            string hardware, string manufacturer, string model, string processors,
            string logicalprocessors, string loggedonuser, string gateway, string macaddress,
            string subnetmask, string dhcpenabled, string networkdescription, string scannotes, DateTime? updatetimestamp)
        {
            Host = host.ToUpper();
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
            ADDescription = addescription;
            Disabled = disabled;
            WhenCreated = whencreated;
            WhenChanged = whenchanged;
            LastLogon = lastlogon;
            LastLogonTimeStamp = lastlogonTimeStamp;
            BadLogonCount = badlogoncount;
            LogonCount = logoncount;
            GUID = guid;
            SID = sid;
            PhysicalLocation = physicallocation;
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
        public ADComputer(string hostname, string ou, string addescription)
        {
            Host = hostname;
            OU = ou;
            ADDescription = addescription;
        }
        public ADComputer() { }
    }
}
