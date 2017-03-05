using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace TGVL.Hubs
{
    public class NotificationHub : Hub
    {
        
        public Task JoinGroup(string groupName)
        {
            return Groups.Add(Context.ConnectionId, groupName);
        }

        public Task LeaveGroup(string groupName)
        {
            return Groups.Remove(Context.ConnectionId, groupName);
        }

        public Task Send(string userId)
        {
            return Clients.User(userId).notify("test");
        }

        //Gửi noti cho customer khi có người reply
        public Task SendNotiReplyTo(string userId)
        {
            return Clients.User(userId).notify("test");
        }

        
    }
}