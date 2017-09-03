using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class SQLInstanceSystemDatabase
    {
        public string DBName { get; set; }
        public string MDF { get; set; }
        public string LDF { get; set; }

        public SQLInstanceSystemDatabase(string dbname, string mdf, string ldf)
        {
            DBName = dbname;
            MDF = mdf;
            LDF = ldf;
        }
        public SQLInstanceSystemDatabase() { }
    }
}
