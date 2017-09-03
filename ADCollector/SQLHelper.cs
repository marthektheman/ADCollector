using System;


namespace ADCollector
{
    class SQLHelper
    {
        internal String GetDBConnectionString()
        {
            //   string connectionstring = "Data Source=DCPWDBS447;Initial Catalog=\"ITSupport\";Integrated Security=True";
            string connectionstring = "Data Source=" + Properties.Settings.Default.DatabaseInstance +
             ";Initial Catalog=" + Properties.Settings.Default.Database +
             ";User ID=" + Properties.Settings.Default.Username +
             ";Password=" + Properties.Settings.Default.Pwd +
             ";Persist Security Info=True;Connection Timeout=3600;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=10000;Pooling=true";
         
       //     string connectionstring = Properties.Settings.Default.ADInventoryTestConnectionString;
       //     string connectionstring = Properties.Settings.Default.ADInventoryConnectionString;
            return connectionstring;
        }  
        internal String GetUtilizationConnectionString()
        {
            string connectionstring = "Data Source=" + Properties.Settings.Default.UtilizationDatabaseInstance +
              ";Initial Catalog=" + Properties.Settings.Default.UtilizationDatabase +
              ";User ID=" + Properties.Settings.Default.UtilizationUsername +
              ";Password=" + Properties.Settings.Default.UtilizationPassword +
              ";Persist Security Info=True;Connection Timeout=3600;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=10000;Pooling=true";
            return connectionstring;
        }      
    }
}
