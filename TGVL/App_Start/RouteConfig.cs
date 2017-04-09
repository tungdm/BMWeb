using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TGVL.App_Start;

namespace TGVL
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.Add("RequestDetails", new SeoFriendlyRoute(
                "Request/Details/{id}",
                new RouteValueDictionary(new { controller = "Request", action = "Details" }),
                new MvcRouteHandler()
                )
            );

            routes.Add("ViewDetails", new SeoFriendlyRoute(
                "Home/ViewDetail/{id}",
                new RouteValueDictionary(new { controller = "Home", action = "ViewDetail" }),
                new MvcRouteHandler()
                )
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "TGVL.Areas.Admin.Controllers" }
            );

           
        }
    }
}
