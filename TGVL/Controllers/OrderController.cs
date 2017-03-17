using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;

namespace TGVL.Controllers
{
    public class OrderController : Controller
    {
        private BMWEntities db = new BMWEntities();
        private ApplicationUserManager _userManager;

        public OrderController()
        {
        }

        public OrderController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Create(OrderViewModel model)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            var orderSh = (OrderViewModel)Session["Order"];
            

            if (orderSh != null) //mua lẻ hoặc mua deal
            {
                var list = orderSh.ShoppingCart.ShoppingCartProducts
                    .Select(s => new {
                        SupplierId = s.SupplierId
                    })
                    .GroupBy(s => s.SupplierId).ToList();
                foreach (var s in list)
                {
                    var newOrder = new Order();

                    decimal total = 0;
                    foreach (var o in orderSh.ShoppingCart.ShoppingCartProducts.Where(scp => scp.SupplierId == s.First().SupplierId))
                    {
                        total += o.UnitPrice * o.Quantity;
                    }
                    newOrder.CustomerId = user.Id;
                    newOrder.SupplierId = s.First().SupplierId;
                    newOrder.PaymentId = model.PaymentType;
                    newOrder.Total = total;
                    newOrder.DeliveryAddress = user.Address;
                    newOrder.CreateDate = DateTime.Now;
                    newOrder.Description = model.Description;
                    newOrder.Flag = 0;
                    newOrder.StatusId = 1; //mới đặt

                    db.Orders.Add(newOrder);

                    

                    //Create order detail
                    foreach (var o in orderSh.ShoppingCart.ShoppingCartProducts.Where(scp => scp.SupplierId == newOrder.SupplierId))
                    {
                        var oderDetails = new OrderDetail();
                        total += o.UnitPrice * o.Quantity;

                        if (o.Type == "deal") //mua deal
                        {
                            var deal = db.Deals.Find(o.DealId);

                            oderDetails.OrderId = newOrder.Id;

                            oderDetails.DealId = o.DealId;
                            oderDetails.Quantity = o.Quantity;
                            oderDetails.UnitPrice = o.UnitPrice;
                            oderDetails.Flag = 3; //order deatail cho deal
                        }
                        db.OrderDetails.Add(oderDetails);
                        //else if (o.Type == "normal") //mua lẻ
                        //{

                        //}

                    }
                    db.SaveChanges();
                }

                

            } else //normal request hoặc bid request
            {

            }


            Session.Clear();

            //var callbackUrl = Url.Action("Details", "MyOrders", null, protocol: Request.Url.Scheme);
            //await UserManager.SendEmailAsync(user.Id, "Đặt hàng thành công", "Xem chi tiết tại <a href=\"" + callbackUrl + "\">đây nè :)</a>");
            return RedirectToAction("Index","Home");
        }
    }
}