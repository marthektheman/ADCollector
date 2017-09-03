using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Management;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Globalization;
using System.Diagnostics;
using System.ServiceProcess;
using System.Data;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

namespace ADCollector
{
    class Program
    {
        static string DATABASEINSTANCE = Properties.Settings.Default.Instance;
        static string DATABASE = Properties.Settings.Default.Database;
        static string USERNAME = Properties.Settings.Default.Username;
        static string PWD = Properties.Settings.Default.Pwd;
        static string searchou, ldapquery;
        static int permissiongranted = 0;
        static int permissiondenied = 0;
        static string connectdb;
        static int gigabytes = 1073741824;
        static string memstring = string.Empty;
        static string syspermstring = "Hostname,SQL Instance,Role,Principle\n";
        static string dbpermstring = string.Empty;
        static string systemfile = string.Empty;
        static string dbfilestring = string.Empty;
        static string TOEMAIL = Properties.Settings.Default.toemail;
        static string CCEMAIL = Properties.Settings.Default.ccemail;
        static int sqlinstances = 0;

        //SQL sysadmin accounts
        //done 11/16/2016 on SHBA OUs
        static string SYSADMINACCOUNT = Properties.Settings.Default.SysAdminAccount;
        static bool addsystemadmin = false;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("\nError:  No arguments\n");
                Console.ReadLine();
            }
            else if (args.Length == 1)
            {

                Console.WriteLine("\nError:  Missing argument" + "\n");
                Console.ReadLine();
            }
            else if (args.Length == 2)
            {
                searchou = args[0];
                if (args[1] == "All")
                {
                    ldapquery = "(ObjectCategory=computer)";
                    connectdb = @"Data Source=" + DATABASEINSTANCE + ";Initial Catalog=" + Properties.Settings.Default.AllDatabase + ";User ID=" + USERNAME + ";Password=" + PWD + ";Persist Security Info=True;Connection Timeout=3600;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=100000;Pooling=true";
                }
                else if (args[1] == "Desktops")
                {
                    //ldapquery = "(&(ObjectCategory=computer)(operatingSystem=Windows 7*))";
                    ldapquery = "(|(&(ObjectCategory=computer)(operatingSystem=Windows 7*))(operatingSystem=Windows XP*))";
                    connectdb = @"Data Source=" + DATABASEINSTANCE + ";Initial Catalog=" + Properties.Settings.Default.WorkstationDatabase + ";User ID=" + USERNAME + ";Password=" + PWD + ";Persist Security Info=True;Connection Timeout=3600;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=100000;Pooling=true";
                }
                else if (args[1] == "Servers")
                {
                    ldapquery = "(&(ObjectCategory=computer)(operatingSystem=*Server*))";
                    connectdb = @"Data Source=" + DATABASEINSTANCE + ";Initial Catalog=" + Properties.Settings.Default.Database + ";User ID=" + USERNAME + ";Password=" + PWD + ";Persist Security Info=True;Connection Timeout=3600;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=100000;Pooling=true";
                }
                else if (args[1] == "SQL Servers")
                {
                    //       ldapquery = "(&(ObjectCategory=computer)(operatingSystem=*Server*))";
                    ldapquery = "(&(objectCategory=computer)(servicePrincipalName=MSSQLSVC*)(operatingSystem=*Server*))";
                    connectdb = @"Data Source=" + DATABASEINSTANCE + ";Initial Catalog=" + Properties.Settings.Default.SQLDatabase + ";User ID=" + USERNAME + ";Password=" + PWD + ";Persist Security Info=True;Connection Timeout=3600;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=100000;Pooling=true";
                }
                else if (args[1] == "Test")
                {
                    ldapquery = "(&(ObjectCategory=computer)(operatingSystem=*Server*))";
                    //ldapquery = "(&(ObjectCategory=computer)(operatingSystem=Windows 7*))";
                    //ldapquery = "(|(&(ObjectCategory=computer)(operatingSystem=Windows 7*))(operatingSystem=Windows XP*))";
                    //ldapquery = "(&(objectCategory=computer)(servicePrincipalName=MSSQLSVC*)(operatingSystem=*Server*))";
                    connectdb = @"Data Source=" + DATABASEINSTANCE + ";Initial Catalog="+ Properties.Settings.Default.TestDatabase + ";User ID=" + USERNAME + ";Password=" + PWD + ";Persist Security Info=True;Connection Timeout=3600;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=100000;Pooling=true";
                }
                else if (args[1] == "Sysadmin")
                {
                    ldapquery = "(&(ObjectCategory=computer)(operatingSystem=*Server*))";
                    //ldapquery = "(&(objectCategory=computer)(servicePrincipalName=MSSQLSVC*)(operatingSystem=*Server*))";
                    //   connectdb = Properties.Settings.Default.ADInventoryTestConnectionString;
                    addsystemadmin = true;
                    Console.Write(DISPLAYLINE + "Scanning SQL Instances and adding " + SYSADMINACCOUNT + " to the SQL instance sysadmin role:\nScanning OU: " + searchou + "\nLDAP Filter: " + ldapquery + DISPLAYLINE);
                }
                if (!addsystemadmin)
                {
                    Console.Write(DISPLAYLINE + "Scanning and collecting server stats in OU:\n" + searchou + "\nUsing search filter:" + ldapquery + DISPLAYLINE);
                }
                CollectOUSystemStats();
            }
            else
            {
                Console.WriteLine("Too many args" + "\n");
                Console.ReadLine();
            }
        }
        static private List<SQLService> GetSQLServices(string samaccountname)
        {
            List<SQLService> allinstances = new List<SQLService>();

            string systemname = samaccountname.Trim('$');
            string servicename = "MSSQL$";
            string servicename2 = "MSSQLSERVER";
            //    string servicename2 = "SQLAgent";
            //    string servicename3 = "SQL Server";
            //   string servicename4 = "msftesql";
            //   string serviceoutput = string.Empty;
            try
            {
                ServiceController[] services = ServiceController.GetServices(systemname);
                foreach (ServiceController service in services)
                {
                    if (service != null && !service.DisplayName.Contains("Windows Internal Database") && (service.ServiceName.Contains(servicename) || service.ServiceName.Contains(servicename2)))
                    {
                        if (service.Status.ToString() != "Stopped")
                        {
                            sqlinstances++;
                            SQLService thisinstance = new SQLService(service.DisplayName, service.ServiceName, service.Status.ToString());
                            allinstances.Add(thisinstance);
                            Console.WriteLine(systemname + " " + service.ServiceName + " started. Statistics will be collected.");
                        }
                        else
                        {
                            Console.WriteLine(systemname + " " + service.ServiceName + " stopped.  No statistics will be collected.");
                        }
                    }
                }
            }
      //      catch (InvalidOperationException)
      //      {
      //                Console.Write("Error, server unavailable " + selectedhostname + "\n");
      //      }
            catch (Exception)
            {
                Console.Write("Error, server unavailable " + systemname + "\n");
            //    Console.WriteLine("Exception:\n\n" + exc.ToString());
            }
            return allinstances;
        }
        public static CIMData LoadCIMData(string samaccountname, ADServer adserver)
        {
            CIMData thisdata = new CIMData();
            string systemname = samaccountname.TrimEnd('$');
            CimSession Session = CimSession.Create(systemname);
            thisdata.Scannotes = LoadWin32_OperatingSystem(Session, ref adserver);
            if (thisdata.Scannotes.Equals("Permission to CIM Query"))
            {
                Console.WriteLine(systemname + " allowed CIM query access. Gathering extended statistics...");
                LoadWin32_ComputerSystem(Session, ref adserver);
                thisdata.Server = adserver;
                thisdata.Networkinterfacelist = LoadNICs(Session);
                thisdata.Drives = LoadInstalledDisks(Session);
                thisdata.Patches = LoadInstalledPatches(Session);
                thisdata.Programs = LoadInstalledPrograms(Session);
            }
            else
            {
                Console.WriteLine(systemname + " denied access to CIM Query.  Skipping CIM collections.");
            }
            return thisdata;
        }
        private static void CollectOUSystemStats()
        {
            string days = "";
            string hours = "";
            string minutes = "";
            string seconds = "";
            Stopwatch stopwatch = new Stopwatch();
            SearchResultCollection results = null;
            try
            {
                string searchpath = Properties.Settings.Default.ROOT + "/" + searchou;
                DirectoryEntry searchroot = new DirectoryEntry(searchpath);
                DirectorySearcher mySearcher = new DirectorySearcher(searchroot);
                mySearcher.Filter = ldapquery;
                mySearcher.PageSize = 3000000;

                //uncomment after testing
                results = mySearcher.FindAll();

                //comment after testing
               //int totalobjects = 2;
                int totalobjects = results.Count;

                if (totalobjects > 0)
                {
                    //uncomment after testing
                    //   Console.Write(DISPLAYLINE + "There are " + results.Count.ToString() + " computer objects to scan in OU:\n" + searchpath + "\n");
                    Logger log = new Logger("Started Scan on OU: " + searchou, Environment.UserName, Properties.Settings.Default.ApplicationName + " v" + Properties.Settings.Default.version);
                    try
                    {
                        Console.WriteLine("Sending notification to " + log.ALERTEMAIL);
                        log.AlertAdmin();
                    }
                    catch (Exception exc)
                    {
                        Console.Write("Exception:\n" + exc.ToString());

                    }
                    stopwatch.Start();
                    Task[] tasks = new Task[results.Count];
                    //comment after testing
                    //Task[] tasks = new Task[2];
                    int i = 0;
                    //comment after testing
                    //for (int j = 0; j < 2; j++)
                    foreach (SearchResult result in results)
                    {
                         string samaccountname = result.GetDirectoryEntry().Properties["sAMAccountName"].Value.ToString();
                        //uncomment to test
                        /*     string samaccountname = "DCPWDBS447$";
                              if (j == 1)
                              {
                                  samaccountname = "DCQWDBS222$";
                              }
                        */
                        if (samaccountname != null && samaccountname != string.Empty)
                        {
                            if (addsystemadmin)
                            {
                                Console.WriteLine("Fix as it now returns a list of instances");
                                //tasks[i] = Task.Factory.StartNew(() => GatherSQLInstances(samaccountname, -1));
                            }
                            else
                            {
                                tasks[i] = Task.Factory.StartNew(() => CollectSystemStats(samaccountname));
                            }
                        }
                        i++;
                    }
                    //   CollectSystemStats("ALDBOGCXD315$");
                    //   CollectSystemStats("DCPWDBS447$");
                    //   CollectSystemStats("DCQWDBS222$");
                    Task.WaitAll(tasks);
                    Console.WriteLine(DISPLAYLINE);
                    //results.Dispose();
                    stopwatch.Stop();
                    TimeSpan span = stopwatch.Elapsed;
                    days = span.Days.ToString();
                    hours = span.Hours.ToString();
                    minutes = span.Minutes.ToString();
                    seconds = span.Seconds.ToString();
                    //uncomment after testing
                    log.SetMessage = "Ended Scan on OU: " + searchou + "\nLDAP QUERY: " + ldapquery + "\nCollection Duration: " + days + " days " + hours + " hours " + minutes + " minutes " + seconds + " seconds\nItem Scanned: " + results.Count + "\nSQL Instances: " + sqlinstances;
                    log.AlertAdmin();
                    Console.WriteLine(DISPLAYLINE + "All " + results.Count.ToString() + " systems  were scanned using: " + searchpath + "\nThe results have been entered into the database.\nLDAP Filter: " + ldapquery + "\nCollection Duration: " + days + " days " + hours + " hours " + minutes + " minutes " + seconds + " seconds.\nSystems Scanned: " + results.Count + "\nSQL Instances: " + sqlinstances + "\n" + DISPLAYLINE);

                    if (addsystemadmin)
                    {
                        Console.WriteLine(DISPLAYLINE + "SQL servers granting sysadmin addition: " + permissiongranted.ToString() + "\nSQL servers denying sysadmin Permission: " + permissiondenied.ToString() + "\n" + DISPLAYLINE);
                    }
                    Thread.Sleep(2000);
                    //     Console.Read();
                }
            }
            catch (DirectoryServicesCOMException exc)
            {
                Console.WriteLine("Error with OU:" + exc.ToString());

            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: Invalid credentials.  Please check your account out and try again" + exc.ToString());
            }
        }
        static private void CollectSystemStats(string samaccountname)
        {
            string systemname = samaccountname.TrimEnd('$');
            //gather all data before attempting to open and push into db tables.
            try
            {
                if (samaccountname != null || samaccountname != string.Empty)
                {                    
                    Console.Write("Scanning system in new thread: " + systemname + "\n");
                    ActiveDirectoryHelper adhelper = new ActiveDirectoryHelper();
                    ADComputerDetail computerobject = adhelper.GetComputerBySamAccountName(samaccountname);
                    if (computerobject == null)
                    {
                        Console.WriteLine(samaccountname + " IS NULL! Inspect AD computer object.");
                    }
                    else
                    {
                        int? location = LoadLocation(samaccountname);
                        DateTime now = DateTime.Now;                       
                        DateTime? created;
                        if (computerobject.WhenCreated == null || computerobject.WhenCreated == string.Empty)
                        {
                            created = null;
                        }
                        else
                        {
                            created = Convert.ToDateTime(computerobject.WhenCreated);
                        }
                        bool? disabled = false;
                        int accountflag = Convert.ToInt32(computerobject.UserAccountControl);
                        disabled = false;
                        if ((accountflag & 2) > 0)
                        {
                            disabled = true;
                        }
                        DateTime? changed;
                        if (computerobject.WhenChanged == null || computerobject.WhenChanged == string.Empty)
                        {
                            changed = null;
                        }
                        else
                        {
                            changed = Convert.ToDateTime(computerobject.WhenChanged);
                        }
                        string ou = string.Empty;
                        try
                        {
                            ou = adhelper.GetOu(samaccountname.TrimEnd('$'));
                            if (ou == null)
                            {
                                ou = "ERROR Accessing computer object";
                            }
                        }
                        catch (Exception exc)
                        {
                            ou = "Exception thrown with collecting OU";
                            Console.Write("Exception: " + exc.ToString());
                        }
                        int? logoncount = 0;
                        string logoncountstring = computerobject.LogonCount;
                        if (logoncountstring != null)
                        {
                            try
                            {
                                logoncount = Convert.ToInt32(logoncountstring);
                            }
                            catch
                            {
                                logoncount = 0;
                            }
                        }
                        else
                        {
                            logoncount = 0;
                        }
                        int? badlogoncount = 0;
                        string badlogoncountstring = computerobject.BadLogonCount;
                        if (badlogoncountstring != null)
                        {
                            try
                            {
                                badlogoncount = Convert.ToInt32(badlogoncountstring);
                            }
                            catch
                            {
                                badlogoncount = 0;
                            }
                        }
                        else
                        {
                            badlogoncount = 0;
                        }
                        string guid = string.Empty;
                        string sid = string.Empty;
                        DateTime? lastlogon = null;
                        PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                        ComputerPrincipal systemprinciple = ComputerPrincipal.FindByIdentity(ctx, samaccountname);
                        if (systemprinciple != null)
                        {
                            if (systemprinciple.Guid != null)
                            {
                                guid = systemprinciple.Guid.ToString();
                            }
                            else
                            {
                                guid = "not found";
                            }
                            if (systemprinciple.Sid != null)
                            {
                                sid = systemprinciple.Sid.ToString();
                            }
                            else
                            {
                                sid = "not found";
                            }
                            if (systemprinciple.LastLogon != null)
                            {
                                lastlogon = systemprinciple.LastLogon;
                            }
                            else
                            {
                                lastlogon = null;
                            }
                        }
                        //START OF CIM Collection
                        ADServer thisserver = new ADServer(samaccountname);
                        CIMData thisdata = LoadCIMData(systemname, thisserver); 
                        Console.WriteLine("\n" + systemname + " searching for SQL instances... ");
                        List<SQLService> instances = GetSQLServices(samaccountname);
                        List<SQLInstance> allinstances = new List<SQLInstance>();

                        //    int instanceid = 1;
                        string databaseinfo = string.Empty;
                        foreach (SQLService instance in instances)
                        {
                            string thisinstance = string.Empty;
                            string connectionstr = string.Empty;
                            if (instance.ServiceName != "MSSQLSERVER")
                            {
                                int index = instance.ServiceName.IndexOf('$');
                                //      string thisinstance = "\\" + selectedinstance.ServiceName.Substring(index + 1);
                                thisinstance = systemname + "\\" + instance.ServiceName.Substring(index + 1);
                            }
                            else
                            {
                                thisinstance = systemname;
                            }
                            SQLInstance loadedinstance = LoadSQLInfo(thisinstance);
                            allinstances.Add(loadedinstance);                           
                        }                        
                        using (var connection = new SqlConnection(connectdb))
                        {
                            try
                            {
                                connection.Open();
                                int serverid = -1;
                                int instanceid = -1;
                                int databaseid = -1;
                                try
                                {
                                    SqlCommand command = new SqlCommand();
                                    //   command.CommandText = @"INSERT INTO AllADServers (LocationID, Hostname, SamAccountName, ScanNotes, UpdateTimeStamp) VALUES ('3333', 'testhost', 'testaccount', 'testnotes', '" + now + "')";
                                    //                            command.CommandText = @"INSERT INTO AllADServers OUTPUT inserted.ServerID VALUES ('" + location + "', '" + samaccountname.TrimEnd('$') + "', '" + samaccountname + "', '', '" + now + "')";
                                    command.CommandText = @"INSERT INTO AllADServers OUTPUT inserted.ServerID VALUES (@location,@hostname,@samaccountname,@scannotes,@updatetimestamp)";
                                    command.Parameters.AddWithValue("@location", location);
                                    command.Parameters.AddWithValue("@hostname", systemname);
                                    command.Parameters.AddWithValue("@samaccountname", samaccountname);
                                    command.Parameters.AddWithValue("@scannotes", thisdata.Scannotes + " Scanned by: " + Environment.UserName.ToLower());
                                    command.Parameters.AddWithValue("@updatetimestamp", now);
                                    command.Connection = connection;
                                    serverid = (int)command.ExecuteScalar();
                                    //   EnterSQLData(serverid);
                                }
                                catch (SqlException ex)
                                {
                                    Console.Write("Cannot enter data into the database\n" + ex.ToString());
                                }
                                catch (Exception ex)
                                {
                                    Console.Write("Cannot enter data into the database\n" + ex.ToString());
                                }
                                try
                                {
                                    SqlCommand adcommand = new SqlCommand();
                                    adcommand.CommandText = @"INSERT INTO ADInfo VALUES (@serverid, @ou, @os,@servicepack, @description, @guid, @sid, 
                        @lastlogon, @badlogoncount, @disabled, @created,@changed,@logoncount)";
                                    //                adcommand.CommandText = @"INSERT INTO ADInfo VALUES ('" + serverid.ToString() + @"','" + ou +
                                    //                    @"','" + computerobject.OperatingSystem + @"','" + computerobject.OperatingSystemServicePack + @"','" + computerobject.Description +
                                    //                    @"','" + guid + "', '" + sid + @"','" + lastlogon.ToString() + @"','" + badlogoncount.ToString() + @"','" +
                                    //                    disabled.ToString() + @"','" + created.ToString() + @"','" + changed.ToString() + @"','" + logoncount.ToString() + @"')";

                                    adcommand.Parameters.AddWithValue("@serverid", serverid);
                                    adcommand.Parameters.AddWithValue("@ou", ou);
                                    adcommand.Parameters.AddWithValue("@os", computerobject.OperatingSystem);
                                    adcommand.Parameters.AddWithValue("@servicepack", computerobject.OperatingSystemServicePack);
                                    adcommand.Parameters.AddWithValue("@description", computerobject.Description);
                                    adcommand.Parameters.AddWithValue("@guid", guid);
                                    adcommand.Parameters.AddWithValue("@sid", sid);
                                    adcommand.Parameters.AddWithValue("@lastlogon", lastlogon);                                   
                                    adcommand.Parameters.AddWithValue("@badlogoncount", badlogoncount);
                                    adcommand.Parameters.AddWithValue("@disabled", disabled);
                                    adcommand.Parameters.AddWithValue("@created", created.ToString());
                                    adcommand.Parameters.AddWithValue("@changed", changed.ToString());
                                    adcommand.Parameters.AddWithValue("@logoncount", logoncount.ToString());
                                    adcommand.Connection = connection;
                                    adcommand.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    Console.Write("Exception on " + samaccountname + ":\n\n" + ex.ToString() + "\n\n");
                                }
                                //START OF WMI Data Insertion                              
                                if (thisdata.Scannotes.Equals("Permission to CIM Query"))
                                {
                                    Console.WriteLine("Performing INSERT for CIM data on " + systemname);
                                    try
                                    {
                                        SqlCommand oscommand = new SqlCommand();
                                        oscommand.CommandText = @"INSERT INTO Win32_OperatingSystem VALUES (@ServerID, @installdate, @lastbootuptime, @serialnumber, @status, @physicalmemory, @freephysicalmemory, @virtualmemory, @freevirtualmemory)";
                                        oscommand.Parameters.AddWithValue("@ServerID", serverid.ToString());
                                        oscommand.Parameters.AddWithValue("@installdate", thisdata.Server.InstallDate.ToString());
                                        oscommand.Parameters.AddWithValue("@lastbootuptime", thisdata.Server.LastBootUpTime.ToString());
                                        oscommand.Parameters.AddWithValue("@serialnumber", thisdata.Server.SerialNumber);
                                        oscommand.Parameters.AddWithValue("@status", thisdata.Server.Status);
                                        oscommand.Parameters.AddWithValue("@physicalmemory", thisdata.Server.PhysicalMemory);
                                        oscommand.Parameters.AddWithValue("@freephysicalmemory", thisdata.Server.FreePhysicalMemory);
                                        oscommand.Parameters.AddWithValue("@virtualmemory", thisdata.Server.VirtualMemory);
                                        oscommand.Parameters.AddWithValue("@freevirtualmemory", thisdata.Server.FreeVirtualMemory);
                                        oscommand.Connection = connection;
                                        oscommand.ExecuteNonQuery();
                                        oscommand.Dispose();
                                        SqlCommand cscommand = new SqlCommand();
                                        cscommand.CommandText = @"INSERT INTO Win32_ComputerSystem VALUES (@ServerID, @domain, @domainrole, @manufacturer ,@model, @processors, @logicalprocessors, @loggedonuser)";
                                        cscommand.Parameters.AddWithValue("@ServerID", serverid.ToString());
                                        cscommand.Parameters.AddWithValue("@domain", thisdata.Server.Domain);
                                        cscommand.Parameters.AddWithValue("@domainrole", thisdata.Server.DomainRole);
                                        cscommand.Parameters.AddWithValue("@manufacturer", thisdata.Server.Manufacturer);
                                        cscommand.Parameters.AddWithValue("@model", thisdata.Server.Model);
                                        cscommand.Parameters.AddWithValue("@processors", thisdata.Server.Processors);
                                        cscommand.Parameters.AddWithValue("@logicalprocessors", thisdata.Server.LogicalProcessors);
                                        cscommand.Parameters.AddWithValue("@loggedonuser", thisdata.Server.LoggedOnUser);
                                        cscommand.Connection = connection;
                                        cscommand.ExecuteNonQuery();
                                        cscommand.Dispose();
                                        foreach (NetworkInterface n in thisdata.Networkinterfacelist)
                                        {
                                            SqlCommand niccommand = new SqlCommand();
                                            //      niccommand.CommandText = @"INSERT INTO NIC VALUES ('" + serverid.ToString() + "','" + n.IPEnabled +
                                            //    "', '" + n.Ip + "', '" + n.Subnet + "', '" + n.Gateway +
                                            //      "','" + n.Macaddress + "', '" + n.Dhcpenabled + "', '" + n.Description + "')";

                                            niccommand.CommandText = @"INSERT INTO NIC VALUES (@ServerID, @ipenabled,@ip, @mask, @gateway ,@mac,@dhcpenabled,@description)";
                                            niccommand.Parameters.AddWithValue("@ServerID", serverid.ToString());
                                            niccommand.Parameters.AddWithValue("@ipenabled", n.IPEnabled);
                                            niccommand.Parameters.AddWithValue("@ip", n.Ip);
                                            niccommand.Parameters.AddWithValue("@mask", n.Subnet);
                                            niccommand.Parameters.AddWithValue("@gateway", n.Gateway);
                                            niccommand.Parameters.AddWithValue("@mac", n.Macaddress);
                                            niccommand.Parameters.AddWithValue("@dhcpenabled", n.Dhcpenabled);
                                            niccommand.Parameters.AddWithValue("@description", n.Description);
                                            niccommand.Connection = connection;
                                            niccommand.ExecuteNonQuery();
                                        }
                                        foreach (Drive d in thisdata.Drives)
                                        {
                                            SqlCommand dcommand = new SqlCommand();
                                            //      public Drive(string name, double size, double free, string drivetype, string label, string filesystem, string serialnumber)
                                            dcommand.CommandText = @"INSERT INTO Storage VALUES (@ServerID, @name,@size, @free, @drivetype , @label, @filesystem, @serialnumber)";
                                            dcommand.Parameters.AddWithValue("@ServerID", serverid.ToString());
                                            dcommand.Parameters.AddWithValue("@name", d.Name);
                                            dcommand.Parameters.AddWithValue("@size", d.Size.ToString());
                                            dcommand.Parameters.AddWithValue("@free", d.Free.ToString());
                                            dcommand.Parameters.AddWithValue("@drivetype", d.DriveType);
                                            dcommand.Parameters.AddWithValue("@label", d.Label);
                                            dcommand.Parameters.AddWithValue("@filesystem", d.FileSystem);
                                            dcommand.Parameters.AddWithValue("@serialnumber", d.SerialNumber);
                                            dcommand.Connection = connection;
                                            dcommand.ExecuteNonQuery();
                                        }                                    
                                        foreach (Patch patch in thisdata.Patches)
                                        {
                                            SqlCommand patchcommand = new SqlCommand();
                                            patchcommand.CommandText = @"INSERT INTO InstalledPatches VALUES (@ServerID, @caption, @description, @hotfixid, @installedby, @installedon)";
                                            patchcommand.Parameters.AddWithValue("@ServerID", serverid.ToString());
                                            patchcommand.Parameters.AddWithValue("@caption", patch.Caption);
                                            patchcommand.Parameters.AddWithValue("@description", patch.Description);
                                            patchcommand.Parameters.AddWithValue("@hotfixid", patch.HotfixID);
                                            patchcommand.Parameters.AddWithValue("@installedby", patch.InstalledBy);
                                            patchcommand.Parameters.AddWithValue("@installedon", patch.InstalledOn);
                                            patchcommand.Connection = connection;
                                            patchcommand.ExecuteNonQuery();
                                        }
                                        foreach (InstalledProgram program in thisdata.Programs)
                                        {
                                            SqlCommand programcommand = new SqlCommand();
                                            programcommand.CommandText = @"INSERT INTO InstalledPrograms VALUES (@ServerID, @name, @version, @installstate, @description, @identifyingnumber, @installdate, @installsource, @packagename, @vendor, @language)";
                                            programcommand.Parameters.AddWithValue("@ServerID", serverid.ToString());
                                            programcommand.Parameters.AddWithValue("@name", program.Name);
                                            programcommand.Parameters.AddWithValue("@version", program.Version);
                                            programcommand.Parameters.AddWithValue("@installstate", program.InstallState);
                                            programcommand.Parameters.AddWithValue("@description", program.Description);
                                            programcommand.Parameters.AddWithValue("@identifyingnumber", program.IdentifyingNumber);
                                            programcommand.Parameters.AddWithValue("@installdate", program.InstallDate);
                                            programcommand.Parameters.AddWithValue("@installsource", program.InstallSource);
                                            programcommand.Parameters.AddWithValue("@packagename", program.PackageName);
                                            programcommand.Parameters.AddWithValue("@vendor", program.Vendor);
                                            programcommand.Parameters.AddWithValue("@language", program.Language);
                                            programcommand.Connection = connection;
                                            programcommand.ExecuteNonQuery();
                                        }
                                        //ENTER SQL DATA
                                        if (allinstances.Count() == 1)
                                        {
                                            Console.WriteLine(systemname + " has " + allinstances.Count().ToString() + " running SQL instance.");
                                        }
                                        else
                                        {
                                            Console.WriteLine(systemname + " has " + allinstances.Count().ToString() + " running SQL instances.");
                                        }
                                        foreach (SQLInstance i in allinstances)
                                        {
                                            SqlCommand instancecommand = new SqlCommand();
                                            instancecommand.CommandText = @"INSERT INTO DBInstances OUTPUT inserted.InstanceID VALUES (@ServerID, @name, @edition, @productlevel, @type, @version)";
                                            if (i.Version == "ERROR")
                                            {
                                                Console.WriteLine(systemname + ": Could not query SQL instance " + i.Instancename);
                                                instancecommand.Parameters.AddWithValue("@ServerID", serverid.ToString());
                                                instancecommand.Parameters.AddWithValue("@name", i.Instancename);
                                                instancecommand.Parameters.AddWithValue("@edition", "PERMISSION DENIED");
                                                instancecommand.Parameters.AddWithValue("@productlevel", "PERMISSION DENIED");
                                                instancecommand.Parameters.AddWithValue("@type", "PERMISSION DENIED");
                                                instancecommand.Parameters.AddWithValue("@version", "PERMISSION DENIED");
                                                instancecommand.Connection = connection;
                                                instancecommand.ExecuteNonQuery();
                                            }
                                            else
                                            {
                                                Console.WriteLine(systemname + ": able to query SQL instance " + i.Instancename);
                                                instancecommand.Parameters.AddWithValue("@ServerID", serverid.ToString());
                                                instancecommand.Parameters.AddWithValue("@name", i.Instancename);
                                                instancecommand.Parameters.AddWithValue("@edition", i.Edition);
                                                instancecommand.Parameters.AddWithValue("@productlevel", i.Productlevel);
                                                instancecommand.Parameters.AddWithValue("@type", i.Type);
                                                instancecommand.Parameters.AddWithValue("@version", i.Version);
                                                instancecommand.Connection = connection;
                                                instanceid = (int)instancecommand.ExecuteScalar();

                                                SqlCommand memorycommand = new SqlCommand();
                                                memorycommand.CommandText = @"INSERT INTO DBInstanceMemory VALUES (@InstanceID, @min, @max, @inuse)";
                                                memorycommand.Parameters.AddWithValue("@InstanceID", instanceid);
                                                memorycommand.Parameters.AddWithValue("@min", i.memory.Min);
                                                memorycommand.Parameters.AddWithValue("@max", i.memory.Max);
                                                memorycommand.Parameters.AddWithValue("@inuse", i.memory.InUse);
                                                memorycommand.Connection = connection;
                                                memorycommand.ExecuteNonQuery();

                                                foreach (SQLInstanceSystemDatabase systemdb in i.systemdatabases)
                                                {
                                                    SqlCommand systemdbcommand = new SqlCommand();
                                                    systemdbcommand.CommandText = @"INSERT INTO DBInstanceFiles VALUES (@InstanceID, @name, @dbfile, @tlog)";
                                                    systemdbcommand.Parameters.AddWithValue("@InstanceID", instanceid);
                                                    systemdbcommand.Parameters.AddWithValue("@name", systemdb.DBName);
                                                    systemdbcommand.Parameters.AddWithValue("@dbfile", systemdb.MDF);
                                                    systemdbcommand.Parameters.AddWithValue("@tlog", systemdb.LDF);
                                                    systemdbcommand.Connection = connection;
                                                    systemdbcommand.ExecuteNonQuery();
                                                    //        Console.WriteLine(systemdb.DBName + "\nMDF: " + systemdb.MDF + "\nLDF: " + systemdb.LDF);
                                                }
                                                //     Console.WriteLine("Instance sysadmin permission: ");
                                                foreach (SQLPermission systempermission in i.permissions)
                                                {
                                                    SqlCommand systempermissioncommand = new SqlCommand();
                                                    systempermissioncommand.CommandText = @"INSERT INTO DBInstancePermissions VALUES (@InstanceID, @prole, @username)";
                                                    systempermissioncommand.Parameters.AddWithValue("@InstanceID", instanceid);
                                                    systempermissioncommand.Parameters.AddWithValue("@prole", systempermission.Role);
                                                    systempermissioncommand.Parameters.AddWithValue("@username", systempermission.Member);
                                                    systempermissioncommand.Connection = connection;
                                                    systempermissioncommand.ExecuteNonQuery();
                                                    //        Console.WriteLine("Role: " + systempermission.Role + "    Member: " + systempermission.Member);
                                                }
                                                //   Console.WriteLine("Database Backup History");
                                                foreach (SQLInstanceBackup b in i.backups)
                                                {
                                                    SqlCommand backupcommand = new SqlCommand();
                                                    backupcommand.CommandText = @"INSERT INTO DBInstanceBackups VALUES (@InstanceID, @dbname, @recoverymodel, @lastfull, @lastdiff, @lastlog, @lastlog2)";
                                                    backupcommand.Parameters.AddWithValue("@InstanceID", instanceid);
                                                    backupcommand.Parameters.AddWithValue("@dbname", b.DBName);
                                                    backupcommand.Parameters.AddWithValue("@recoverymodel", b.RecoveryModel);
                                                    backupcommand.Parameters.AddWithValue("@lastfull", b.LastFull);
                                                    backupcommand.Parameters.AddWithValue("@lastdiff", b.LastDiff);
                                                    backupcommand.Parameters.AddWithValue("@lastlog", b.LastLog);
                                                    backupcommand.Parameters.AddWithValue("@lastlog2", b.LastLog2);
                                                    backupcommand.Connection = connection;
                                                    backupcommand.ExecuteNonQuery();

                                                    //    Console.WriteLine(backupcount + ":" + b.DBName + " " +
                                                    //        "\nLast Recovery Model: " + b.RecoveryModel +
                                                    //        "\nLast Full: " + b.LastFull +
                                                    //        "\nLast Diff: " + b.LastDiff +
                                                    //        "\nLast Log: " + b.LastLog +
                                                    //         "\nLast Log2: " + b.LastLog2
                                                    //         );

                                                }
                                                foreach (SQLInstanceDatabase db in i.databases)
                                                {
                                                    SqlCommand dbcommand = new SqlCommand();
                                                    dbcommand.CommandText = @"INSERT INTO DBInstanceDatabases OUTPUT inserted.DatabaseID VALUES (@InstanceID, @instancedbid, @name, @createdate)";
                                                    dbcommand.Parameters.AddWithValue("@InstanceID", instanceid);
                                                    dbcommand.Parameters.AddWithValue("@instancedbid", db.DBID.ToString());
                                                    dbcommand.Parameters.AddWithValue("@name", db.DBName);
                                                    dbcommand.Parameters.AddWithValue("@createdate", db.CreateDate.ToString());
                                                    dbcommand.Connection = connection;
                                                    databaseid = (int)dbcommand.ExecuteScalar();

                                                    //   Console.WriteLine(db.DBName + "ID: " + db.DBID + " (" + db.CreateDate.ToString() + ")");
                                                    //   Console.WriteLine("Files: ");
                                                    foreach (SQLFile dbfile in db.dbfiles)
                                                    {
                                                        SqlCommand databasefilecommand = new SqlCommand();
                                                        databasefilecommand.CommandText = @"INSERT INTO DBFiles VALUES (@DatabaseId, @type, @fname, @fgroup, @location, @size, @used, @free, @perc, @growth)";
                                                        databasefilecommand.Parameters.AddWithValue("@DatabaseID", databaseid);
                                                        databasefilecommand.Parameters.AddWithValue("@type", dbfile.Type);
                                                        databasefilecommand.Parameters.AddWithValue("@fname", dbfile.Name);
                                                        databasefilecommand.Parameters.AddWithValue("@fgroup", dbfile.Group);
                                                        databasefilecommand.Parameters.AddWithValue("@location", dbfile.Location);
                                                        databasefilecommand.Parameters.AddWithValue("@size", dbfile.Size);
                                                        databasefilecommand.Parameters.AddWithValue("@used", dbfile.Used);
                                                        databasefilecommand.Parameters.AddWithValue("@free", dbfile.Free);
                                                        databasefilecommand.Parameters.AddWithValue("@perc", dbfile.Percent);
                                                        databasefilecommand.Parameters.AddWithValue("@growth", dbfile.Growth);
                                                        databasefilecommand.Connection = connection;
                                                        databasefilecommand.ExecuteNonQuery();
                                                        //       Console.WriteLine(dbfile.Name + "   Location: " + dbfile.Location);
                                                    }
                                                    //    Console.WriteLine("Permissions: ");
                                                    foreach (SQLPermission p in db.dbpermissions)
                                                    {
                                                        SqlCommand permissioncommand = new SqlCommand();
                                                        permissioncommand.CommandText = @"INSERT INTO DBPermissions VALUES (@DatabaseID, @prole, @username)";
                                                        permissioncommand.Parameters.AddWithValue("@DatabaseID", databaseid);
                                                        permissioncommand.Parameters.AddWithValue("@prole", p.Role);
                                                        permissioncommand.Parameters.AddWithValue("@username", p.Member);
                                                        permissioncommand.Connection = connection;
                                                        permissioncommand.ExecuteNonQuery();
                                                        //    Console.WriteLine("   Role: " + p.Role + "   Member: " + p.Member);
                                                    }
                                                    //  Console.WriteLine("");
                                                }
                                            }
                                        }
                                        
                                    }
                                    catch (SqlException ex)
                                    {
                                        //          Console.WriteLine(samaccountname.TrimEnd('$') + " Exception:Error, the parameterized query '(@ServerID nvarchar(5),@domain nvarchar(4000),@domainrole nvarch' expects the parameter '@domain', which was not supplied. ");
                                        Console.WriteLine(systemname + " Exception: " + ex.ToString() + "\n");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(systemname + " Exception: " + ex.ToString() + "\n");
                                    }
                                    Console.WriteLine(systemname + " System and SQL Query SUCCESS.  Data Entered");
                                }
                                else
                                {
                                    Console.WriteLine("CIM query error: " + systemname + "\nscannotes = " + thisdata.Scannotes);
                                }

                            }
                            catch (InvalidOperationException)
                            {
                                Console.WriteLine("InvalidOperationException.  Connection Pool Error.  All pooled connections in use or max pools sized reached. ");
                            }
                            catch (Exception exc)
                            {
                                Console.WriteLine("Exception:  Cannot connect to the Database.\n\n" + exc.ToString());
                            }
                            //    Task.WaitAll(sqlcollectiontasks);  
                            connection.Close();
                        }
    
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception:\n\n" + exc.ToString());
            }
        }
        private static SQLInstance LoadSQLInfo(string instance)
        {
            Console.WriteLine("Loading SQL Data from instance: " + instance);
            SQLInstance thisinstance;
            string connectionstr = "Data Source=" + instance + ";Integrated Security=True;";
            try
            {
                using (var connection = new SqlConnection(connectionstr))
                {
                    try
                    {
                        connection.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter = new SqlDataAdapter(versionquery, connection);
                        DataSet data = new DataSet();
                        adapter.Fill(data);
                        string thisedition = data.Tables[0].Rows[0].ItemArray[1].ToString();
                        string thisproductlevel = data.Tables[0].Rows[0].ItemArray[2].ToString();
                        string thistype = data.Tables[0].Rows[0].ItemArray[3].ToString();
                        string thisversion = data.Tables[0].Rows[0].ItemArray[4].ToString();
                        thisinstance = new SQLInstance(instance, thisedition, thisproductlevel, thistype, thisversion);
                        SqlDataAdapter memadapter = new SqlDataAdapter(memquery, connection);
                        if (memadapter != null)
                        {
                            DataSet memdata = new DataSet();
                            memadapter.Fill(memdata);
                            double memmin = 0;
                            double memmax = 0;
                            double meminuse = 0;
                            string description = string.Empty;
                            string isdynamic = string.Empty;
                            string isadvanced = string.Empty;
                            //       Console.Write(hostname + ":\nmin: " + memdata.Tables[0].Rows[0].ItemArray[2].ToString() + "\nmax: " + memdata.Tables[0].Rows[1].ItemArray[2].ToString() + "\nin use: " + memdata.Tables[0].Rows[1].ItemArray[5].ToString() + "\n");
                            memmin = Convert.ToInt32(memdata.Tables[0].Rows[0].ItemArray[2].ToString());
                            memmax = Convert.ToInt32(memdata.Tables[0].Rows[1].ItemArray[2].ToString());
                            meminuse = Convert.ToInt32(memdata.Tables[0].Rows[1].ItemArray[5].ToString());
                            thisinstance.memory.Min = Math.Round(memmin / 1024, 0, MidpointRounding.AwayFromZero);
                            thisinstance.memory.Max = Math.Round(memmax / 1024, 0, MidpointRounding.AwayFromZero);
                            thisinstance.memory.InUse = Math.Round(meminuse / 1024, 0, MidpointRounding.AwayFromZero);
                        }                  
                        SqlDataAdapter adminadapter = new SqlDataAdapter(sysadminrolemember, connection);
                        DataSet permissionsdata = new DataSet();
                        adminadapter.Fill(permissionsdata);
                        string serverrole;
                        string member;
                        int sysadminrow = 0;
                        foreach (DataRow row in permissionsdata.Tables[0].Rows)
                        {
                            serverrole = string.Empty;
                            member = string.Empty;
                            serverrole = permissionsdata.Tables[0].Rows[sysadminrow].ItemArray[0].ToString();
                            member = permissionsdata.Tables[0].Rows[sysadminrow].ItemArray[1].ToString();
                            SQLPermission thispermission = new SQLPermission(serverrole, member);
                            thisinstance.permissions.Add(thispermission);
                            sysadminrow++;
                        }
                        SqlDataAdapter backupadapter = new SqlDataAdapter(getdbbackups, connection);
                        DataSet backupfiledata = new DataSet();
                        backupadapter.Fill(backupfiledata);
                        string thisdatabasename = string.Empty;
                        string thisrecovermodel = string.Empty;
                        string thislastfull = string.Empty;
                        string thislastdiff = string.Empty;
                        string thislastlog = string.Empty;
                        string thislastlog2 = string.Empty;
                        int backuprowinfo = 0;
                        foreach (DataRow backuprow in backupfiledata.Tables[0].Rows)
                        {
                            thisdatabasename = string.Empty;
                            thisrecovermodel = string.Empty;
                            thislastfull = string.Empty;
                            thislastdiff = string.Empty;
                            thislastlog = string.Empty;
                            thislastlog2 = string.Empty;
                            thisdatabasename = backupfiledata.Tables[0].Rows[backuprowinfo].ItemArray[0].ToString();
                            thisrecovermodel = backupfiledata.Tables[0].Rows[backuprowinfo].ItemArray[1].ToString();
                            thislastfull = backupfiledata.Tables[0].Rows[backuprowinfo].ItemArray[2].ToString();
                            thislastdiff = backupfiledata.Tables[0].Rows[backuprowinfo].ItemArray[3].ToString();
                            thislastlog = backupfiledata.Tables[0].Rows[backuprowinfo].ItemArray[4].ToString();
                            thislastlog2 = backupfiledata.Tables[0].Rows[backuprowinfo].ItemArray[5].ToString();
                            SQLInstanceBackup thisbackup = new SQLInstanceBackup(thisdatabasename, thisrecovermodel, thislastfull, thislastdiff, thislastlog, thislastlog2);
                            thisinstance.backups.Add(thisbackup);
                            backuprowinfo++;                           
                        }
                        //get system database info                            

                        //  List<DBFile> files = new List<DBFile>();
                        SqlDataAdapter sysdbadapter = new SqlDataAdapter(getsystemdblocations, connection);
                        DataSet filedata = new DataSet();
                        sysdbadapter.Fill(filedata);
                        string dbname = string.Empty;
                        string mdf = string.Empty;
                        string ldf = string.Empty;
                        int sysdbrow = 0;
                        foreach (DataRow sysd in filedata.Tables[0].Rows)
                        {
                            dbname = string.Empty;
                            mdf = string.Empty;
                            ldf = string.Empty;
                            dbname = filedata.Tables[0].Rows[sysdbrow].ItemArray[0].ToString();
                            mdf = filedata.Tables[0].Rows[sysdbrow].ItemArray[1].ToString();
                            ldf = filedata.Tables[0].Rows[sysdbrow].ItemArray[2].ToString();
                            SQLInstanceSystemDatabase thisfile = new SQLInstanceSystemDatabase(dbname, mdf, ldf);
                       //     Console.WriteLine(dbname.ToString() + "MDF: " + thisfile.MDF + "LDF: " + thisfile.LDF);
                            thisinstance.systemdatabases.Add(thisfile);
                            sysdbrow++;
                        }
                        DataTable databases = connection.GetSchema("Databases");
                        List<DatabaseGrid> alldbs = new List<DatabaseGrid>();

                        foreach (DataRow database in databases.Rows)
                        {  
                           string databasename = database.Field<String>("database_name");
                            if (databasename != "master" && databasename != "model" && databasename != "msdb" && databasename != "tempdb" && databasename != null && databasename != string.Empty)
                            {
                                // Console.WriteLine(count + ": " + databasename);
                                short dbid = database.Field<short>("dbid");
                                DateTime creationdate = database.Field<DateTime>("create_date");
                                SQLInstanceDatabase thisdatabase = new SQLInstanceDatabase(databasename, dbid, creationdate);

                                string databaserolememberOLD = "USE [" + databasename + "]; SELECT user_name([memberuid]) as [Username], User_Name([groupuid]) as [Role_Name] FROM [sys].[sysmembers]";
                                //      databaserolemember = "USE [" + databasename + "]; SELECT user_name([memberuid]) as [Username], User_Name([groupuid]) as [Role_Name] FROM [sys].[sysmembers]";
                                //      string databaserolemember = "USE Patching SELECT user_name([memberuid]) as [Username], User_Name([groupuid]) as [Role_Name] FROM [sys].[sysmembers]";
                                string databaserolemember = "USE [" + databasename + @"];  SELECT DP1.name AS RoleName,   
   isnull (DP2.name, 'No members') AS UserName   
 FROM sys.database_role_members AS DRM  
 RIGHT OUTER JOIN sys.database_principals AS DP1  
   ON DRM.role_principal_id = DP1.principal_id  
 LEFT OUTER JOIN sys.database_principals AS DP2  
   ON DRM.member_principal_id = DP2.principal_id  
WHERE DP1.type = 'R'
ORDER BY DP1.name;";
                                //  Console.Write(databaserolemember);

                                SqlDataAdapter userpermissionadapter = new SqlDataAdapter(databaserolemember, connection);
                                DataSet userpermissiondata = new DataSet();
                                userpermissionadapter.Fill(userpermissiondata);

                                //   Console.WriteLine("\nChecking DB roles on DB:  " + databasename);
                                int permissionrow = 0;
                                foreach (DataRow userpermission in userpermissiondata.Tables[0].Rows)
                                {
                                    //     pstr = pstr + permissionsdata.Tables[0].Rows[i].ItemArray[j].ToString() + " ";
                                    string userserverrole = userpermission.ItemArray[0].ToString();
                                    string usermember = userpermission.ItemArray[1].ToString();
                                    //       string usermember = permissionsdata.Tables[0].Rows[permissionrow].ItemArray[0].ToString();
                                    //       string userserverrole = permissionsdata.Tables[0].Rows[permissionrow].ItemArray[1].ToString();
                                    SQLPermission thispermission = new SQLPermission(userserverrole, usermember);

                                    if (!usermember.Equals("No members"))
                                    {
                                        //       Console.WriteLine(permissionrow + ".  Member: " + usermember + "  Role: " + userserverrole);
                                        thisdatabase.dbpermissions.Add(thispermission);
                                    }
                                    permissionrow++;
                                }


                                string getuserdbfileinfo = "USE [" + databasename + "]" + @"SELECT
                                               [TYPE] = A.TYPE_DESC
                                              ,[FILE_Name] = A.name
                                              ,[FILEGROUP_NAME] = fg.name
                                              ,[File_Location] = A.PHYSICAL_NAME
                                              ,[FILESIZE_MB] = CONVERT(DECIMAL(10,2),A.SIZE/128.0)
                                              ,[USEDSPACE_MB] = CONVERT(DECIMAL(10,2),A.SIZE/128.0 - ((SIZE/128.0) - CAST(FILEPROPERTY(A.NAME, 'SPACEUSED') AS INT)/128.0))
                                              ,[FREESPACE_MB] = CONVERT(DECIMAL(10,2),A.SIZE/128.0 - CAST(FILEPROPERTY(A.NAME, 'SPACEUSED') AS INT)/128.0)
                                              ,[FREESPACE_%] = CONVERT(DECIMAL(10,2),((A.SIZE/128.0 - CAST(FILEPROPERTY(A.NAME, 'SPACEUSED') AS INT)/128.0)/(A.SIZE/128.0))*100)
                                              ,[AutoGrow] = 'By ' + CASE is_percent_growth WHEN 0 THEN CAST(growth/128 AS VARCHAR(10)) + ' MB -' 
                                                  WHEN 1 THEN CAST(growth AS VARCHAR(10)) + '% -' ELSE '' END 
                                                  + CASE max_size WHEN 0 THEN 'DISABLED' WHEN -1 THEN ' Unrestricted' 
                                                      ELSE ' Restricted to ' + CAST(max_size/(128*1024) AS VARCHAR(10)) + ' GB' END 
                                                  + CASE is_percent_growth WHEN 1 THEN ' [autogrowth by percent, BAD setting!]' ELSE '' END
                                          FROM sys.database_files A LEFT JOIN sys.filegroups fg ON A.data_space_id = fg.data_space_id 
                                          order by A.TYPE desc, A.NAME;";

                                SqlDataAdapter userdbfileadapter = new SqlDataAdapter(getuserdbfileinfo, connection);
                                DataSet userdbfiledata = new DataSet();
                                userdbfileadapter.Fill(userdbfiledata);
                                //         int userdbrowcount = filedata.Tables[0].Rows.Count;
                                //         int userdbcolumncount = filedata.Tables[0].Columns.Count;
                                string type = string.Empty;
                                string file = string.Empty;
                                string group = string.Empty;
                                string flocation = string.Empty;
                                string fsize = string.Empty;
                                string fused = string.Empty;
                                string ffree = string.Empty;
                                string fpercent = string.Empty;
                                string fgrowth = string.Empty;
                                int userfilerow = 0;                            
                                foreach (DataRow userrow in userdbfiledata.Tables[0].Rows)
                                {
                                    type = string.Empty;
                                    file = string.Empty;
                                    group = string.Empty;
                                    flocation = string.Empty;
                                    fsize = string.Empty;
                                    fused = string.Empty;
                                    ffree = string.Empty;
                                    fpercent = string.Empty;
                                    fgrowth = string.Empty;
                                    type = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[0].ToString();
                                    file = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[1].ToString();
                                    group = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[2].ToString();
                                    flocation = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[3].ToString();
                                    fsize = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[4].ToString();
                                    fused = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[5].ToString();
                                    ffree = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[6].ToString();
                                    fpercent = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[7].ToString();
                                    fgrowth = userdbfiledata.Tables[0].Rows[userfilerow].ItemArray[8].ToString();
                                    SQLFile thisfile = new SQLFile(type, file, group, flocation, fsize, fused, ffree, fpercent, fgrowth);
                                    thisdatabase.dbfiles.Add(thisfile);
                                    userfilerow++;
                                }                                   
                                thisinstance.databases.Add(thisdatabase);
                            }
                        }
                        connection.Close();
                        return thisinstance;
                    }
                    catch (SqlException)
                    {
                        Console.WriteLine(instance + "SQLException: Login failed for user " + System.Environment.UserName);
                        thisinstance = new SQLInstance(instance, "ERROR", "ERROR", "ERROR", "ERROR");
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(instance + "Exception: Login failed for user " + System.Environment.UserName);
                        thisinstance = new SQLInstance(instance, "ERROR", "ERROR", "ERROR", "ERROR");
                        Console.WriteLine(exc.ToString());
                    }
                    return thisinstance;
                }        
            }
            catch (Exception)
            {
                Console.WriteLine("Exception: Login failed for user " + System.Environment.UserName + " attempting to access instance " + instance );
                thisinstance = new SQLInstance(instance, "ERROR", "ERROR", "ERROR", "ERROR");
                return thisinstance;
            }
        }
        static private string LoadWin32_OperatingSystem(CimSession cimsession, ref ADServer systeminfo)
        {
            string scannotes = string.Empty;
            try
            {
                var stats = cimsession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem");
                foreach (CimInstance stat in stats)
                {
                    try
                    {
                        string tempdate = stat.CimInstanceProperties["InstallDate"].Value.ToString();
                        systeminfo.InstallDate = Convert.ToDateTime(tempdate);
                    }
                    catch (Exception)
                    {
                        systeminfo.InstallDate = null;
                    }
                    try
                    {
                        string tempdate = stat.CimInstanceProperties["LastBootUpTime"].Value.ToString();
                        if (tempdate != null && tempdate != string.Empty && tempdate != "")
                        {
                            systeminfo.LastBootUpTime = Convert.ToDateTime(tempdate);
                        }
                        else
                        {
                            systeminfo.LastBootUpTime = null;
                        }
                    }
                    catch (Exception)
                    {
                        systeminfo.LastBootUpTime = null;
                    }
                    try
                    {
                        systeminfo.SerialNumber = stat.CimInstanceProperties["SerialNumber"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.SerialNumber = " ";
                    }
                    try
                    {
                        systeminfo.Status = stat.CimInstanceProperties["Status"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Status = string.Empty;
                    }
                    try
                    {
                        systeminfo.PhysicalMemory = stat.CimInstanceProperties["TotalVisibleMemorySize"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.PhysicalMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.FreePhysicalMemory = stat.CimInstanceProperties["FreePhysicalMemory"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.FreePhysicalMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.VirtualMemory = stat.CimInstanceProperties["TotalVirtualMemorySize"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.VirtualMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.FreeVirtualMemory = stat.CimInstanceProperties["FreeVirtualMemory"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.FreeVirtualMemory = string.Empty;
                    }
                }
                scannotes = "Permission to CIM Query";
            }
            catch (ManagementException)
            {
             //   Console.WriteLine("ManagementException:\n\n " + exc.ToString());
                scannotes = "Management Exception: CIM issue or no admin rights to the system.";
            }
            catch (DirectoryServicesCOMException)
            {
            //    Console.WriteLine("DirectoryServicesCOMException:\n\n " + exc.ToString());
                scannotes = "DirectoryServicesCOMExceptionException:  The object may have been removed since the scan started.";
            }
            catch (NullReferenceException)
            {
            //    Console.WriteLine("NullReferenceException:\n\n " + exc.ToString());
                scannotes = "NullReferenceException.";
            }
            catch (COMException)
            {
            //    Console.WriteLine("COMException:\n\n " + exc.ToString());
                scannotes = "COMException: The host is not responding.";
            }
            catch (UnauthorizedAccessException)
            {
             //   Console.WriteLine("UnauthorizedAccessException:\n\n " + exc.ToString());
                scannotes = "UnauthorizedAccessException: No admin rights to this system.";
            }
            catch (Exception)
            {
             //   Console.WriteLine("Exception:\n\n " + exc.ToString());
                scannotes = "Exception: No DNS Entry or the host not a Windows system.";
            }
            return scannotes;
        }
        static private string LoadWin32_OperatingSystemCIM(string samaccountname, ADServer systeminfo)
        {
            string systemname = samaccountname.TrimEnd('$');
            string scannotes = "Permission to CIM Query";
            try
            {
                CimSession Session = CimSession.Create(systemname);
                var stats = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem");
                foreach (CimInstance stat in stats)
                {
                    try
                    {
                        string tempdate = stat.CimInstanceProperties["InstallDate"].Value.ToString();
                        systeminfo.InstallDate = Convert.ToDateTime(tempdate);
                    }
                    catch (Exception)
                    {
                        systeminfo.InstallDate = null;
                    }
                    try
                    {
                        string tempdate = stat.CimInstanceProperties["LastBootUpTime"].Value.ToString();
                        if (tempdate != null && tempdate != string.Empty && tempdate != "")
                        {
                            systeminfo.LastBootUpTime = Convert.ToDateTime(tempdate);
                        }
                        else
                        {
                            systeminfo.LastBootUpTime = null;
                        }
                    }
                    catch (Exception)
                    {
                        systeminfo.LastBootUpTime = null;
                    }
                    try
                    {
                        systeminfo.SerialNumber = stat.CimInstanceProperties["SerialNumber"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.SerialNumber = " ";
                    }
                    try
                    {
                        systeminfo.Status = stat.CimInstanceProperties["Status"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Status = string.Empty;
                    }
                    try
                    {
                        systeminfo.PhysicalMemory = stat.CimInstanceProperties["TotalVisibleMemorySize"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.PhysicalMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.FreePhysicalMemory = stat.CimInstanceProperties["FreePhysicalMemory"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.FreePhysicalMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.VirtualMemory = stat.CimInstanceProperties["TotalVirtualMemorySize"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.VirtualMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.FreeVirtualMemory = stat.CimInstanceProperties["FreeVirtualMemory"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.FreeVirtualMemory = string.Empty;
                    }
                }
            }
            catch (ManagementException exc)
            {
                Console.WriteLine("ManagementException:\n\n " + exc.ToString());
                scannotes = "Management Exception: CIM issue or no admin rights to the system.";
            }
            catch (DirectoryServicesCOMException exc)
            {
                Console.WriteLine("DirectoryServicesCOMException:\n\n " + exc.ToString());
                scannotes = "DirectoryServicesCOMExceptionException:  The object may have been removed since the scan started.";
            }
            catch (NullReferenceException exc)
            {
                Console.WriteLine("NullReferenceException:\n\n " + exc.ToString());
                scannotes = "NullReferenceException.";
            }
            catch (COMException exc)
            {
                Console.WriteLine("COMException:\n\n " + exc.ToString());
                scannotes = "COMException: The host is not responding.";
            }
            catch (UnauthorizedAccessException exc)
            {
                Console.WriteLine("UnauthorizedAccessException:\n\n " + exc.ToString());
                scannotes = "UnauthorizedAccessException: No admin rights to this system.";
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception:\n\n " + exc.ToString());
                scannotes = "Exception: No DNS Entry or the host not a Windows system.";
            }
            return scannotes;
        }
        static private string LoadWin32_OperatingSystemWMI(string samaccountname, ADServer systeminfo)
        {
            string scannotes = "Permission to WMI Query";
            ADInventory db = new ADInventory(connectdb);
            try
            {
                //START OF OS Win32_OperatingSystem Collector
                //Start of OS collector
                string fullpath = "\\\\" + samaccountname.TrimEnd('$') + "\\root\\cimv2";
                ManagementScope scope = new ManagementScope(fullpath);
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementObject m in queryCollection)
                {
                    try
                    {
                        string datestring = m["InstallDate"].ToString();
                        systeminfo.InstallDate = SetDate(datestring);
                    }
                    catch (Exception)
                    {
                        systeminfo.InstallDate = null;
                    }
                    try
                    {
                        string datestring = m["LastBootUpTime"].ToString();
                        systeminfo.LastBootUpTime = SetDate(datestring);
                    }
                    catch (Exception)
                    {
                        systeminfo.LastBootUpTime = null;
                    }
                    try
                    {
                        systeminfo.SerialNumber = m["SerialNumber"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.SerialNumber = " ";
                    }
                    try
                    {
                        systeminfo.Status = m["Status"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Status = string.Empty;
                    }
                    try
                    {
                        systeminfo.PhysicalMemory = m["TotalVisibleMemorySize"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.PhysicalMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.FreePhysicalMemory = m["FreePhysicalMemory"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.FreePhysicalMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.VirtualMemory = m["TotalVirtualMemorySize"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.VirtualMemory = string.Empty;
                    }
                    try
                    {
                        systeminfo.FreeVirtualMemory = m["FreeVirtualMemory"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.FreeVirtualMemory = string.Empty;
                    }
                }
            }
            catch (ManagementException)
            {
                scannotes = "Management Exception: WMI issue or no admin rights to the system.";
            }
            catch (DirectoryServicesCOMException)
            {
                scannotes = "DirectoryServicesCOMExceptionException:  The object may have been removed since the scan started.";
            }
            catch (NullReferenceException)
            {
                scannotes = "NullReferenceException.";
            }
            catch (COMException)
            {
                scannotes = "COMException: The host is not responding.";
            }
            catch (UnauthorizedAccessException)
            {
                scannotes = "UnauthorizedAccessException: No admin rights to this system.";
            }
            catch (Exception)
            {
                Console.WriteLine(samaccountname.TrimEnd('$'), " Error, can't WMI query Win32_OperatingSystem class ");
                scannotes = "Exception: No DNS Entry or the host not a Windows system.";
            }
            return scannotes;
        }
        static private void LoadWin32_ComputerSystem(CimSession cimsession, ref ADServer systeminfo)
        {
            var stats = cimsession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_ComputerSystem");
            try
            {
                foreach (CimInstance stat in stats)
                {
                    try
                    {
                        systeminfo.Domain = stat.CimInstanceProperties["Domain"].Value.ToString();
                    }
                    catch (Exception exc)
                    {
                        systeminfo.Domain = "root.sutterhealth.org";
                        Console.WriteLine(systeminfo.HostName + "Domain Exception:\n\n" + exc.ToString());
                    }
                    try
                    {
                        systeminfo.DomainRole = stat.CimInstanceProperties["DomainRole"].Value.ToString();
                        //MessageBox.Show(domainrole, domain);
                    }
                    catch (Exception exc)
                    {
                        systeminfo.DomainRole = string.Empty;
                        Console.WriteLine(systeminfo.HostName + "Domain Role Exception:\n\n" + exc.ToString());
                    }
                    try
                    {
                        systeminfo.Manufacturer = stat.CimInstanceProperties["Manufacturer"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Manufacturer = string.Empty;
                    }
                    try
                    {
                        systeminfo.Model = stat.CimInstanceProperties["Model"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Model = string.Empty;
                    }
                    try
                    {
                        systeminfo.Processors = stat.CimInstanceProperties["NumberOfLogicalProcessors"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Processors = string.Empty;
                    }
                    try
                    {
                        systeminfo.LogicalProcessors = stat.CimInstanceProperties["NumberOfProcessors"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.LogicalProcessors = string.Empty;
                    }
                    try
                    {
                        systeminfo.LoggedOnUser = stat.CimInstanceProperties["UserName"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.LoggedOnUser = string.Empty;
                    }
                    if (systeminfo.LoggedOnUser == string.Empty)
                    {
                        systeminfo.LoggedOnUser = "No Locally logged on user.";
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query Win32_ComputerSystem class on " + cimsession.ComputerName);
            }
        }
        static private void LoadWin32_ComputerSystemCIM(string samaccountname, ADServer systeminfo)
        {
            string systemname = samaccountname.TrimEnd('$');
            CimSession Session = CimSession.Create(systemname);
            var stats = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_ComputerSystem");
            try
            {
                foreach (CimInstance stat in stats)
                {
                    try
                    {
                        systeminfo.Domain = stat.CimInstanceProperties["Domain"].Value.ToString();
                    }
                    catch (Exception exc)
                    {
                        systeminfo.Domain = "root.sutterhealth.org";
                        Console.WriteLine(samaccountname.TrimEnd('$') + "Domain Exception:\n\n" + exc.ToString());
                    }
                    try
                    {
                        systeminfo.DomainRole = stat.CimInstanceProperties["DomainRole"].Value.ToString();
                        //MessageBox.Show(domainrole, domain);
                    }
                    catch (Exception exc)
                    {
                        systeminfo.DomainRole = string.Empty;
                        Console.WriteLine(samaccountname.TrimEnd('$') + "Domain Role Exception:\n\n" + exc.ToString());
                    }
                    try
                    {
                        systeminfo.Manufacturer = stat.CimInstanceProperties["Manufacturer"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Manufacturer = string.Empty;
                    }
                    try
                    {
                        systeminfo.Model = stat.CimInstanceProperties["Model"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Model = string.Empty;
                    }
                    try
                    {
                        systeminfo.Processors = stat.CimInstanceProperties["NumberOfLogicalProcessors"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Processors = string.Empty;
                    }
                    try
                    {
                        systeminfo.LogicalProcessors = stat.CimInstanceProperties["NumberOfProcessors"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.LogicalProcessors = string.Empty;
                    }
                    try
                    {
                        systeminfo.LoggedOnUser = stat.CimInstanceProperties["UserName"].Value.ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.LoggedOnUser = string.Empty;
                    }
                    if (systeminfo.LoggedOnUser == string.Empty)
                    {
                        systeminfo.LoggedOnUser = "No Locally logged on user.";
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query Win32_ComputerSystem class on " + systemname);
            }
        }
        static private void LoadWin32_ComputerSystemWMI(string samaccountname, ADServer systeminfo)
        {
            //    int x = 0;
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
            try
            {
                string fullpath = "\\\\" + samaccountname.TrimEnd('$') + "\\root\\cimv2";
                ManagementScope scope = new ManagementScope(fullpath);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementObject m in queryCollection)
                {
                    try
                    {
                        systeminfo.Domain = m["Domain"].ToString();
                    }
                    catch (Exception exc)
                    {
                        systeminfo.Domain = "root.sutterhealth.org";
                        Console.WriteLine(samaccountname.TrimEnd('$') + "Domain Exception:\n\n" + exc.ToString());
                    }
                    try
                    {
                        systeminfo.DomainRole = m["DomainRole"].ToString();
                    }
                    catch (Exception exc)
                    {
                        systeminfo.DomainRole = string.Empty;
                        Console.WriteLine(samaccountname.TrimEnd('$') + "Domain Role Exception:\n\n" + exc.ToString());
                    }
                    try
                    {
                        systeminfo.Manufacturer = m["Manufacturer"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Manufacturer = string.Empty;
                    }
                    try
                    {
                        systeminfo.Model = m["Model"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Model = string.Empty;
                    }
                    try
                    {
                        systeminfo.Processors = m["NumberOfProcessors"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.Processors = string.Empty;
                    }
                    try
                    {
                        systeminfo.LogicalProcessors = m["NumberOfLogicalProcessors"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.LogicalProcessors = string.Empty;
                    }
                    try
                    {
                        systeminfo.LoggedOnUser = m["UserName"].ToString();
                    }
                    catch (Exception)
                    {
                        systeminfo.LoggedOnUser = string.Empty;
                    }
                    if (systeminfo.LoggedOnUser == string.Empty)
                    {
                        systeminfo.LoggedOnUser = "No Locally logged on user.";
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine(samaccountname.TrimEnd('$'), " Error, can't WMI query Win32_ComputerSystem class ");
            }
        }
        static private List<NetworkInterface> LoadNICs(CimSession cimsession)
        {
            List<NetworkInterface> networkinterfacelist = new List<NetworkInterface>();
            var stats = cimsession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_NetworkAdapterConfiguration");
            try
            {
                string ipenabled;
                string dhcpenabled;
                string description;
                string ip;
                string subnetmask;
                string gateway;
                string macaddress;
                int count;
                foreach (CimInstance stat in stats)
                {
                    try
                    {
                        ipenabled = string.Empty;
                        dhcpenabled = string.Empty;
                        description = string.Empty;
                        ip = string.Empty;
                        subnetmask = string.Empty;
                        gateway = string.Empty;
                        macaddress = string.Empty;

                        ipenabled = stat.CimInstanceProperties["IPEnabled"].Value.ToString();
                        if (ipenabled == "True")
                        {
                            string[] temparray = (string[])stat.CimInstanceProperties["IPAddress"].Value;
                            if (temparray != null)
                            {
                                count = 0;
                                foreach (string i in temparray)
                                {
                                    if (count < 1)
                                    {
                                        ip = i.ToString();
                                    }
                                    count++;
                                }
                            }
                            temparray = (string[])stat.CimInstanceProperties["IPSubnet"].Value;
                            count = 0;
                            if (temparray != null)
                            {
                                foreach (string s in temparray)
                                {
                                    if (count < 1)
                                    {
                                        subnetmask = s.ToString();
                                    }
                                    count++;
                                }
                                if (subnetmask.Contains(",") || subnetmask.Contains(" "))
                                {
                                    int index = subnetmask.IndexOf(' ');
                                    subnetmask = subnetmask.Substring(0, index);
                                }
                            }
                            temparray = (string[])stat.CimInstanceProperties["DefaultIPGateway"].Value;
                            if (temparray != null)
                            {
                                count = 0;
                                foreach (string g in temparray)
                                {
                                    if (count < 1)
                                    {
                                        gateway = g.ToString();
                                    }
                                    count++;
                                }
                            }
                            macaddress = stat.CimInstanceProperties["MACAddress"].Value.ToString();
                            dhcpenabled = stat.CimInstanceProperties["DHCPEnabled"].Value.ToString();
                            description = stat.CimInstanceProperties["Description"].Value.ToString();
                            networkinterfacelist.Add(new NetworkInterface(ip, ipenabled, subnetmask, gateway, macaddress, dhcpenabled, description));
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.Write("Exception:\n\n" + exc.ToString());
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query installed NICS on " + cimsession.ComputerName);
            }
            return networkinterfacelist;
        }
        static private List<NetworkInterface> LoadCIM(string samaccountname)
        {
            List<NetworkInterface> networkinterfacelist = new List<NetworkInterface>();
            string systemname = samaccountname.TrimEnd('$');
            CimSession Session = CimSession.Create(systemname);
            var stats = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_NetworkAdapterConfiguration");
            try
            {
                string ipenabled;
                string dhcpenabled;
                string description;
                string ip;
                string subnetmask;
                string gateway;
                string macaddress;
                int count;
                foreach (CimInstance stat in stats)
                {
                    try
                    {
                        ipenabled = string.Empty;
                        dhcpenabled = string.Empty;
                        description = string.Empty;
                        ip = string.Empty;
                        subnetmask = string.Empty;
                        gateway = string.Empty;
                        macaddress = string.Empty;

                        ipenabled = stat.CimInstanceProperties["IPEnabled"].Value.ToString();
                        if (ipenabled == "True")
                        {
                            string[] temparray = (string[])stat.CimInstanceProperties["IPAddress"].Value;
                            if (temparray != null)
                            {
                                count = 0;
                                foreach (string i in temparray)
                                {
                                    if (count < 1)
                                    {
                                        ip = i.ToString();
                                    }
                                    count++;
                                }
                            }
                            temparray = (string[])stat.CimInstanceProperties["IPSubnet"].Value;
                            count = 0;
                            if (temparray != null)
                            {
                                foreach (string s in temparray)
                                {
                                    if (count < 1)
                                    {
                                        subnetmask = s.ToString();
                                    }
                                    count++;
                                }
                                if (subnetmask.Contains(",") || subnetmask.Contains(" "))
                                {
                                    int index = subnetmask.IndexOf(' ');
                                    subnetmask = subnetmask.Substring(0, index);
                                }
                            }
                            temparray = (string[])stat.CimInstanceProperties["DefaultIPGateway"].Value;
                            if (temparray != null)
                            {
                                count = 0;
                                foreach (string g in temparray)
                                {
                                    if (count < 1)
                                    {
                                        gateway = g.ToString();
                                    }
                                    count++;
                                }
                            }
                            macaddress = stat.CimInstanceProperties["MACAddress"].Value.ToString();
                            dhcpenabled = stat.CimInstanceProperties["DHCPEnabled"].Value.ToString();
                            description = stat.CimInstanceProperties["Description"].Value.ToString();
                            networkinterfacelist.Add(new NetworkInterface(ip, ipenabled, subnetmask, gateway, macaddress, dhcpenabled, description));
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.Write("Exception:\n\n" + exc.ToString());
                    }
                }                    
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query installed NICS on " + systemname);
            }
            return networkinterfacelist;
        }
        static private List<NetworkInterface> LoadNICsWMI(string samaccountname)
        {
            string fullpath = "\\\\" + samaccountname.TrimEnd('$') + "\\root\\cimv2";
            ManagementScope scope = new ManagementScope(fullpath);
            List<NetworkInterface> networkinterfacelist = new List<NetworkInterface>();

            //   ObjectQuery query = new ObjectQuery("SELECT *  FROM win32_Printer");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_NetworkAdapterConfiguration");
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection queryCollection = searcher.Get();

                //       ADInventory db = new ADInventory(connectdb);
                //       Table<NICTable> nics = db.NICTable;
                string ipenabled;
                string dhcpenabled;
                string description;
                string ip;
                string subnetmask;
                string gateway;
                string macaddress;
                int count;
                foreach (ManagementObject m in queryCollection)
                {
                    ipenabled = string.Empty;
                    dhcpenabled = string.Empty;
                    description = string.Empty;
                    ip = string.Empty;
                    subnetmask = string.Empty;
                    gateway = string.Empty;
                    macaddress = string.Empty;

                    ipenabled = m["IPEnabled"].ToString();
                    if (ipenabled == "True")
                    {
                        string[] temparray = (string[])m["IPAddress"];
                        if (temparray != null)
                        {
                            count = 0;
                            foreach (string i in temparray)
                            {
                                if (count < 1)
                                {
                                    ip = i.ToString();
                                }
                                count++;
                            }
                        }
                        temparray = (string[])m["IPSubnet"];
                        count = 0;
                        if (temparray != null)
                        {
                            foreach (string s in temparray)
                            {
                                if (count < 1)
                                {
                                    subnetmask = s.ToString();
                                }
                                count++;
                            }
                            if (subnetmask.Contains(",") || subnetmask.Contains(" "))
                            {
                                int index = subnetmask.IndexOf(' ');
                                subnetmask = subnetmask.Substring(0, index);
                            }
                        }
                        temparray = (string[])m["DefaultIPGateway"];
                        if (temparray != null)
                        {
                            count = 0;
                            foreach (string g in temparray)
                            {
                                if (count < 1)
                                {
                                    gateway = g.ToString();
                                }
                                count++;
                            }
                        }
                        macaddress = m["MACAddress"].ToString();
                        dhcpenabled = m["DHCPEnabled"].ToString();
                        description = m["Description"].ToString();
                        networkinterfacelist.Add(new NetworkInterface(ip, ipenabled, subnetmask, gateway, macaddress, dhcpenabled, description));
                    }
                }

                /* foreach (ManagementObject m in queryCollection)
                 {
                     ipenabled = string.Empty;
                     dhcpenabled = string.Empty;
                     description = string.Empty;
                     ip = string.Empty;
                     OSmask = string.Empty;
                     gateway = string.Empty;
                     macaddress = string.Empty;

                     ipenabled = m["IPEnabled"].ToString();
                     if (ipenabled == "True")
                     {
                         string[] temparray = (string[])m["IPAddress"];
                         if (temparray != null)
                         {
                             foreach (string i in temparray)
                             {
                                 ip = ip + " " + i.ToString();
                             }
                         }
                         temparray = (string[])m["IPSubnet"];
                         if (temparray != null)
                         {
                             foreach (string s in temparray)
                             {
                                 OSmask = OSmask + " " + s.ToString();
                             }
                         }
                         temparray = (string[])m["DefaultIPGateway"];
                         if (temparray != null)
                         {
                             foreach (string g in temparray)
                             {
                                 gateway = gateway + " " + g.ToString();
                             }
                         }
                         macaddress = m["MACAddress"].ToString();
                         dhcpenabled = m["DHCPEnabled"].ToString();
                         description = m["Description"].ToString();
                         networkinterfacelist.Add(new NIC(ip, ipenabled, OSmask, gateway, macaddress, dhcpenabled, description));
                     }
                 }
                 */
            }
            catch (Exception)
            {
                Console.WriteLine(samaccountname.TrimEnd('$'), " Error, can't WMI query installed NICs ");
            }
            return networkinterfacelist;
        }
        static List<Drive> LoadInstalledDisks(CimSession cimsession)
        {
            List<Drive> drives = new List<Drive>();

            string description, drivetype, driveletter, size, label, filesystem, serialnumber;
            double freespace, totalsize;
            //1GB = 1 Byte x 1024^3
            double gigabytes = 1073741824;

            List<NetworkInterface> networkinterfacelist = new List<NetworkInterface>();
            var stats = cimsession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Volume");
            try
            {
                foreach (CimInstance stat in stats)
                {
                    try
                    {
                        description = string.Empty;
                        drivetype = string.Empty;
                        driveletter = string.Empty;
                        size = string.Empty;
                        label = string.Empty;
                        serialnumber = string.Empty;
                        filesystem = string.Empty;
                        freespace = 0;
                        try
                        {
                            description = stat.CimInstanceProperties["Description"].Value.ToString();
                        }
                        catch (Exception)
                        {
                            description = null;
                        }
                        try
                        {
                            drivetype = stat.CimInstanceProperties["DriveType"].Value.ToString();
                        }
                        catch (Exception)
                        {
                            drivetype = null;
                        }
                        try
                        {
                            driveletter = stat.CimInstanceProperties["Caption"].Value.ToString();
                        }
                        catch (Exception)
                        {
                            driveletter = null;
                        }
                        if (description != "3 1/2 Inch Floppy Drive" && description != "CD-ROM Disc" && driveletter != "A:" && drivetype != "0" && drivetype != "2" && drivetype != "5")
                        {
                            try
                            {
                                size = stat.CimInstanceProperties["Capacity"].Value.ToString();
                            }
                            catch (Exception)
                            {
                                //        Console.Write("Size missing for drive " + driveletter + " on system " + hostname + "\nIgnoring and continuing with next drive" + "\n");
                                size = null;
                            }
                            string volumename = string.Empty;
                            try
                            {
                                volumename = stat.CimInstanceProperties["Name"].Value.ToString();
                            }
                            catch
                            {
                                volumename = null;
                            }

                            try
                            {
                                totalsize = Convert.ToDouble(stat.CimInstanceProperties["Capacity"].Value.ToString()) / gigabytes;
                                totalsize = Math.Round(totalsize, 2);
                            }
                            catch
                            {
                                totalsize = 0;
                            }
                            if (totalsize != 0)
                            {
                                freespace = 0;
                                try
                                {
                                    freespace = Convert.ToDouble(stat.CimInstanceProperties["FreeSpace"].Value.ToString()) / gigabytes;
                                    freespace = Math.Round(freespace, 2);
                                }
                                catch
                                {
                                    freespace = 0;
                                }

                                //      string description = string.Empty;
                                var tlabel = stat.CimInstanceProperties["Label"].Value;
                                if (tlabel != null)
                                {
                                    label = stat.CimInstanceProperties["Label"].Value.ToString();
                                }
                                else
                                {
                                    label = "";
                                }
                                filesystem = string.Empty;
                                try
                                {
                                    filesystem = stat.CimInstanceProperties["FileSystem"].Value.ToString();
                                }
                                catch
                                {
                                    filesystem = null;
                                }
                                serialnumber = string.Empty;
                                try
                                {
                                    serialnumber = stat.CimInstanceProperties["SerialNumber"].Value.ToString();
                                }
                                catch
                                {
                                    serialnumber = null;
                                }
                                Drive thisdrive = new Drive(driveletter, totalsize, freespace, drivetype, label, filesystem, serialnumber);
                                drives.Add(thisdrive);

                                //       Console.WriteLine("Added: " + driveletter);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.Write("Exception:\n\n" + exc.ToString());
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query installed drives on " + cimsession.ComputerName);
            }
            return drives;
        }
        static List<Drive> LoadInstalledDisksCIM(string samaccountname)
        {
            List<Drive> drives = new List<Drive>();

            string description, drivetype, driveletter, size, label, filesystem, serialnumber;
            double freespace, totalsize;
           //1GB = 1 Byte x 1024^3
            double gigabytes = 1073741824;
           
            List<NetworkInterface> networkinterfacelist = new List<NetworkInterface>();
            string systemname = samaccountname.TrimEnd('$');
            CimSession Session = CimSession.Create(systemname);
            var stats = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Volume");
            try
            {
                foreach (CimInstance stat in stats)
                {
                    try
                    {
                        description = string.Empty;
                        drivetype = string.Empty;
                        driveletter = string.Empty;
                        size = string.Empty;
                        label = string.Empty;
                        serialnumber = string.Empty;
                        filesystem = string.Empty;
                        freespace = 0;
                        try
                        {
                            description = stat.CimInstanceProperties["Description"].Value.ToString();
                        }
                        catch (Exception)
                        {
                            description = null;
                        }
                        try
                        {
                            drivetype = stat.CimInstanceProperties["DriveType"].Value.ToString();
                        }
                        catch (Exception)
                        {
                            drivetype = null;
                        }
                        try
                        {
                            driveletter = stat.CimInstanceProperties["Caption"].Value.ToString();
                        }
                        catch (Exception)
                        {
                            driveletter = null;
                        }
                        if (description != "3 1/2 Inch Floppy Drive" && description != "CD-ROM Disc" && driveletter != "A:" && drivetype != "0" && drivetype != "2" && drivetype != "5")
                        {
                            try
                            {
                                size = stat.CimInstanceProperties["Capacity"].Value.ToString();
                            }
                            catch (Exception)
                            {
                                //        Console.Write("Size missing for drive " + driveletter + " on system " + hostname + "\nIgnoring and continuing with next drive" + "\n");
                                size = null;
                            }
                            string volumename = string.Empty;
                            try
                            {
                                volumename = stat.CimInstanceProperties["Name"].Value.ToString();
                            }
                            catch
                            {
                                volumename = null;
                            }

                            try
                            {
                                totalsize = Convert.ToDouble(stat.CimInstanceProperties["Capacity"].Value.ToString()) / gigabytes;
                                totalsize = Math.Round(totalsize, 2);
                            }
                            catch
                            {
                                totalsize = 0;
                            }
                            if (totalsize != 0)
                            {
                                freespace = 0;
                                try
                                {
                                    freespace = Convert.ToDouble(stat.CimInstanceProperties["FreeSpace"].Value.ToString()) / gigabytes;
                                    freespace = Math.Round(freespace, 2);
                                }
                                catch
                                {
                                    freespace = 0;
                                }

                                //      string description = string.Empty;
                                var tlabel = stat.CimInstanceProperties["Label"].Value;
                                if (tlabel != null)
                                {
                                    label = stat.CimInstanceProperties["Label"].Value.ToString();
                                }
                                else
                                {
                                    label = "";
                                }
                                filesystem = string.Empty;
                                try
                                {
                                    filesystem = stat.CimInstanceProperties["FileSystem"].Value.ToString();
                                }
                                catch
                                {
                                    filesystem = null;
                                }
                                serialnumber = string.Empty;
                                try
                                {
                                    serialnumber = stat.CimInstanceProperties["SerialNumber"].Value.ToString();
                                }
                                catch
                                {
                                    serialnumber = null;
                                }
                                Drive thisdrive = new Drive(driveletter, totalsize, freespace, drivetype, label, filesystem, serialnumber);
                                drives.Add(thisdrive);

                         //       Console.WriteLine("Added: " + driveletter);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.Write("Exception:\n\n" + exc.ToString());
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query installed drives on " + systemname);
            }
            return drives;
        }
        static List<Drive> LoadInstalledDisksWMI(string samaccountname)
        {
            //  int t = 0;
            List<Drive> drives = new List<Drive>();
            string fullpath = "\\\\" + samaccountname.TrimEnd('$') + "\\root\\cimv2";
            try
            {
                ManagementScope scope = new ManagementScope(fullpath);
                //get server ID to pop into the storage table
                ObjectQuery storagequery = new ObjectQuery("SELECT * FROM Win32_LogicalDisk");
                ManagementObjectSearcher storagesearcher = new ManagementObjectSearcher(scope, storagequery);
                ManagementObjectCollection storagequerycollection = storagesearcher.Get();
                string description, drivetype, driveletter, size, label, filesystem, serialnumber;
                double totalsize, freespace;

                foreach (ManagementObject m in storagequerycollection)
                {
                    description = string.Empty;
                    drivetype = string.Empty;
                    driveletter = string.Empty;
                    size = string.Empty;
                    label = string.Empty;
                    serialnumber = string.Empty;
                    filesystem = string.Empty;
                    totalsize = 0;
                    freespace = 0;
                    try
                    {
                        description = m["Description"].ToString();
                    }
                    catch (Exception)
                    {
                        description = null;
                    }
                    try
                    {
                        drivetype = m["DriveType"].ToString();
                    }
                    catch (Exception)
                    {
                        drivetype = null;
                    }
                    try
                    {
                        driveletter = m["Name"].ToString();
                    }
                    catch (Exception)
                    {
                        driveletter = null;
                    }
                    if (description != "3 1/2 Inch Floppy Drive" && description != "CD-ROM Disc" && driveletter != "A:" && drivetype != "0" && drivetype != "2" && drivetype != "5")
                    {
                        try
                        {
                            size = m["Size"].ToString();
                        }
                        catch (Exception)
                        {
                            //        Console.Write("Size missing for drive " + driveletter + " on system " + hostname + "\nIgnoring and continuing with next drive" + "\n");
                            size = null;
                        }
                        string volumename = string.Empty;
                        try
                        {
                            volumename = m["VolumeName"].ToString();
                        }
                        catch
                        {
                            volumename = null;
                        }

                        try
                        {
                            totalsize = Convert.ToDouble(m["Size"]) / gigabytes;
                            totalsize = Math.Round(totalsize, 2);
                        }
                        catch
                        {
                            totalsize = 0;
                        }
                        if (totalsize != 0)
                        {
                            freespace = 0;
                            try
                            {
                                freespace = Convert.ToDouble(m["FreeSpace"]) / gigabytes;
                                freespace = Math.Round(freespace, 2);
                            }
                            catch
                            {
                                freespace = 0;
                            }

                            //      string description = string.Empty;
                            label = string.Empty;
                            try
                            {
                                label = m["VolumeName"].ToString();
                            }
                            catch
                            {
                                label = null;
                            }
                            filesystem = string.Empty;
                            try
                            {
                                filesystem = m["FileSystem"].ToString();
                            }
                            catch
                            {
                                filesystem = null;
                            }
                            serialnumber = string.Empty;
                            try
                            {
                                serialnumber = m["VolumeSerialNumber"].ToString();
                            }
                            catch
                            {
                                serialnumber = null;
                            }
                            Drive thisdrive = new Drive(driveletter, totalsize, freespace, drivetype, label, filesystem, serialnumber);
                            drives.Add(thisdrive);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine(samaccountname.TrimEnd('$'), " Error, can't WMI query installed drives ");
            }
            return drives;
        }
        static private List<Patch> LoadInstalledPatches(CimSession cimsession)
        {
            List<Patch> allpatches = new List<Patch>();
            string caption;
            string description;
            string hotfixid;
            string installedby;
            string datestring;
            DateTime? installedon;
            var stats = cimsession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_QuickFixEngineering");
            try
            {
                foreach (CimInstance stat in stats)
                {
                    caption = string.Empty;
                    description = string.Empty;
                    hotfixid = string.Empty; ;
                    installedby = string.Empty;
                    datestring = string.Empty;
                    installedon = null;
                    try
                    {
                        caption = stat.CimInstanceProperties["Caption"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        caption = "";
                    }
                    try
                    {
                        description = stat.CimInstanceProperties["Description"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        description = "";
                    }
                    try
                    {
                        hotfixid = stat.CimInstanceProperties["HotFixID"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        hotfixid = null;
                    }
                    try
                    {
                        installedby = stat.CimInstanceProperties["InstalledBy"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("Installed By here\n:" + exc.ToString());
                        installedby = "";
                    }
                    try
                    {
                        //   installedon = m["InstalledOn"].ToString();
                        datestring = stat.CimInstanceProperties["InstalledOn"].Value.ToString();
                        //       MessageBox.Show(datestring);
                        //      installedon = ConvertToDateTime(datestring);
                        installedon = Convert.ToDateTime(datestring);

                    }
                    catch (Exception)
                    {
                        //        Console.Write("Crash here\n:" + exc.ToString());
                        installedon = null;
                    }
                    if (caption != null)
                    {
                        Patch thispatch = new Patch(caption, description, hotfixid, installedon.ToString(), installedby);
                        allpatches.Add(thispatch);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query installed patches on " + cimsession.ComputerName);
            }
            return allpatches;
        }
        static private List<Patch> LoadInstalledPatchesCIM(string samaccountname)
        {

            List<Patch> allpatches = new List<Patch>();
            string caption;
            string description;
            string hotfixid;
            string installedby;
            string datestring;
            DateTime? installedon;
            string systemname = samaccountname.TrimEnd('$');
            CimSession Session = CimSession.Create(systemname);
            var stats = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_QuickFixEngineering");
            try
            {
                foreach (CimInstance stat in stats)
                {                   
                    caption = string.Empty;
                    description = string.Empty;
                    hotfixid = string.Empty; ;
                    installedby = string.Empty;
                    datestring = string.Empty;
                    installedon = null;
                    try
                    {
                        caption = stat.CimInstanceProperties["Caption"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        caption = "";
                    }
                    try
                    {
                        description = stat.CimInstanceProperties["Description"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        description = "";
                    }
                    try
                    {
                        hotfixid = stat.CimInstanceProperties["HotFixID"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        hotfixid = null;
                    }
                    try
                    {
                        installedby = stat.CimInstanceProperties["InstalledBy"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("Installed By here\n:" + exc.ToString());
                        installedby = "";
                    }
                    try
                    {
                        //   installedon = m["InstalledOn"].ToString();
                        datestring = stat.CimInstanceProperties["InstalledOn"].Value.ToString();
                        //       MessageBox.Show(datestring);
                        //      installedon = ConvertToDateTime(datestring);
                        installedon = Convert.ToDateTime(datestring);

                    }
                    catch (Exception)
                    {
                        //        Console.Write("Crash here\n:" + exc.ToString());
                        installedon = null;
                    }
                    if (caption != null)
                    {
                        Patch thispatch = new Patch(caption, description, hotfixid, installedon.ToString(), installedby);
                        allpatches.Add(thispatch);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query installed patches on " + systemname);
            }
            return allpatches;
        }
        static private List<Patch> LoadInstalledPatchesWMI(string samaccountname)
        {
            ADInventory db = new ADInventory(connectdb);
            List<Patch> allpatches = new List<Patch>();
            //get server ID to pop into the storage table
            string fullpath = "\\\\" + samaccountname.TrimEnd('$') + "\\root\\cimv2";
            try
            {
                ManagementScope scope = new ManagementScope(fullpath);
                ObjectQuery installquery = new ObjectQuery("SELECT * FROM Win32_QuickFixEngineering");
                ManagementObjectSearcher installsearcher = new ManagementObjectSearcher(scope, installquery);
                ManagementObjectCollection installquerycollection = installsearcher.Get();
                string caption;
                string description;
                string hotfixid;
                string installedby;
                string datestring;
                DateTime? installedon;
                foreach (ManagementObject m in installquerycollection)
                {
                    caption = string.Empty;
                    description = string.Empty;
                    hotfixid = string.Empty; ;
                    installedby = string.Empty;
                    datestring = string.Empty;
                    installedon = null;
                    try
                    {
                        caption = m["Caption"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        caption = null;
                    }
                    try
                    {
                        description = m["Description"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        description = null;
                    }
                    try
                    {
                        hotfixid = m["HotFixID"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        hotfixid = null;
                    }
                    try
                    {
                        installedby = m["InstalledBy"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("Installed By here\n:" + exc.ToString());
                        installedby = null;
                    }
                    try
                    {
                        //   installedon = m["InstalledOn"].ToString();
                        datestring = m["InstalledOn"].ToString();
                        //       MessageBox.Show(datestring);
                        //      installedon = ConvertToDateTime(datestring);
                        installedon = Convert.ToDateTime(datestring);

                    }
                    catch (Exception)
                    {
                        //        Console.Write("Crash here\n:" + exc.ToString());
                        installedon = null;
                    }
                    if (caption != null)
                    {
                        Patch thispatch = new Patch(caption, description, hotfixid, installedon.ToString(), installedby.ToString());
                        allpatches.Add(thispatch);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine(samaccountname.TrimEnd('$'), " Error, can't WMI query installed patches ");
            }
            return allpatches;
        }
        static private List<InstalledProgram> LoadInstalledPrograms(CimSession cimsession)
        {
            List<InstalledProgram> allprograms = new List<InstalledProgram>();
            string name;
            string version;
            DateTime? installdate;
            string installstate;
            string description;
            string identifyingnumber;
            string datestring;
            string installsource;
            string packagename;
            string vendor;
            string language;
            var stats = cimsession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Product");
            try
            {
                foreach (CimInstance stat in stats)
                {
                    name = string.Empty;
                    version = string.Empty;
                    installdate = null;
                    installstate = string.Empty;
                    description = string.Empty;
                    identifyingnumber = string.Empty;
                    datestring = string.Empty;
                    installsource = string.Empty;
                    packagename = string.Empty;
                    vendor = string.Empty;
                    language = string.Empty;
                    try
                    {
                        name = stat.CimInstanceProperties["Name"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        name = "";
                    }
                    try
                    {
                        version = stat.CimInstanceProperties["Version"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("version here\n:" + exc.ToString());
                        version = "";
                    }
                    try
                    {
                        var date = stat.CimInstanceProperties["InstallDate"].Value;
                        //  datestring = stat.CimInstanceProperties["InstallDate"].Value;
                        installdate = ConvertToDateTime(date.ToString());
                        //     Console.Write(datestring + "  " + installdate.ToString());
                    }
                    catch (Exception)
                    {
                        //      Console.Write("installdate\n:" + exc.ToString());
                        installdate = null;
                    }
                    try
                    {
                        installstate = stat.CimInstanceProperties["InstallState"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("installstate\n:" + exc.ToString());
                        installstate = "";
                    }
                    try
                    {
                        description = stat.CimInstanceProperties["Description"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //     Console.Write("description here\n:" + exc.ToString());
                        description = "";
                    }
                    try
                    {
                        identifyingnumber = stat.CimInstanceProperties["IdentifyingNumber"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("identifyingnumber here\n:" + exc.ToString());
                        identifyingnumber = "";
                    }
                    try
                    {
                        installsource = stat.CimInstanceProperties["InstallSource"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //         Console.Write("installsource here\n:" + exc.ToString());
                        installsource = "";
                    }
                    try
                    {
                        packagename = stat.CimInstanceProperties["PackageName"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //             Console.Write("packagename here\n:" + exc.ToString());
                        packagename = "";
                    }
                    try
                    {
                        vendor = stat.CimInstanceProperties["Vendor"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("vendor here\n:" + exc.ToString());
                        language = "";
                    }
                    try
                    {
                        language = stat.CimInstanceProperties["Language"].Value.ToString();
                        //      Console.Write(name);
                    }

                    catch (Exception)
                    {
                        //      Console.Write("language here\n:" + exc.ToString());
                        language = "";
                    }
                    InstalledProgram thisprogram = new InstalledProgram(name, version, installstate, description, identifyingnumber, installdate.ToString(), installsource, packagename, vendor, language);
                    allprograms.Add(thisprogram);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error, can't CIM query installed programs on " + cimsession.ComputerName);
            }
            return allprograms;
        }
        static private List<InstalledProgram> LoadInstalledProgramsCIM(string samaccountname)
        {
            List<InstalledProgram> allprograms = new List<InstalledProgram>();
            string name;
            string version;
            DateTime? installdate;
            string installstate;
            string description;
            string identifyingnumber;
            string datestring;
            string installsource;
            string packagename;
            string vendor;
            string language;
            string systemname = samaccountname.TrimEnd('$');
            CimSession Session = CimSession.Create(systemname);
            var stats = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Product");
            try
            {
                foreach (CimInstance stat in stats)
                {
                    name = string.Empty;
                    version = string.Empty;
                    installdate = null;
                    installstate = string.Empty;
                    description = string.Empty;
                    identifyingnumber = string.Empty;
                    datestring = string.Empty;
                    installsource = string.Empty;
                    packagename = string.Empty;
                    vendor = string.Empty;
                    language = string.Empty;
                    try
                    {
                        name = stat.CimInstanceProperties["Name"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        name = "";
                    }
                    try
                    {
                        version = stat.CimInstanceProperties["Version"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("version here\n:" + exc.ToString());
                        version = "";
                    }
                    try
                    {
                        var date = stat.CimInstanceProperties["InstallDate"].Value;
                      //  datestring = stat.CimInstanceProperties["InstallDate"].Value;
                        installdate = ConvertToDateTime(date.ToString());
                        //     Console.Write(datestring + "  " + installdate.ToString());
                    }
                    catch (Exception)
                    {
                        //      Console.Write("installdate\n:" + exc.ToString());
                        installdate = null;
                    }
                    try
                    {
                        installstate = stat.CimInstanceProperties["InstallState"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("installstate\n:" + exc.ToString());
                        installstate = "";
                    }
                    try
                    {
                        description = stat.CimInstanceProperties["Description"].Value.ToString(); 
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //     Console.Write("description here\n:" + exc.ToString());
                        description = "";
                    }
                    try
                    {
                        identifyingnumber = stat.CimInstanceProperties["IdentifyingNumber"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("identifyingnumber here\n:" + exc.ToString());
                        identifyingnumber = "";
                    }
                    try
                    {
                        installsource = stat.CimInstanceProperties["InstallSource"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //         Console.Write("installsource here\n:" + exc.ToString());
                        installsource = "";
                    }
                    try
                    {
                        packagename = stat.CimInstanceProperties["PackageName"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //             Console.Write("packagename here\n:" + exc.ToString());
                        packagename = "";
                    }
                    try
                    {
                        vendor = stat.CimInstanceProperties["Vendor"].Value.ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("vendor here\n:" + exc.ToString());
                        language = "";
                    }
                    try
                    {
                        language = stat.CimInstanceProperties["Language"].Value.ToString();
                        //      Console.Write(name);
                    }

                    catch (Exception)
                    {
                        //      Console.Write("language here\n:" + exc.ToString());
                        language = "";
                    }
                    InstalledProgram thisprogram = new InstalledProgram(name, version, installstate, description, identifyingnumber, installdate.ToString(), installsource, packagename, vendor, language);
                    allprograms.Add(thisprogram);
                }
            }
            catch (Exception)
            {
                 Console.WriteLine("Error, can't CIM query installed programs on " + systemname);
            }
            return allprograms;
        }
        static private List<InstalledProgram> LoadInstalledProgramsWMI(string samaccountname)
        {
            List<InstalledProgram> allprograms = new List<InstalledProgram>();
            string fullpath = "\\\\" + samaccountname.TrimEnd('$') + "\\root\\cimv2";
            try
            {
                ManagementScope scope = new ManagementScope(fullpath);
                ObjectQuery installquery = new ObjectQuery("SELECT * FROM Win32_Product");
                ManagementObjectSearcher installsearcher = new ManagementObjectSearcher(scope, installquery);
                ManagementObjectCollection installquerycollection = installsearcher.Get();
                string name;
                string version;
                DateTime? installdate;
                string installstate;
                string description;
                string identifyingnumber;
                string datestring;
                string installsource;
                string packagename;
                string vendor;
                string language;
                foreach (ManagementObject m in installquerycollection)
                {
                    name = string.Empty;
                    version = string.Empty;
                    installdate = null;
                    installstate = string.Empty;
                    description = string.Empty;
                    identifyingnumber = string.Empty;
                    datestring = string.Empty;
                    installsource = string.Empty;
                    packagename = string.Empty;
                    vendor = string.Empty;
                    language = string.Empty;
                    try
                    {
                        name = m["Name"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("name here\n:" + exc.ToString());
                        name = "";
                    }
                    try
                    {
                        version = m["Version"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("version here\n:" + exc.ToString());
                        version = "";
                    }
                    try
                    {
                        datestring = m["InstallDate"].ToString();
                        installdate = ConvertToDateTime(datestring);
                        //     Console.Write(datestring + "  " + installdate.ToString());
                    }
                    catch (Exception)
                    {
                        //      Console.Write("installdate\n:" + exc.ToString());
                        installdate = null;
                    }
                    try
                    {
                        installstate = m["InstallState"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("installstate\n:" + exc.ToString());
                        installstate = "";
                    }
                    try
                    {
                        description = m["Description"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //     Console.Write("description here\n:" + exc.ToString());
                        description = "";
                    }
                    try
                    {
                        identifyingnumber = m["IdentifyingNumber"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //       Console.Write("identifyingnumber here\n:" + exc.ToString());
                        identifyingnumber = "";
                    }
                    try
                    {
                        installsource = m["InstallSource"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //         Console.Write("installsource here\n:" + exc.ToString());
                        installsource = "";
                    }
                    try
                    {
                        packagename = m["PackageName"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //             Console.Write("packagename here\n:" + exc.ToString());
                        packagename = "";
                    }
                    try
                    {
                        vendor = m["Vendor"].ToString();
                        //      Console.Write(name);
                    }
                    catch (Exception)
                    {
                        //        Console.Write("vendor here\n:" + exc.ToString());
                        language = "";
                    }
                    try
                    {
                        language = m["Language"].ToString();
                        //      Console.Write(name);
                    }

                    catch (Exception)
                    {
                        //      Console.Write("language here\n:" + exc.ToString());
                        language = "";
                    }
                    InstalledProgram thisprogram = new InstalledProgram(name, version, installstate, description, identifyingnumber, installdate.ToString(), installsource, packagename, vendor, language);
                    allprograms.Add(thisprogram);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(samaccountname.TrimEnd('$'), " Error, can't WMI query installed programs");

            }
            return allprograms;
        }
        static private int? LoadLocation(string samaccountname)
        {
            ADInventory db = new ADInventory(connectdb);
            Table<Location> locations = db.Locations;
            int? location = -1;
            if (samaccountname.Length >= 2)
            {
                string sitecode = samaccountname.Substring(0, 2);
                location = (from l in locations where l.SiteCode == sitecode select l.LocationID).FirstOrDefault();
                if (location == null || location == 0)
                {
                    location = 0;
                }
            }
            else
            {
                location = 0;
            }
            return location;
        }
        private static void MakeSQLSysAdmin(string hostname, List<SQLService> instances)
        {
            foreach (SQLService inst in instances)
            {
                if (inst.Status.ToString() == "Stopped")
                {
                    //     databaseinfo = databaseinfo + inst.ServiceName + " service is stopped on " + hostname;
                    //      Console.WriteLine(hostname + " " + inst.ServiceName + " service is stopped ");
                    //log it
                }
                else
                {
                    string thisinstance = string.Empty;
                    string connectionstr = string.Empty;
                    if (inst.ServiceName != "MSSQLSERVER")
                    {
                        int index = inst.ServiceName.IndexOf('$');
                        //      string thisinstance = "\\" + selectedinstance.ServiceName.Substring(index + 1);
                        thisinstance = hostname + "\\" + inst.ServiceName.Substring(index + 1);
                    }
                    else
                    {
                        thisinstance = hostname;
                    }
                    connectionstr = "Data Source=" + thisinstance + ";Integrated Security=True;";
                    //  int thisinstanceid = 0;
                    //       Console.WriteLine(inst.ServiceName + " service is started on " + hostname + "\nAttemting to Add Sysadmin user using Connection string: " + connectionstr);

                    string tsql_makesysadmin = @"
CREATE LOGIN [" + SYSADMINACCOUNT + @"] FROM WINDOWS WITH DEFAULT_DATABASE=[master]
EXEC sp_addsrvrolemember '" + SYSADMINACCOUNT + "', 'sysadmin'";

                    using (var connection = new SqlConnection(connectionstr))
                    {
                        try
                        {
                            bool exist = false;
                            connection.Open();
                            try
                            {
                                SqlCommand command = new SqlCommand();
                                command.CommandText = tsql_makesysadmin;
                                command.Connection = connection;
                                //  connection.Open();
                                command.ExecuteNonQuery();
                                connection.Close();

                                //  int serverid = (int)command.ExecuteScalar();
                            }
                            catch (SqlException)
                            {
                                Console.WriteLine(hostname + " SQL sysadmin user add failed.  User " + SYSADMINACCOUNT + "  already exists as sysadmin or add permission denied.");
                                exist = true;
                                permissiondenied++;
                            }
                            catch (Exception exc)
                            {
                                Console.WriteLine("Exception:\n\n" + exc.ToString());
                            }
                            if (!exist)
                            {
                                Console.WriteLine(hostname + " SQL SYSADMIN ACCESS GRANTED " + inst.InstanceName + " for user. " + SYSADMINACCOUNT);
                                permissiongranted++;
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine(hostname + " SQL Connection failed.  Permission denied.");
                            permissiondenied++;
                        }
                    }
                }
            }
        }
        private static void QuerySQL()
        {
            string days = "";
            string hours = "";
            string minutes = "";
            string seconds = "";
            Stopwatch stopwatch = new Stopwatch();
            SearchResultCollection results = null;
            try
            {
                string searchpath = Properties.Settings.Default.ROOT + "/" + searchou;
                DirectoryEntry searchroot = new DirectoryEntry(searchpath);
                DirectorySearcher mySearcher = new DirectorySearcher(searchroot);
                int totalobjects = 0;
                try
                {
                    mySearcher.Filter = ldapquery;
                    mySearcher.PageSize = 3000000;
                    results = mySearcher.FindAll();
                    totalobjects = results.Count;
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error:\n\n" + exc.ToString());
                }
                if (totalobjects > 0)
                {
                    Console.WriteLine("There are " + results.Count.ToString() + " server objects to scan in OU:\n" + searchpath);
                    Logger log = new Logger("Started Scan on OU: " + searchou, Environment.UserName, Properties.Settings.Default.ApplicationName + " v" + Properties.Settings.Default.version);
                    log.AlertAdmin();
                    stopwatch.Start();

                    Task[] tasks = new Task[results.Count];
                    int i = 0;
                    foreach (SearchResult result in results)
                    {
                        string samaccountname = result.GetDirectoryEntry().Properties["sAMAccountName"].Value.ToString();
                        Console.WriteLine(samaccountname);
                        if (samaccountname != null && samaccountname != string.Empty)
                        {
                            Console.WriteLine("Scanning SQL Instances in new thread: " + samaccountname.TrimEnd('$'));
                            //     GatherSQLInstances(samaccountname);
                            //     tasks[i] = Task.Factory.StartNew(() => GatherSQLInstances(samaccountname));
                        }
                        i++;
                    }
                    Task.WaitAll(tasks);
                    //results.Dispose();
                    stopwatch.Stop();
                    TimeSpan span = stopwatch.Elapsed;
                    days = span.Days.ToString();
                    hours = span.Hours.ToString();
                    minutes = span.Minutes.ToString();
                    seconds = span.Seconds.ToString();
                    Console.WriteLine("\n" + DISPLAYLINE + "Sending Result To: " + TOEMAIL + " and CC: " + CCEMAIL);
                    Console.WriteLine("\nAll Sysadmin Permissions for ou " + searchou + ":\n" + DISPLAYLINE + "\n\n" + syspermstring);
                    Console.Write(DISPLAYLINE + "All " + results.Count.ToString() + " systems  were scanned using: " + searchpath + "\nThe results have been entered into the database.\nLDAP Filter: " + ldapquery + "\nThe process took: " + days + " days " + hours + " hours " + minutes + " minutes " + seconds + " seconds\nItems Scannned: " + results.Count + "\n" + DISPLAYLINE);

                    syspermstring = String.Format(@"" + DISPLAYLINE + "\n\n" + syspermstring + "\n\n" + DISPLAYLINE);
                    //         "Started Scan on OU: " + searchou, Environment.UserName, Properties.Settings.Default.ApplicationName + " v" + Properties.Settings.Default.version);

                    log.SetMessage = "Ended Scan on OU: " + searchou + "\nThe process took: " + days + " days " + hours + " hours " + minutes + " minutes " + seconds + " seconds\nItems Scannned: " + results.Count + "\n\nCopy and Paste CSV Results: " + syspermstring;
                    log.AlertAdmin();
                    Thread.Sleep(1000);

                }
            }
            catch (DirectoryServicesCOMException exc)
            {
                Console.WriteLine("Error with OU:" + exc.ToString());
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception\n\n" + exc.ToString());

            }
        }
        static private DateTime? ConvertToDateTime(string value)
        {
            DateTime? converteddate;
            try
            {
                //    converteddate = Convert.ToDateTime(value);
                converteddate = DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
                //     MessageBox.Show(value + " converts to " + converteddate + " time. " + converteddate.Kind.ToString());
                return converteddate;
            }
            catch (FormatException)
            {
                //       MessageBox.Show(exc.ToString());
                return null;
            }

        }
        static private DateTime? SetDate(string strDate)
        {
            int year = Convert.ToInt32(strDate.Substring(0, 4));
            int month = Convert.ToInt32(strDate.Substring(4, 2));
            int day = Convert.ToInt32(strDate.Substring(6, 2));
            int hour = Convert.ToInt32(strDate.Substring(8, 2));
            int minute = Convert.ToInt32(strDate.Substring(10, 2));
            int second = Convert.ToInt32(strDate.Substring(12, 2));
            DateTime DT = new DateTime(year, month, day, hour, minute, second);
            return DT;
        }
        static string DISPLAYLINE = "\n---------------------------------------------------------------------------------------\n";
        static string versionquery = @"SELECT 
          SERVERPROPERTY('InstanceName') as Instance,
          SERVERPROPERTY('Edition') as Edition, /*shows 32 bit or 64 bit*/
          SERVERPROPERTY('ProductLevel') as ProductLevel, /* RTM or SP1 etc*/
          Case SERVERPROPERTY('IsClustered') when 1 then 'CLUSTERED' else
      'STANDALONE' end as ServerType,
          @@VERSION as VersionNumber";
        static string memquery = "SELECT * FROM sys.configurations where name like '%server memory%'";
        static string sysadminrolemember = "EXEC sp_helpsrvrolemember 'sysadmin'";
        static string getdbbackups = @"SELECT
                      DISTINCT
                            a.Name AS DatabaseName ,
                            CONVERT(SYSNAME, DATABASEPROPERTYEX(a.name, 'Recovery')) RecoveryModel ,
                            COALESCE((SELECT   CONVERT(VARCHAR(20), MAX(backup_finish_date), 100)
                                       FROM     msdb.dbo.backupset
                                       WHERE    database_name = a.name
                                                AND type = 'D'
                                                AND is_copy_only = '0'
                                     ), 'No Full') AS 'Full' ,
                            COALESCE((SELECT   CONVERT(VARCHAR(20), MAX(backup_finish_date), 100)
                                       FROM     msdb.dbo.backupset
                                       WHERE    database_name = a.name
                                                AND type = 'I'
                                                AND is_copy_only = '0'
                                     ), 'No Diff') AS 'Diff' ,
                            COALESCE((SELECT   CONVERT(VARCHAR(20), MAX(backup_finish_date), 100)
                                       FROM     msdb.dbo.backupset
                                       WHERE    database_name = a.name
                                                AND type = 'L'
                                     ), 'No Log') AS 'LastLog' ,
                            COALESCE((SELECT   CONVERT(VARCHAR(20), backup_finish_date, 100)
                                       FROM(SELECT    ROW_NUMBER() OVER(ORDER BY backup_finish_date DESC) AS 'rownum',
                                                            backup_finish_date
                                                  FROM      msdb.dbo.backupset
                                                  WHERE     database_name = a.name
                                                            AND type = 'L'
                                                ) withrownum
                                       WHERE    rownum = 2
                                     ), 'No Log') AS 'LastLog2'
                    FROM sys.databases a
                            LEFT OUTER JOIN msdb.dbo.backupset b ON b.database_name = a.name
                    WHERE a.name <> 'tempdb'
                            AND a.state_desc = 'online'
                    GROUP BY a.Name ,
                            a.compatibility_level
                    ORDER BY a.name";
        static string getsystemdblocations = @"select 
                        d.name as 'database',
                        mdf.physical_name as 'mdf_file',
                        ldf.physical_name as 'log_file'
                    from sys.databases d
                    inner join sys.master_files mdf on 
                        d.database_id = mdf.database_id and mdf.[type] = 0
                    inner join sys.master_files ldf on 
                        d.database_id = ldf.database_id and ldf.[type] = 1
                           where d.name  = 'master' OR d.name  = 'model' OR d.name  = 'msdb' OR d.name  = 'tempdb'";



    }
}