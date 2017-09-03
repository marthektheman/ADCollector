using System;
using System.Collections.ObjectModel;

namespace ADCollector
{
    public class Databases : ObservableCollection<DatabaseGrid> { }

    public class DatabaseGrid
    {
        private string databasename { get; set; }
        private DateTime createdate { get; set; }
        private int id { get; set;}
        public string DatabaseName
        {
            get { return databasename; }
        }
        public DateTime CreateDate
        {
            get { return createdate; }
        }
        public int ID
        {
            get { return id; }
        }
        public DatabaseGrid(String Databasename, DateTime Createdate, int Id)
        {
            databasename = Databasename;            
            createdate= Createdate;
            id = Id;
        }
        public DatabaseGrid() { }
    }
}
