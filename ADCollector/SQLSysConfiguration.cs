using System;
using System.Collections.ObjectModel;

namespace ADCollector
{
    public class SQLSysConfigurations : ObservableCollection<SQLSysConfiguration> { }

    public class SQLSysConfiguration
    {
        public int configid { get; set; }
        public string name { get; set; }
        public int value { get; set; }
        public int minimum { get; set; }
        public int maximum { get; set; }
        public int inuse { get; set; }
        public string description { get; set; }
        public string isdynamic { get; set; }
        public  string isadvanced { get; set; }

        public SQLSysConfiguration(int Configid, string Name, int Value, int Minimum, int Maximum, int Valueinuse, string Description, string Isdynamic, string Isadvanced)
        {
            configid = Configid;
            name = Name;
            value = Value;
            minimum = Minimum;
            maximum = Maximum;
            inuse = Valueinuse;
            description = Description;
            isdynamic = Isdynamic;
            isadvanced = Isdynamic;
        }
        public SQLSysConfiguration() { }
    }
}
