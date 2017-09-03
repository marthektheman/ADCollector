using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Windows;
//using System.Windows.Controls;

namespace ADCollector
{

    public class ADUserList
    {
        private ADUserList(DirectoryEntry directoryUser)
        {
            _firstName = GetProperty(directoryUser, ADUserProperties.FIRSTNAME);
            _middleName = GetProperty(directoryUser, ADUserProperties.MIDDLENAME);
            _lastName = GetProperty(directoryUser, ADUserProperties.LASTNAME);
            _displayName = GetProperty(directoryUser, ADUserProperties.DISPLAYNAME);
            _loginName = GetProperty(directoryUser, ADUserProperties.LOGINNAME);
            
            _streetAddress = GetProperty(directoryUser, ADUserProperties.STREETADDRESS);
            _city = GetProperty(directoryUser, ADUserProperties.CITY);
            _company = GetProperty(directoryUser, ADUserProperties.COMPANY);
            _department = GetProperty(directoryUser, ADUserProperties.DEPARTMENT);
            _telePhone = GetProperty(directoryUser, ADUserProperties.TELEPHONE);
            _emailAddress = GetProperty(directoryUser, ADUserProperties.EMAILADDRESS);
            _title = GetProperty(directoryUser, ADUserProperties.TITLE);
            _whencreated = GetProperty(directoryUser, ADUserProperties.WHENCREATED);
            _whenchanged = GetProperty(directoryUser, ADUserProperties.WHENCHANGED);
            _distinguishedname = GetProperty(directoryUser, ADUserProperties.DISTINGUISHEDNAME);
        }
        public static ADUserList GetUser(DirectoryEntry directoryUser)
        {
            return new ADUserList(directoryUser);
        }
        public string ExtractOU(string str)
        {
            if (str == null)
                return null;
            if (str.Length > 2)
            {
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
        private String _loginName;
        private String _streetAddress;
        private String _city;
        private String _telePhone;
        private String _emailAddress;
        private String _title;
        private String _company; 
        private String _department;
        private String _whencreated;
        private String _whenchanged;
        private String _distinguishedname;

        public String FirstName
        {
            get { return _firstName; }
        }
        public String MiddleName
        {
            get { return _middleName; }
        }
        public String LastName
        {
            get { return _lastName; }
        }
        public String DisplayName
        {
            get { return _displayName; }
        }
        public String LoginName
        {
            get { return _loginName; }
        }
        public String StreetAddress
        {
            get { return _streetAddress; }
        }
        public String City
        {
            get { return _city; }
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
        public String Company
        {
            get { return _company; }
        }
        public String Department
        {
            get { return _department; }
        }
        public String WhenCreated
        {
            get { return _whencreated; }
        }
        public String WhenChanged
        {
            get { return _whenchanged; }
        }
        public String DinstinguishedName
        {
            get { return _distinguishedname; }
        }
    }
}