using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TGVL.Controllers
{
    public class RequestController : Controller
    {
        // GET: Request
        public ActionResult CreateRequest()
        {
            return View();
        }
        
        public ActionResult BidRequest()
        {
            return View();
        }

        public ActionResult NormalRequest()
        {
            return View();
        }

        public ActionResult ViewRequest()
        {
            return View();
        }

        public ActionResult ViewBidRequestDetail()
        {
            return View();
        }

        public ActionResult ViewBidFloor()
        {
            return View();
        }

        public ActionResult ViewNormalFloor()
        {
            return View();
        }
    }
}