using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TGVL.Models;

namespace TGVL
{
    public class MvcApplication : System.Web.HttpApplication
    {
        string con = ConfigurationManager.ConnectionStrings["MyIdentityConnection"].ConnectionString;
        private BMWEntities db = new BMWEntities();
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            SqlDependency.Start(con);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            //NotificationComponent NC = new NotificationComponent();
            RequestComponent RC = new RequestComponent();

            //var currentTime = DateTime.Now;
            //HttpContext.Current.Session["LastUpdated"] = currentTime;
            if (Request.IsAuthenticated)
            {
                string username = User.Identity.Name;
                int userId = User.Identity.GetUserId<int>();
                //NC.RegisterNotification(currentTime, username);

                RC.RegisterRequestNotification(username);
              
                var numOfUnseen = db.Notifications.Where(n => n.UserId == userId && n.IsSeen == false).Count();

                HttpContext.Current.Session["UnSeenNoti"] = numOfUnseen;

            }
        }


        protected void Application_End()
        {
            SqlDependency.Stop(con);
        }

    }
}
