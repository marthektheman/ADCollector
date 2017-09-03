# ADCollector
Active Directory Computer Object and MS SQL Server Inventory 

Console app for doing a CIM and SQL inventory of AD computer objects.   All data is stored in an SQL database.  The inventory can be scheduled to run so that you can compare computer and SQL state over time.  
1.  LDAP search2.  CIM query system in thread 3.  SQL query system in thread4.  Store all data in SQL tables.
#For data presentation a WPF/XAML GUI interface along with ASP.net AngularJS code exists as well.  #I will probably add this as a different project here shortly.
Example runs:
#All Servers.
#ldapquery = "(&(ObjectCategory=computer)(operatingSystem=*Server*))"; 
ADCollector.exe "OU=Coastal Region,DC=MYDOMAIN,DC=ORG" "Servers"  

#All desktops.    
#ldapquery = "(|(&(ObjectCategory=computer)(operatingSystem=Windows 7*))(operatingSystem=Windows XP*))"; 
ADCollector.exe "OU=Coastal Region,DC=MYDOMAIN,DC=ORG" "Desktops"  

#All computer objects
#ldapquery = "(ObjectCategory=computer)";
ADCollector.exe "OU=Coastal Region,DC=MYDOMAIN,DC=ORG" "All"
