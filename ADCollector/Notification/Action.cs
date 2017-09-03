using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ADCollector
{
    public class AllActions : ObservableCollection<Action> { }
    public class Action
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Telephone { get; set; }
        public string EmailAddress { get; set; }
        public string ManagerName { get; set; }
        public string Department { get; set; }
        public string Activity { get; set; }
        public string Tool { get; set; }
        public DateTime? TimeLogged { get; set; }
        public Action(string username, string displayname, string telephone, string managername, string emailaddress, string department, string activity, string tool, DateTime? updatetimestamp)
        {
            Username = username;
            DisplayName = displayname;
            Telephone = telephone;
            ManagerName = managername;
            EmailAddress = emailaddress;
            Department = department;
            Activity = activity;
            Tool = tool;
            TimeLogged = updatetimestamp;
        }
        public Action() { }
    }
}
