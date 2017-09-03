using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class SQLInstance
    {
        public string Instancename { get; set; }
        public string Edition { get; set; }
        public string Productlevel { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public SQLInstanceMemory memory { get; set; }
        public List<SQLInstanceSystemDatabase> systemdatabases { get; set; }
        public List<SQLPermission> permissions { get; set; }
        public List<SQLInstanceBackup> backups { get; set; }
        public List<SQLInstanceDatabase> databases { get; set; }  

        public SQLInstance(string instance, string edition, string productlevel, string type, string verion)
        {
            Instancename = instance;
            Edition = edition;
            Productlevel = productlevel;
            Type = type;
            Version = verion;
            memory = new SQLInstanceMemory();
            systemdatabases = new List<SQLInstanceSystemDatabase>();
            permissions = new List<SQLPermission>();
            backups = new List<SQLInstanceBackup>();
            databases = new List<SQLInstanceDatabase>();
        }
        public SQLInstance() { }
    }
}
