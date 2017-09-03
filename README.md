# ADCollector
Active Directory Computer Object and MS SQL Server Inventory 


Console app for doing a CIM and SQL inventory of AD computer objects.   All data is stored in an SQL database.  The inventory can be scheduled to run so that you can compare computer and SQL state over time.  


1.  LDAP search for ObjectCategory=computer and operatingSystem= query. The Operating System can be and Windows name.  IE Server* would be all servers.  Windows 7* would be all desktops, etc. 

2.  CIM query each result of the LDAP query in a seperate thread 

3.  SQL query each system a MSSQLSERVER or LOCALMACHINE\Instance name is found as a running service.  

4.  Store resulting CIM and SQL query data in SQL tables for later audit and review (GUI WPF/XAML interface as well as ASP.net interfaces are complete I just need to publish the code.

For data presentation a WPF/XAML GUI interface along with ASP.net AngularJS code exists as well.  

I will probably add this as a different project here shortly.

Example runs:


-All Servers.

ldapquery = "(&(ObjectCategory=computer)(operatingSystem=*Server*))"; 

ADCollector.exe "OU=Coastal Region,DC=MYDOMAIN,DC=ORG" "Servers"  


-All desktops. 

ldapquery = "(|(&(ObjectCategory=computer)(operatingSystem=Windows 7*))(operatingSystem=Windows XP*))"; 

ADCollector.exe "OU=Coastal Region,DC=MYDOMAIN,DC=ORG" "Desktops"  


-All computer objects

ldapquery = "(ObjectCategory=computer)";

ADCollector.exe "OU=Coastal Region,DC=MYDOMAIN,DC=ORG" "All"
