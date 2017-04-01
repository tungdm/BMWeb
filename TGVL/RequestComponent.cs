using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TGVL.Hubs;

namespace TGVL
{
    public class RequestComponent
    {
        public void RegisterRequestNotification(string UserName)
        {
            string conStr = ConfigurationManager.ConnectionStrings["MyIdentityConnection"].ConnectionString;

            string sqlCommand = @"SELECT [dbo].[Requests].[Id], [dbo].[Users].[UserName], [dbo].[Requests].[Flag], [dbo].[Requests].[Expired] ";
            sqlCommand += "FROM [dbo].[Requests] ";
            sqlCommand += "Join [dbo].[Users] ";
            sqlCommand += "On [dbo].[Requests].[CustomerId] = [dbo].[Users].[Id] ";
            sqlCommand += "WHERE [dbo].[Users].[UserName] = @UserName ";
            sqlCommand += "AND [dbo].[Requests].[Flag] = 1";
            sqlCommand += "AND [dbo].[Requests].[Expired] = 1";

            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand(sqlCommand, con);

                cmd.Parameters.AddWithValue("@UserName", UserName);

                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                cmd.Notification = null;
                SqlDependency sqlDep = new SqlDependency(cmd);

                sqlDep.OnChange += new OnChangeEventHandler((sender, e) => sqlDep_Onchange(sender, e, UserName));

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                }
            }
        }
        void sqlDep_Onchange(object sender, SqlNotificationEventArgs e, string UserName)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                //SqlDependency sqlDep = sender as SqlDependency;
                //sqlDep.OnChange -= sqlDep_Onchange;
                //sqlDep.OnChange -= sqlDep_Onchange(sender, e, UserName);

                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                notificationHub.Clients.User(UserName).notify("expiredOutside");
                //RegisterRequestNotification(UserName);
            }
        }
    }
    }