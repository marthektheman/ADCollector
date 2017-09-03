using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Windows;

namespace ADCollector
{
    public class ADComputerDetail
    {
        private String _objectClass;
        private String _samaccountname;
        private String _commonName;
        private String _operatingSystem;
        private String _operatingSystemVersion;
        private String _operatingSystemServicePack;
        private String _name;
        private String _lockedout;
        private String _description;
        private String _distinguishedName;
        private String _dnsHostname;
        private String _whencreated;
        private String _whenchanged;
        private String _lastlogon;
        private String _lastlogontimestamp;
        private String _logoncount;
        private String _badlogoncount;
        private String _guid;
        private String _sid;
        private String _useraccountcontrol;
        public String ObjectClass
        {
            get { return _objectClass; }
        }
        public String SamAccountName
        {
            get { return _samaccountname; }
        }
        public String CommonName
        {
            get { return _commonName; }
        }        
       public String OperatingSystemVersion
        {
            get { return _operatingSystemVersion; }
        }
        public String OperatingSystem
         {
            get { return _operatingSystem; }
        }
        public String OperatingSystemServicePack
        {
            get { return _operatingSystemServicePack; }
        }
        public String Name
        {
            get { return _name; }
        }
        public String LockedOut
        {
            get { return _lockedout; }
        }
        public String Description
        {
            get { return _description; }
        } 
        public String DistinguishedName
        {
            get { return _distinguishedName; }
        }
        public String DNSHostname
        {
            get { return _dnsHostname; }
        }
        public String WhenCreated
        {
            get { return _whencreated; }
        }
        public String WhenChanged
        {
            get { return _whenchanged; }
        }       
        public String LogonCount
        {
            get { return _logoncount; }
        }
        public String LastLogon
        {
            get { return _lastlogon; }
        }
        public String LastLogonTimeStamp
        {
            get { return _lastlogontimestamp; }
        }
        public String BadLogonCount
        {
            get { return _badlogoncount; }
        }
        public String GUID
        {
            get { return _guid; }
        }
        public String SID
        {
            get { return _sid; }
        }
        public String UserAccountControl
        {
            get { return _useraccountcontrol; }
        }
        private ADComputerDetail(DirectoryEntry directoryComputer)
        {
            _objectClass = GetProperty(directoryComputer, ADComputerProperties.OBJECTCLASS);
            _samaccountname = GetProperty(directoryComputer, ADComputerProperties.SAMACCOUNTNAME);
            _commonName = GetProperty(directoryComputer, ADComputerProperties.COMMONNAME);
            _operatingSystemVersion = GetProperty(directoryComputer, ADComputerProperties.OPERATINGSYSTEMVERSION);
            _operatingSystem = GetProperty(directoryComputer, ADComputerProperties.OPERATINGSYSTEM);
            _operatingSystemServicePack = GetProperty(directoryComputer, ADComputerProperties.OPERATINGSYSTEMSERVICEPACK);
            _name = GetProperty(directoryComputer, ADComputerProperties.NAME);
            _lockedout = GetProperty(directoryComputer, ADComputerProperties.LOCKEDOUT);
            _description = GetProperty(directoryComputer, ADComputerProperties.DESCRIPTION);
            _distinguishedName = GetProperty(directoryComputer, ADComputerProperties.DISTINGUISHEDNAME);
            _dnsHostname = GetProperty(directoryComputer, ADComputerProperties.DNSHOSTNAME);
            _whenchanged = GetProperty(directoryComputer, ADComputerProperties.WHENCHANGED);
            _whencreated = GetProperty(directoryComputer, ADComputerProperties.WHENCREATED);
            _lastlogon = GetProperty(directoryComputer, ADComputerProperties.LASTLOGON);
            _lastlogontimestamp = GetProperty(directoryComputer, ADComputerProperties.LASTLOGONTIMESTAMP);
            _badlogoncount = GetProperty(directoryComputer, ADComputerProperties.BADLOGONCOUNT);
            _logoncount = GetProperty(directoryComputer, ADComputerProperties.LOGONCOUNT);
            _guid = GetProperty(directoryComputer, ADComputerProperties.GUID);
            _sid = GetProperty(directoryComputer, ADComputerProperties.SID);
            _useraccountcontrol = GetProperty(directoryComputer, ADComputerProperties.USERACCOUNTCONTROL);
        }
        private static String GetProperty(DirectoryEntry ComputerDetail, String propertyName)
        {
            if (ComputerDetail.Properties.Contains(propertyName))
            {
                return ComputerDetail.Properties[propertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        public static ADComputerDetail GetComputer(DirectoryEntry directoryComputer)
        {
            return new ADComputerDetail(directoryComputer);
        }
    }
}