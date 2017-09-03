using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;
using System.Windows;

namespace ADCollector
{
    public class ADManager
    {
        PrincipalContext context;     

        public ADManager()
        {
            context = new PrincipalContext(ContextType.Machine, "xxx", "xxx", "xxx");
        }
        public ADManager(string domain, string container)
        {
            context = new PrincipalContext(ContextType.Domain, domain, container);
        }
        public ADManager(string domain, string username, string password)
        {
            context = new PrincipalContext(ContextType.Domain, username, password);
        }
        public bool CreateComputerObject(string computerName)
        {
         //   string newcomputername = computerName + "@root.sutterhealth.org";
            ComputerPrincipal ComputerPrincipal = new ComputerPrincipal(context);
            ComputerPrincipal.Name = computerName;
            
            ComputerPrincipal.DisplayName = computerName;
            ComputerPrincipal.SamAccountName = computerName + "$";
            ComputerPrincipal.Enabled = true;
            ComputerPrincipal.Save();
            if (ComputerPrincipal != null)
                return true;
            else
                return false;
        }
        public bool CreateUserObject(string userName, string password)
        {
            string newusername = userName + "@root.sutterhealth.org";
            UserPrincipal UserPrincipal = new UserPrincipal(context, userName, password, true);
            UserPrincipal.UserPrincipalName = newusername;
            UserPrincipal.GivenName = userName;
            UserPrincipal.Surname = userName;
            UserPrincipal.DisplayName = userName;
            UserPrincipal.Save();
            if (UserPrincipal != null)
                return true;
            else
                return false;
        }
        public bool AddUserToGroup(string userName, string groupName)
        {
            bool done = false;
            GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupName);
            if (group == null)
            {
                group = new GroupPrincipal(context, groupName);
            }
            UserPrincipal user = UserPrincipal.FindByIdentity(context, userName);
            if (user != null & group != null)
            {
                group.Members.Add(user);
                group.Save();
                done = (user.IsMemberOf(group));
            }
            return done;
        }
        public bool AddComputerToGroup(string computerName, string groupName)
        {
            bool done = false;
            GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupName);
            if (group == null)
            {
                group = new GroupPrincipal(context, groupName);
            }
            ComputerPrincipal computer = ComputerPrincipal.FindByIdentity(context, computerName);
            if (computer != null & group != null)
            {
                group.Members.Add(computer);
                group.Save();
                done = (computer.IsMemberOf(group));
            }
            return done;
        }
        public bool RemoveUserFromGroup(string userName, string groupName)
        {
            bool done = false;
            UserPrincipal user = UserPrincipal.FindByIdentity(context, userName);
            GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupName);
            if (user != null & group != null)
            {
                group.Members.Remove(user);
                group.Save();
                done = !(user.IsMemberOf(group));
            }
            return done;
        } 
    }
}