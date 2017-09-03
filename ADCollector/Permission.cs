using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ADCollector
{
    public class Permissions : ObservableCollection<Permission> { }
    public class Permission
    {
        public string Host { get; set; }
        public string Instance { get; set; }
        public string ServerRole { get; set; }
        public string UserName { get; set; }
      
        public Permission(string host, string instance, string role, string member)
        {
            Instance = instance;
            Host = host;
            ServerRole = role;
            UserName = member;
        }
        public Permission() { }
    }
}
