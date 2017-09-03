using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADCollector
{
    public class SQLInstanceBackup
    {
        public string DBName { get; set; }
        public string RecoveryModel { get; set; }
        public string LastFull { get; set; }
        public string LastDiff { get; set; }
        public string LastLog { get; set; }
        public string LastLog2 { get; set; }

        public SQLInstanceBackup(string dbname, string recoverymodel, string lastfull, string lastdiff, string lastlog, string lastlog2)
        {
            DBName = dbname;
            RecoveryModel = recoverymodel;
            LastFull = lastfull;
            LastDiff = lastdiff;
            LastLog = lastlog;
            LastLog2 = lastlog2;
        }
        public SQLInstanceBackup() { }
    }
}

