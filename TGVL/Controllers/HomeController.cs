using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;

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
            //var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;
            //NotificationComponent NC = new NotificationComponent();
            //var list = NC.GetReplies(notificationRegisterTime).ToList();
            var userId = User.Identity.GetUserId<int>();

            //Hiển thị trên top
            var list2 = db.Notifications
                .Where(r => r.UserId == userId)
                .Take(10)
                
                .Select(r => new {
                    ReplyId = r.ReplyId,
                    RequestId = r.RequestId,
                    CreatedDate = r.CreatedDate,
                    Supplier = r.Reply.User.UserName,
                    Message = r.Message,
                    IsSeen = r.IsSeen
                })
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            //update session here for get only new added contacts (notification)
            //Session["LastUpdate"] = DateTime.Now;
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

        public ActionResult CheckOut()
        {
            return View();
        }

        public ActionResult ShoppingCart()
        {
            var cartSession = (CartViewModel)Session["Cart"];
            if (cartSession != null)
            {
                var model = new ShoppingCart();
                model.ShoppingCartProducts = new List<ShoppingCartProducts>();
                decimal total = 0;

                foreach (var c in cartSession.CartDetails)
                {
                    if (c.Type == "deal")
                    {
                        var deal = db.Deals.Find(c.DealId);

                        var scProducts = new ShoppingCartProducts
                        {
                            DealId = deal.Id,
                            Type = "deal",
                            Image = deal.Product.Image,
                            ProductName = deal.Product.SysProduct.Name,
                            Quantity = c.Quantity,
                            UnitPrice = deal.UnitPrice,
                            UnitType = deal.Product.SysProduct.UnitType.Type,
                            MiniTotal = deal.UnitPrice * c.Quantity
                        };
                        model.ShoppingCartProducts.Add(scProducts);
                        total += scProducts.MiniTotal;
                    } else
                    {
                        //normal
                    }          
                }
                model.Total = total;
                return View(model);
            }
            return View();
        }

        public JsonResult RemoveFromCart(string type, int id)
        {
            var cartSession = (CartViewModel)Session["Cart"];

            if (cartSession != null)
            {
                if (type == "deal")
                {
                    var check = cartSession.CartDetails.Where(c => c.DealId == id).FirstOrDefault();
                    if (check != null)
                    {
                        cartSession.CartDetails.Remove(check);
                    }
                } else
                {
                    var check = cartSession.CartDetails.Where(c => c.ProductId == id).FirstOrDefault();
                    if (check != null)
                    {
                        cartSession.CartDetails.Remove(check);
                    }
                }
                
               
                Session["Cart"] = cartSession; //save vao session

                return new JsonResult
                {
                    Data = new
                    {
                        Success = "Success",
                        RemoveElement = type + "_"+ id
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = "Fail"
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

    }
}