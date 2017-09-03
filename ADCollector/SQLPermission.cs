using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class SQLPermission
    {
        public string Role { get; set; }
        public string Member { get; set; }

        public SQLPermission(string role, string member)
        {
            Role = role;
            Member = member;
        }
        public SQLPermission() { }
    }
}
