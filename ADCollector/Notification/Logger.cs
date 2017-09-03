using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Management;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Threading;
using System.Windows;
using System.Security.AccessControl;
using System.Text;
using System.ComponentModel;
using System.Net.Mail;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Configuration;
using System.Data.SqlClient;

namespace ADCollector
{
    public class Logger
    {
        private string message;
        private string subject;
        private string user;
        private string app;
        public static string SMTPSERVER = Properties.Settings.Default.SMTPSERVER;
        public string FROMEMAIL = Properties.Settings.Default.fromemail;
        public string ALERTEMAIL = Properties.Settings.Default.toemail;
        private string CCALERTEMAIL;
        private string BCCALERTEMAIL;

        public string GetMessage
        {
            get
            {
                return message;
            }
        }
        public string SetMessage
        {
            set
            {
                message = value;
            }
        }
        public string GetUser
        {
            get
            {
                return user;
            }
        }
        public string App
        {
            get
            {
                return app;
            }
        }
        public Logger(string msg, string username, string applicationname)
        {
            message = msg;
            subject = string.Empty;
            user = username;
            app = applicationname;
            ALERTEMAIL = Properties.Settings.Default.alertemail;
            CCALERTEMAIL = string.Empty;
            BCCALERTEMAIL = string.Empty;
        }
        public Logger(string msg, string sub, string username, string applicationname, string emailaddress)
        {
            message = msg;
            subject = sub;
            user = username;
            app = applicationname;
            ALERTEMAIL = emailaddress;
            CCALERTEMAIL = string.Empty;
            BCCALERTEMAIL = string.Empty;
        }
        public Logger(string msg, string sub, string username, string applicationname, string emailaddress, string ccemailaddress)
        {
            message = msg;
            subject = sub;
            user = username;
            app = applicationname;
            ALERTEMAIL = emailaddress;
            CCALERTEMAIL = ccemailaddress;
            BCCALERTEMAIL = string.Empty;
        }
        public Logger(string msg, string sub, string username, string applicationname, string emailaddress, string ccemailaddress, string bccemailaddress)
        {
            message = msg;
            subject = sub;
            user = username;
            app = applicationname;
            ALERTEMAIL = emailaddress;
            CCALERTEMAIL = ccemailaddress;
            BCCALERTEMAIL = bccemailaddress;
        }
        public AllActions GetAllActions()
        {
            AllActions allactivities = new AllActions();
            SQLHelper sqlhelper = new SQLHelper();
            //   string connectionstring = @"Data Source=DCPWDBS447\SHIS;Initial Catalog=ADInventoryProd;User ID=shbascripts;Password=$cr1pt$upp0rt;Persist Security Info=True;Connection Timeout=3600;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=10000;Pooling=true";
        

            string connectionstring = sqlhelper.GetUtilizationConnectionString();
            Utilization db = new Utilization(connectionstring);
            Table<ActionTable> actions = db.Actions;

            var query = from action in actions orderby action.UpdateTimeStamp select action;
            foreach (var activity in query)
            {
                Action thisaction = new Action(activity.Username, activity.DisplayName, activity.Telephone, activity.ManagerName, activity.EmailAddress,
                    activity.Department, activity.Activity, activity.Tool, activity.UpdateTimeStamp);
                allactivities.Add(thisaction);
            }
            return allactivities;
        }
        public AllActions SearchActivities(string searchstring)
        {
            AllActions allactivities = new AllActions();
            SQLHelper sqlhelper = new SQLHelper();
            string connectionstring = sqlhelper.GetUtilizationConnectionString();
            Utilization db = new Utilization(connectionstring);
            Table<ActionTable> actions = db.Actions;
            searchstring = searchstring.ToLower();

            var query = from action in actions
                        where (action.Username.ToLower().Contains(searchstring) || action.Activity.ToLower().Contains(searchstring) ||
 action.DisplayName.ToLower().Contains(searchstring) || action.Department.ToLower().Contains(searchstring) ||
 action.Username.ToLower().Contains(searchstring) || action.Telephone.ToLower().Contains(searchstring) ||
 action.ManagerName.ToLower().Contains(searchstring) || action.Tool.ToLower().Contains(searchstring) ||
 action.UpdateTimeStamp.ToString().Contains(searchstring))
                        orderby action.Tool
                        select action;
            //     var query = from action in actions where action.Username.ToLower().Contains(searchstring.ToLower()) orderby action.Tool select action;           
            foreach (var activity in query)
            {
                Action thisaction = new Action(activity.Username, activity.DisplayName, activity.Telephone,
                    activity.ManagerName, activity.EmailAddress, activity.Department, activity.Activity,
                    activity.Tool, activity.UpdateTimeStamp);
                allactivities.Add(thisaction);
            }
            return allactivities;
        }
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                //      MessageBox.Show("Send Cancelled:\n" + token);
                //       Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                //       MessageBox.Show("Send Error: \n" + e.Error.ToString());
                //      Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                // MessageBox.Show("Message sent.");
            }
            //    mailsent = true;
        }
        public void AlertAdmin()
        {
            //    mailsent = false;
            MailMessage msg = new MailMessage();
            msg.To.Add(ALERTEMAIL);
            if (CCALERTEMAIL != string.Empty)
            {
                msg.CC.Add(CCALERTEMAIL);
            }
            if (BCCALERTEMAIL != string.Empty)
            {
                msg.Bcc.Add(BCCALERTEMAIL);
            }
            //msg.From = new MailAddress(Properties.Settings.Default.AlertEmail);
            msg.From = new MailAddress(FROMEMAIL, "Application Activity Alerts");
            if (subject == string.Empty)
            {
                try
                {
                    msg.Subject = app + " Activity" + " by " + user;
                }
                catch
                {
                    Console.WriteLine("Error: Invalid Subject");
                }

            }
            else
            {
                msg.Subject = subject;
            }
            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            msg.Body = "User: " + user + "\nApplication: " + app + "\nAction: " + message;
            msg.IsBodyHtml = false;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            //  msg.Body = msg.Body.Replace(Environment.NewLine, "<br>");

            SmtpClient smtpclient = new SmtpClient(SMTPSERVER);
            //      SmtpClient smtpclient = new SmtpClient(Properties.Settings.Default.PCRSMTPServer);

            smtpclient.Credentials = new System.Net.NetworkCredential("SUTTER-CHS\\SHBASCRIPTS", "$cr1pt$upp0rt");
            smtpclient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            string userstate = "message";
            try
            {
                smtpclient.SendAsync(msg, userstate);
            }
            catch (FormatException)
            {
                Console.WriteLine("ERROR incorrect email format specified.\nALERT EMAIL NOT SENT!");

            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR Sending EMAIL:\n\n" + exc.ToString());
            }

        }
        public void UpdateActivity()
        {
            SQLHelper sqlhelper = new SQLHelper();
            string connectionstring = sqlhelper.GetUtilizationConnectionString();
            Utilization db = new Utilization(connectionstring);
            Table<ActionTable> actions = db.Actions;
            ActiveDirectoryHelper adhelper = new ActiveDirectoryHelper();

            DirectoryEntry entry = new DirectoryEntry();
            ADUserDetail thisuser = adhelper.GetUserByLoginName(user);
            string displayname = thisuser.DisplayName;
            string telephone = thisuser.telePhone;
            string managername = thisuser.ManagerName;
            string emailaddress = thisuser.EmailAddress;
            string department = thisuser.Department;

            ActionTable thisaction = new ActionTable
            {
                Username = user,
                DisplayName = displayname,
                Telephone = telephone,
                ManagerName = managername,
                EmailAddress = emailaddress,
                Department = department,
                Activity = message,
                Tool = app,
                UpdateTimeStamp = DateTime.Now
            };
            try
            {
                db.Actions.InsertOnSubmit(thisaction);
                db.SubmitChanges();
            }
            catch (SqlException exc)
            {
                Console.Write("SqlException: " + exc.ToString());
            }
            catch (Exception exc)
            {
                Console.Write("Exception: " + exc.ToString());
            }
        }
    }
}
