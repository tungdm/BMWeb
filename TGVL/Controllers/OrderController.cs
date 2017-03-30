using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                    newOrder.Flag = 3;  //order detail cho deal
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
                            oderDetails.Flag = 3; //order detail cho deal
                        }
                        db.OrderDetails.Add(oderDetails);
                        //else if (o.Type == "normal") //mua lẻ
                        //{

                        //}

                    }
                    
                }

                

            } else //normal request hoặc bid request
            {
                var reply = db.Replies.Find(replyId);

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
                
            }
            db.SaveChanges();

            Session.Clear();

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