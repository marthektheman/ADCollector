using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    class ADComputerProperties
    {
        public const String OBJECTCLASS = "objectClass";
        public const String SAMACCOUNTNAME = "sAMAccountName";
        public const String COMMONNAME = "cn";
        public const String OPERATINGSYSTEMVERSION = "operatingSystemVersion";
        public const String OPERATINGSYSTEM = "operatingSystem";
        public const String OPERATINGSYSTEMSERVICEPACK = "operatingSystemServicePack";
        public const String NAME = "name";
        public const String LOCKEDOUT = "lockedoutTime";
        public const String DESCRIPTION = "description";
        public const String DISTINGUISHEDNAME = "DistinguishedName";
        public const String DNSHOSTNAME = "dNSHostName";
        public const String WHENCREATED = "whenCreated";
        public const String WHENCHANGED = "whenChanged";
        public const String LASTLOGON = "lastLogon";
        public const String LASTLOGONTIMESTAMP = "lastLogonTimeStamp";
        public const String LOGONCOUNT = "logonCount";
        public const String BADLOGONCOUNT = "BadLogonCount";
        public const String GUID = "objectGUID";
        public const String SID = "objectSid";
        public const String USERACCOUNTCONTROL = "userAccountControl";
    }
}

/*
 * complete computer properties from get-adcomputer
 * check adsiedit
 * */

