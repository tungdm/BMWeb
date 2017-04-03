using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TGVL.Hubs;
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
            var userId = User.Identity.GetUserId<int>();
            IEnumerable<Order> listOrders = db.Orders.Where(o => o.CustomerId == userId).ToList();
            ViewBag.ListOrders = listOrders;
            
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Create(OrderViewModel model, int replyId)
        {
            var userId = User.Identity.GetUserId<int>();
            var user = await UserManager.FindByIdAsync(userId);
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
                    newOrder.Flag = 4;  //order detail chung cho deal va mua le
                    newOrder.StatusId = 1; //mới đặt
                    newOrder.Discount = 0;

                    db.Orders.Add(newOrder);
                    db.SaveChanges();

                    //Create order detail
                    foreach (var o in orderSh.ShoppingCart.ShoppingCartProducts.Where(scp => scp.SupplierId == newOrder.SupplierId))
                    {
                        var orderDetails = new OrderDetail();
                        total += o.UnitPrice * o.Quantity;

                        //SignalR
                        var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                        if (o.Type == "deal") //mua deal
                        {
                            var deal = db.Deals.Find(o.DealId);
                            deal.NumBuyer += 1;
                            deal.Quantity -= o.Quantity;
                            db.Entry(deal).State = EntityState.Modified;
                            if (deal.Quantity == 0)
                            {
                                deal.Expired = true;
                            }
                            //Call all client update reply table
                            notificationHub.Clients.All.broadcastMessageDeal("updatedealquantity", o.DealId, deal.Quantity, deal.NumBuyer);   //noti all client

                            orderDetails.OrderId = newOrder.Id;
                            orderDetails.DealId = o.DealId;
                            orderDetails.Quantity = o.Quantity;
                            orderDetails.UnitPrice = o.UnitPrice;
                            orderDetails.Flag = 3; //order detail cho deal
                        }
                        else //mua lẻ
                        {
                            orderDetails.OrderId = newOrder.Id;
                            orderDetails.ProductId = o.ProductId;
                            orderDetails.Quantity = o.Quantity;
                            orderDetails.UnitPrice = o.UnitPrice;
                            orderDetails.Flag = 2; //order detail cho mua le
                        }
                        db.OrderDetails.Add(orderDetails);

                    }
                    
                }

            } else //normal request hoặc bid request
            {
                var reply = db.Replies.Find(replyId);
                reply.Selected = true;
                db.Entry(reply).State = EntityState.Modified;

                var request = db.Requests.Find(reply.RequestId);
                request.Completed = true;
                db.Entry(request).State = EntityState.Modified;

                var newOrder = new Order
                {
                    CustomerId = user.Id,
                    SupplierId = reply.SupplierId,
                    PaymentId = reply.Request.PaymentId,
                    Total = reply.Total,
                    DeliveryDate = reply.DeliveryDate,
                    DeliveryAddress = user.Address,
                    CreateDate = DateTime.Now,
                    Flag = reply.Flag, // 0 <=> normal request, 1 <=> bid request
                    Description = reply.Request.Descriptions,
                    Discount = reply.Discount,
                    StatusId = 1 //mới đặt
                };
                db.Orders.Add(newOrder);

                //Create order detail

                foreach (var repProduct in reply.ReplyProducts)
                {
                    var oderDetails = new OrderDetail {
                        OrderId = newOrder.Id,
                        ProductId = repProduct.ProductId,
                        Quantity = repProduct.Quantity,
                        UnitPrice = repProduct.UnitPrice,
                        Flag = reply.Flag
                    };
                    db.OrderDetails.Add(oderDetails);
                }

                var message = request.User.Fullname + " đã chọn bạn làm nhà cung cấp cho yêu cầu \"" + request.Title + "\" của họ.";
                //Create noti
                var noti = new Notification
                {
                    ReplyId = reply.Id,
                    RequestId = request.Id,
                    UserId = reply.SupplierId,
                    CreatedDate = DateTime.Now,
                    Message = message,
                    IsSeen = false,
                    IsClicked = false
                };
                db.Notifications.Add(noti);

                //SignalR
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                var supplier = UserManager.FindById(reply.SupplierId).UserName;

                notificationHub.Clients.User(supplier).notify("winner");

            }
            db.SaveChanges();

            Session.Clear();

            var numOfUnseen = db.Notifications.Where(n => n.UserId == userId && n.IsSeen == false).Count();
            Session["UnSeenNoti"] = numOfUnseen;




            //var callbackUrl = Url.Action("Details", "MyOrders", null, protocol: Request.Url.Scheme);
            //await UserManager.SendEmailAsync(user.Id, "Đặt hàng thành công", "Xem chi tiết tại <a href=\"" + callbackUrl + "\">đây nè :)</a>");
            return RedirectToAction("Index","Home");
        }

        // GET: Order
        public ActionResult Details(int? id)
        {
            var userId = User.Identity.GetUserId<int>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            IEnumerable<OrderStatus> listStatuses = db.OrderStatuses.ToList();
            ViewBag.ListStatus = listStatuses;
            return View(order);
        }

        // GET: Order
        public ActionResult Review(int id)
        {
            var model = new ReviewViewModel
            {
                PriceGrades = db.Grades,
                QualityGrades = db.Grades,
                ServiceGrades = db.Grades,
                OrderId = id
            };

            return PartialView("_Review", model);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Review(ReviewViewModel model)
        {
            Review newReview = new Review
            {
                OrderId = model.OrderId,
                PriceGradeId = model.PriceGrade,
                QualityGradeId = model.QualityGrade,
                ServiceGradeId = model.ServiceGrade,
                Comment = model.Comment,
                CreatedDate = DateTime.Now,
                CustomerId = User.Identity.GetUserId<int>()
            };
            var order = db.Orders.Find(model.OrderId);
            order.StatusId = 5; //Đã review

            newReview.SupplierId = order.SupplierId;

            db.Reviews.Add(newReview);

            var count = db.Reviews.Where(r => r.SupplierId == order.SupplierId).Count();
            var supplier = await UserManager.FindByIdAsync(order.SupplierId);
            var oldAvg = supplier.AverageGrade;

            var priceGrade = db.Grades.Where(g => g.Id == model.PriceGrade).FirstOrDefault().Grade1;
            var qualityGrade = db.Grades.Where(g => g.Id == model.QualityGrade).FirstOrDefault().Grade1;
            var serviceGrade = db.Grades.Where(g => g.Id == model.ServiceGrade).FirstOrDefault().Grade1;
            
            var newCusAvg = (priceGrade + qualityGrade + serviceGrade) / 3.0;
            var newAvg =  ( (oldAvg * count) + newCusAvg) / (count + 1);
            newAvg = Math.Round(newAvg, 1);

            supplier.AverageGrade = newAvg;

            await UserManager.UpdateAsync(supplier);

            db.SaveChanges();
            
            return new JsonResult { Data = new { Message = "Success" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }

    
}