using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TGVL.Controllers
{
    public class HomeController : Controller
    {
        private BMWEntities db = new BMWEntities();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public JsonResult GetNotificationReplies()
        {
            var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;
            NotificationComponent NC = new NotificationComponent();
            //var list = NC.GetReplies(notificationRegisterTime).ToList();
            var userId = User.Identity.GetUserId<int>();

            var list2 = db.Notifications
                .Where(r => r.CreatedDate > notificationRegisterTime && r.UserId == userId)
                .Take(10)
                
                .Select(r => new {
                    ReplyId = r.ReplyId,
                    CreatedDate = r.CreatedDate,
                    Supplier = r.Reply.User.UserName,
                    RequestId = r.Reply.RequestId,
                })
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            //update session here for get only new added contacts (notification)
            Session["LastUpdate"] = DateTime.Now;
            return new JsonResult { Data = list2, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page. Nguyen - Cho";

            return View();
        }
        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult SearchResult()
        {
            return View();
        }

        public ActionResult ViewDetail()
        {
            return View();
        }

        public ActionResult SearchRequestResult()
        {
            return View();
        }

        public ActionResult SearchShopResult()
        {
            return View();
        }
        
    }
}