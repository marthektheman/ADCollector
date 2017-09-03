using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Windows;

namespace ADCollector
{

    public class ADUsers
    {
        string username, pwd;
        private ADUsers(DirectoryEntry directoryUser, string user, string pass)
        {
            String domainAddress;
            String domainName;
            username = user;
            pwd = pass;
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
            _loginNameWithDomain = String.Format(@"{0}\{1}", domainName, _loginName);
            _streetAddress = GetProperty(directoryUser, ADUserProperties.STREETADDRESS);
            _city = GetProperty(directoryUser, ADUserProperties.CITY);
            _state = GetProperty(directoryUser, ADUserProperties.STATE);
            _postalCode = GetProperty(directoryUser, ADUserProperties.POSTALCODE);
            _company = GetProperty(directoryUser, ADUserProperties.COMPANY);
            _department = GetProperty(directoryUser, ADUserProperties.DEPARTMENT);
            _telePhone = GetProperty(directoryUser, ADUserProperties.TELEPHONE);
            _emailAddress = GetProperty(directoryUser, ADUserProperties.EMAILADDRESS);
            _title = GetProperty(directoryUser, ADUserProperties.TITLE);
            _manager = GetProperty(directoryUser, ADUserProperties.MANAGER);
            if (!String.IsNullOrEmpty(_manager))
            {
                String[] managerArray = _manager.Split(',');
                _managerName = managerArray[0].Replace("CN=", "");
                char[] backslash = { '\\' };
                string _mlast = _managerName.TrimEnd(backslash);
                string _mfirst = managerArray[1];
                _managerName = _mlast + "," + _mfirst;
            }

        }
        public static ADUsers GetUser(DirectoryEntry directoryUser, String user, String pass)
        {
            return new ADUsers(directoryUser, user, pass);
        }
        public string ExtractOU(string str)
        {
            if (str == null)
                return null;
            if (str.Length > 2)
            {
                //     str = str.Remove(0, 3);
                int index = str.IndexOf("ou=");
                if (index > 0)
                    str = str.Substring(index);
                return str.ToLower();
            }
            else
                return str.ToLower();
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
        private String _loginNameWithDomain;
        private String _streetAddress;
        private String _city;
        private String _state;
        private String _postalCode;
        private String _telePhone;
        private String _emailAddress;
        private String _title;
        private String _company;
        private String _manager;
        private String _managerName;
        private String _department;

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
        public String LoginName
        {
            get { return _loginName; }
        }
        public String EmployeeID
        {
            get { return _employeeID; }
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
        public String telePhone
        {
            get { return _telePhone; }
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

        public String Company
        {
            get { return _company; }
        }
        public String Department
        {
            get { return _department; }
        }
        public String DistinguishedName
        {
            get { return _distinguishedName; }
        }
        public String Manager
        {
            get
            {
                if (!String.IsNullOrEmpty(_managerName))
                {
                    ActiveDirectoryHelper ad = new ActiveDirectoryHelper();
                    return ad.GetUsersManager(_managerName, username, pwd);
                }
                return null;
            }
        }

    }
}