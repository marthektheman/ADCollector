using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Windows;

namespace ADCollector
{

    public class ADUserDetail
    {
        private ADUserDetail(DirectoryEntry directoryUser)
        {
            String domainAddress;
            String domainName;
            _firstName = GetProperty(directoryUser, ADUserProperties.FIRSTNAME);
            _middleName = GetProperty(directoryUser, ADUserProperties.MIDDLENAME);
            _lastName = GetProperty(directoryUser, ADUserProperties.LASTNAME);
            _displayName = GetProperty(directoryUser, ADUserProperties.DISPLAYNAME);
            _distinguishedName = GetProperty(directoryUser, ADUserProperties.DISTINGUISHEDNAME);
            _loginName = GetProperty(directoryUser, ADUserProperties.LOGINNAME);
            _employeeID = GetProperty(directoryUser, ADUserProperties.EMPLOYEEID);
            String userPrincipalName = GetProperty(directoryUser, ADUserProperties.USERPRINCIPALNAME);
            if (!string.IsNullOrEmpty(userPrincipalName))
            {
                domainAddress = userPrincipalName.Split('@')[1];
            }
            else
            {
                domainAddress = String.Empty;
            }
            if (!string.IsNullOrEmpty(domainAddress))
            {
                domainName = domainAddress.Split('.').First();
            }
            else
            {
                domainName = String.Empty;
            }
            _streetAddress = GetProperty(directoryUser, ADUserProperties.STREETADDRESS);
            _city = GetProperty(directoryUser, ADUserProperties.CITY);
            _state = GetProperty(directoryUser, ADUserProperties.STATE);
            _postalCode = GetProperty(directoryUser, ADUserProperties.POSTALCODE);
            _country = GetProperty(directoryUser, ADUserProperties.COUNTRY);
            _company = GetProperty(directoryUser, ADUserProperties.COMPANY);
            _department = GetProperty(directoryUser, ADUserProperties.DEPARTMENT);
            _telePhone = GetProperty(directoryUser, ADUserProperties.TELEPHONE);
            _lastlogon = GetProperty(directoryUser, ADUserProperties.LASTLOGON);
            _logoncount = GetProperty(directoryUser, ADUserProperties.LOGONCOUNT);
            _whencreated = GetProperty(directoryUser, ADUserProperties.WHENCREATED);
            _whenchanged = GetProperty(directoryUser, ADUserProperties.WHENCHANGED);
            _homePhone = GetProperty(directoryUser, ADUserProperties.HOMEPHONE);
            _extension = GetProperty(directoryUser, ADUserProperties.EXTENSION);
            _mobile = GetProperty(directoryUser, ADUserProperties.MOBILE);
            _fax = GetProperty(directoryUser, ADUserProperties.FAX);
            _emailAddress = GetProperty(directoryUser, ADUserProperties.EMAILADDRESS);
            _title = GetProperty(directoryUser, ADUserProperties.TITLE);
            _distinguishedname = GetProperty(directoryUser, ADUserProperties.DISTINGUISHEDNAME); 
            _proxyaddresses = GetProperty(directoryUser, ADUserProperties.PROXYADDRESSES);            
            _manager = GetProperty(directoryUser, ADUserProperties.MANAGER);
            if (!String.IsNullOrEmpty(_manager))
            {
                String[] managerArray = _manager.Split(',');
                _managerName = managerArray[0].Replace("CN=", "");
                char[] backslash = {'\\'};
                string _mlast = _managerName.TrimEnd(backslash);
                string _mfirst = managerArray[1];
                _managerName = _mlast + "," + _mfirst;
            }
        }
        public static ADUserDetail GetUser(DirectoryEntry directoryUser)
        {
            return new ADUserDetail(directoryUser);
        }
        private static String GetProperty(DirectoryEntry userDetail, String propertyName)
        {
            if (userDetail.Properties.Contains(propertyName))
            {
                return userDetail.Properties[propertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        private String _firstName;
        private String _middleName;
        private String _lastName;
        private String _displayName;
        private String _distinguishedName;
        private String _loginName;
        private String _employeeID;
        private String _streetAddress;
        private String _city;
        private String _state;
        private String _postalCode;
        private String _country;
        private String _homePhone;
        private String _telePhone;
        private String _extension;
        private String _mobile;
        private String _fax;
        private String _emailAddress;
        private String _title;
        private String _company;
        private String _proxyaddresses;
        private String _manager;
        private String _managerName;
        private String _department;
        private String _lastlogon;
        private String _logoncount;
        private String _whencreated;
        private String _whenchanged;
        private String _distinguishedname;
        

        public String LastName
        {
            get { return _lastName; }
        }
        public String FirstName
        {
            get { return _firstName; }
        }
        public String MiddleName
        {
            get { return _middleName; }
        }        
        public String DisplayName
        {
            get { return _displayName; }
        }
        public String DistinguishedName
        {
            get { return _distinguishedName; }
        }
        public String LoginName
        {
            get { return _loginName; }
        }
        public String EmployeeID
        {
            get { return _employeeID; }
        }
        public String LastLogon
        {
            get { return _lastlogon; }
        }
        public String LogonCount
        {
            get { return _logoncount; }
        }
        public String WhenCreated
        {
            get { return _whencreated; }
        }
        public String WhenChanged
        {
            get { return _whenchanged; }
        }
        public String StreetAddress
        {
            get { return _streetAddress; }
        }
        public String City
        {
            get { return _city; }
        }
        public String State
        {
            get { return _state; }
        }
        public String PostalCode
        {
            get { return _postalCode; }
        }
        public String Country
        {
            get { return _country; }
        }
        public String HomePhone
        {
            get { return _homePhone; }
        }
        public String telePhone
        {
            get { return _telePhone; }
        }
        public String Extension
        {
            get { return _extension; }
        }
        public String Mobile
        {
            get { return _mobile; }
        }
        public String EmailAddress
        {
            get { return _emailAddress; }
        }
        public String Title
        {
            get { return _title; }
        }
        public String ManagerName
        {
            get { return _managerName; }
        }
        public String Department
        {
            get { return _department; }
        }
        public String ProxyAddresses
        {
            get { return _proxyaddresses; }
        }
        public String Company
        {
            get { return _company; }
        }                  
    }
}