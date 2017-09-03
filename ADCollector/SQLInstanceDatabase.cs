using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class SQLInstanceDatabase
    {
        public string DBName { get; set; }
        public short DBID { get; set; }
        public DateTime CreateDate { get; set; }
        public List<SQLPermission> dbpermissions { get; set; }
        public List<SQLFile> dbfiles { get; set; }

        public SQLInstanceDatabase(string dbname, short dbid, DateTime createdate)
        {
            DBName = dbname;
            DBID = dbid;
            CreateDate = createdate;
            dbpermissions = new List<SQLPermission>();
            dbfiles = new List<SQLFile>();
        }
        public SQLInstanceDatabase() { }
    }
}