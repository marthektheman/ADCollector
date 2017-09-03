using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.DirectoryServices;
using System.Configuration;
using System.Windows;
using System.Net;
using System.Management;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Net.NetworkInformation;


namespace ADCollector
{
    public class ActiveDirectoryHelper
    {
        private DirectoryEntry SearchRoot
        {
            get
            {
                if (_directoryEntry == null)
                {
                  //  _directoryEntry = new DirectoryEntry(LDAPPath, LDAPUser, LDAPPassword, AuthenticationTypes.Secure);
                    _directoryEntry = new DirectoryEntry(LDAPPath);
                }
                return _directoryEntry;
            }
        }
        private String LDAPPath
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPPath"];
            }
        }
        private String LDAPUser
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPUser"];
            }
        }
        private String LDAPPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPPassword"];
            }
        }
        public ADUserDetail GetUserByLoginName(String userName)
        {
            userName = userName.ToLower();
            try
            {
                _directoryEntry = null;
                DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
                directorySearch.Filter = "(&(objectClass=user)(SAMAccountName=" + userName + "))";
                SearchResult results = directorySearch.FindOne();
                if (results != null)
                {
                    DirectoryEntry user = new DirectoryEntry(results.Path, LDAPUser, LDAPPassword);
                    return ADUserDetail.GetUser(user);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        internal String GetUsersManager(String user, String username, String pwd)
        {
            try
            {
                _directoryEntry = null;
                DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
                directorySearch.Filter = "(&(objectClass=user)(cn=" + user + "))";
                SearchResult results = directorySearch.FindOne();
                if (results != null)
                {
                    DirectoryEntry thisuser = new DirectoryEntry(results.Path, username, pwd);
                    ADUsers managerinfo = ADUsers.GetUser(thisuser, username, pwd);
                    return managerinfo.DisplayName;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private String LDAPDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPDomain"];
            }
        }
        internal ADComputerDetail GetComputerBySamAccountName(String SamAccountName)
        {
            try
            {
                _directoryEntry = null;
                // DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
                DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);

                directorySearch.Filter = "(&(objectClass=computer)(sAMAccountName=" + SamAccountName + "))";
                SearchResult results = directorySearch.FindOne();
                if (results != null)
                {
                    DirectoryEntry computer = new DirectoryEntry(results.Path, LDAPUser, LDAPPassword);
                    return ADComputerDetail.GetComputer(computer);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        internal string GetComputerOperatingSystem(string Host)
        {
            ActiveDirectoryHelper adhelper = new ActiveDirectoryHelper();
            try
            {
                ADComputerDetail computerobject = adhelper.GetComputerByName(Host);
                string os = computerobject.OperatingSystem;
                return os;
            }
            catch (Exception)
            {
                return null;
            }
        }
        internal ADComputerDetail GetComputerByName(String computerName)
        {
            try
            {
                _directoryEntry = null;
                DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);

                directorySearch.Filter = "(&(objectClass=computer)(cn=" + computerName + "))";
                SearchResult results = directorySearch.FindOne();

                //      MessageBox.Show(results.Path.ToString());
                //       return null; 
                if (results != null)
                {
                    DirectoryEntry computer = new DirectoryEntry(results.Path, LDAPUser, LDAPPassword);
                    try
                    {
                        ADComputerDetail thiscomputer = ADComputerDetail.GetComputer(computer);
                        if (thiscomputer != null)
                        {
                            return thiscomputer;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (System.Net.Sockets.SocketException exc)
                    {
                        Console.Write("Exception " + exc.ToString());
                        return null;
                    }
                    catch (Exception exc)
                    {
                        Console.Write("Exception " + exc.ToString());
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (System.Net.Sockets.SocketException exc)
            {
                Console.Write("Exception " + exc.ToString());
                return null;
            }
            catch (Exception exc)
            {
                Console.Write("Exception " + exc.ToString());
                return null;
            }
        }
        internal string GetOu(string Host)
        {
            ActiveDirectoryHelper adhelper = new ActiveDirectoryHelper();
            ADComputerDetail computerobject = adhelper.GetComputerByName(Host);

            string ou;
            if (computerobject != null)
            {
                ou = computerobject.DistinguishedName.ToLower();
                ou = adhelper.ExtractOU(ou);
                return ou;
            }
            else
            {
                return null;
            }
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
        internal DirectoryEntry _directoryEntry = null;
        public string dname;
    //    private string serveradmingroup = "PCR.ServerAdmins";
     //   private string cn = "cn";
    //    private string dn = "distinguishedName";
     //   private string samaccount = "sAMAccountName";
    //    private string description = "description";
     //   private string notes = "info";
      //  private string managedby = "managedBy";
     //   private string whencreated = "whenCreated";
     //   private string whenchanged = "whenChanged";
    //    private string mail = "mail";  
     }
 }