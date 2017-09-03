using System;
using System.Collections.ObjectModel;

namespace ADCollector
{
//    public class SQLInstances : ObservableCollection<SQLInstance> { }

    public class SQLService
    {
        private string instancename { get; set; }
        private string servicename { get; set; }
        private string status { get; set; }
        public string InstanceName
        {
            get { return instancename; }
        }
        public string ServiceName
        {
            get { return servicename; }        
        }
        public string Status
        {
            get { return status; }
        }        
        public SQLService(String InstanceName, String ServiceName, String Status)
        {
            instancename = InstanceName;
            servicename = ServiceName;
            status = Status;         
        }
        public SQLService() { }
    }    
}
