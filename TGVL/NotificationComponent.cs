﻿using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TGVL.Hubs;

namespace TGVL
{
    public class NotificationComponent
    {
        public void RegisterNotification(DateTime currentTime, string UserName)
        {
            string conStr = ConfigurationManager.ConnectionStrings["MyIdentityConnection"].ConnectionString;

            string sqlCommand = @"SELECT [dbo].[Notifications].[Id], [ReplyId], [UserId], [dbo].[Users].[UserName], [CreatedDate]";
            sqlCommand += "FROM [dbo].[Notifications]";
            sqlCommand += "Join [dbo].[Users]";
            sqlCommand += "On [dbo].[Notifications].[UserId] = [dbo].[Users].[Id]";
            sqlCommand += "WHERE[CreatedDate] > @CreatedDate AND[UserName] = @UserName";

            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand(sqlCommand, con);
                cmd.Parameters.AddWithValue("@CreatedDate", currentTime);
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
                 

                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                //Gửi notification cho customer khi có người reply
                //Cách 1: gửi cho group có groupName = UserName đã đăng kí
                //notificationHub.Clients.Group(UserName).notify("added"); 

                //Cách 2: gửi trực tiếp cho user có name = UserName đã đăng kí
                notificationHub.Clients.User(UserName).notify("added");


                //re-register notification
                RegisterNotification(DateTime.Now, UserName);

            }
        }
    }
}