using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNet.SignalR;
using TGVL.Hubs;

namespace TGVL.Controllers
{
    public class RequestController : Controller
    {
        private BMWEntities db = new BMWEntities();
        private ApplicationUserManager _userManager;

        public RequestController()
        {
        }

        public RequestController(ApplicationUserManager userManager)
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

        // GET: Request
        public ActionResult Index()
        {
            //Xem tất cả request
            return View();
        }

        //TungDM: Autocomplete
        //GET
        public ActionResult ProductAutocomplete(string term)
        {
            if (term != null)
            {
                var model = db.SysProducts
                .Where(p => p.Name.StartsWith(term))
                .Take(10)
                .Select(p => new
                {
                    label = p.Name
                });
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("Create");
        }

        //TungDM
        // GET: Request
        public async Task<ActionResult> Create(string mode, string searchString, string[] selectedProduct, int[] quantity)
        {
            var model = new RequestProductViewModel();

            if (Request.IsAjaxRequest())
            {
                if (!String.IsNullOrWhiteSpace(searchString))
                {
                    var productList = db.SysProducts
                        .Where(p => p.Name.Contains(searchString));

                    if (productList == null || !productList.Any())
                    {
                        //Sản phẩm hiện k có trong hệ thống
                        model.Message = "Sản phẩm hiện k có trong hệ thống";

                        //return Json(null, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        model.Products = productList.ToList();

                        if (selectedProduct != null)
                        {
                            for (int i = 0; i < selectedProduct.Count(); i++)
                            {
                                var key = "product" + selectedProduct[i];
                                Session[key] = quantity[i];
                            }

                            model.SelectedProduct = new List<SysProduct>();
                            foreach (var productId in selectedProduct)
                            {
                                var productToAdd = db.SysProducts.Find(int.Parse(productId));

                                if (model.Products.Contains(productToAdd))
                                {
                                    model.Products.Remove(productToAdd);
                                }

                                model.SelectedProduct.Add(productToAdd);
                            }

                            model.ListQuantity = quantity;

                            if (model.Products.Count == 0)
                            {
                                model.Message = "Mua gì lắm thế???";
                            }
                        }
                    }
                    return PartialView("_RequestProduct", model);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
            Session.Clear();

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            var model2 = new CreateRequestViewModel();
            model2.CustomerName = user.Fullname;
            model2.Email = user.Email;
            model2.Address = user.Address;
            model2.Phone = user.PhoneNumber;

            ViewBag.PaymentType = new SelectList(db.Payments, "Id", "Type");
            ViewBag.TypeOfHouse = new SelectList(db.Houses, "Id", "Type");
            return View(model2);
        }

        //TungDM
        // GET: Request
        public ActionResult SelectProduct(RequestProductViewModel model, string[] selectedProduct)
         {
            if (Request.IsAjaxRequest())
            {
                if (selectedProduct != null)
                {
                    model.Products = new List<SysProduct>();
                    foreach (var productId in selectedProduct)
                    {
                        var productToAdd = db.SysProducts.Find(int.Parse(productId));
                        model.SelectedProduct.Add(productToAdd); 
                    }
                }
            }
            return PartialView("_SelectedProduct", model);
        }

        //TungDM
        // POST: Shop/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateRequestViewModel model, string mode, string[] selectedProduct, int[] quantity)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());

                var request = new Request
                {
                    CustomerId = user.Id,
                    DeliveryAddress = user.Address,
                    ReceivingDate = model.ReceivingDate,
                    Descriptions = WebUtility.HtmlDecode(model.Description),
                    StartDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(model.TimeRange),
                    PaymentId = model.PaymentType,
                    Title = model.Title,
                    TypeOfHouse = model.TypeOfHouse,
                    Flag = 0
                };

                if (mode == "bid")
                {
                    request.Flag = 1; //set flag = 1: bid request
                }

                db.Requests.Add(request);

                if (selectedProduct != null && quantity != null)
                {
                    model.RequestProducts = new List<SysProduct>();

                    for (int i = 0; i < selectedProduct.Count(); i++)
                    {
                        var requestProduct = new RequestProduct
                        {
                            RequestId = request.Id,
                            SysProductId = int.Parse(selectedProduct[i]),
                            Quantity = quantity[i],
                        };
                        db.RequestProducts.Add(requestProduct);
                    }
                }

                if (mode == "bid")
                {
                    var bidRequest = new BidRequest
                    {
                        RequestId = request.Id,
                        CreatedDate = request.StartDate,
                        Flag = 0
                    };
                    db.BidRequests.Add(bidRequest);
                }

                db.SaveChanges();
                Session.Clear();
                return RedirectToAction("Index");
            }


            return View(model);
        }

        //TungDM
        //GET: Request/Details/Id
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Request request = db.Requests.Find(id);
            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Flag == 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    //Normar request
                    var reply = db.Replies
                                        .Where(r => r.SupplierId == userId && r.RequestId == id);


                    ViewBag.Repliable = reply.Count() == 1 ? false : true;

                    var query = "SELECT [dbo].[Requests].[CustomerId], [dbo].[Users].[Id] AS SupplierId, [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[Description], [dbo].[Replies].[CreatedDate], [dbo].[Replies].[Id] "
                        + "FROM [dbo].[Replies], [dbo].[Users], [dbo].[Requests] "
                        + "WHERE [dbo].[Replies].[RequestId] = {0} "
                        + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                        + "AND [dbo].[Requests].[Id] = [dbo].[Replies].[RequestId] "
                        + "ORDER BY CASE WHEN [dbo].[Replies].[SupplierId] = {1} THEN 0 else 1 END, [dbo].[Replies].[CreatedDate] DESC ";
                    IEnumerable<BriefReply> data = db.Database.SqlQuery<BriefReply>(query, id, userId).ToList();
                    ViewBag.Replies = data;
                }
            }
            else
            {
                //Bid request

                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();

                    if (await UserManager.IsInRoleAsync(userId, "Customer") && userId == request.CustomerId)
                    {
                        //View all bid reply: cho customer sở hữu request đó
                        var query = "SELECT [dbo].[BidReplies].[Rank], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                            + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users] "
                            + "WHERE [dbo].[Replies].[RequestId] = {0} "
                            + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND [dbo].[Replies].[Id] = [dbo].[BidReplies].[ReplyId] "
                            + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";

                        IEnumerable<BriefBidReply> data = db.Database.SqlQuery<BriefBidReply>(query, id).ToList();
                        ViewBag.BidReplies = data;
                    }
                    else if (await UserManager.IsInRoleAsync(userId, "Supplier"))
                    {
                        var reply = db.Replies
                                        .Where(r => r.SupplierId == userId && r.RequestId == id);
                        ViewBag.Bidable = reply.Count() == 1 ? false : true;

                        var query = "SELECT [dbo].[BidReplies].[Rank], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                            + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users] "
                            + "WHERE [dbo].[Replies].[RequestId] = {0} "
                            + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND [dbo].[Replies].[Id] = [dbo].[BidReplies].[ReplyId] "
                            + "AND [dbo].[Replies].[SupplierId] = {1} "
                            + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";
                        IEnumerable<BriefBidReply> data = db.Database.SqlQuery<BriefBidReply>(query, id, userId).ToList();
                        ViewBag.BidReplies = data;
                    }
                }
                
            };

            return View(request);
        }

        //TungDM
        //GET: Request/UpdateReplies
        public JsonResult UpdateReplies(int id, int type)
        {
            var test = Session["LastUpdated"];
            var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;
            
            if (type == 1)
            {
                //bid

                ///////////////////////////////////////////////////
                //Request request = db.Requests.Find(id);

                //var newestDate = request.Replies.OrderByDescending(r => r.CreatedDate).FirstOrDefault().CreatedDate;
                //var listUser = new List<string>();
                
                //var query = "SELECT[dbo].[Users].[UserName] "
                //        + "FROM[dbo].[Replies], [dbo].[Requests], [dbo].[Users] "
                //        + "WHERE[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                //        + "AND[dbo].[Replies].[RequestId] = [dbo].[Requests].[Id] "
                //        + "AND[dbo].[Requests].[Id] = {0} "
                //        + "AND[dbo].[Replies].[CreatedDate] < {1}";
                //List<string> listUser2 = db.Database.SqlQuery<string>(query, id, newestDate).ToList();

                //if (listUser2.Count() != 0)
                //{
                //    foreach (var u in listUser2)
                //    {
                //        listUser.Add(u);
                //    }

                //    var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                //    foreach (var userId in listUser)
                //    {
                //        notificationHub.Clients.User(userId).notify("update");
                //    }
                //}
                ///////////////////////////////////////////////////

                var query = "SELECT [dbo].[BidReplies].[Rank], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                            + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users] "
                            + "WHERE [dbo].[Replies].[RequestId] = {0} "
                            + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND [dbo].[Replies].[Id] = [dbo].[BidReplies].[ReplyId] "
                            + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";

                IEnumerable<BriefBidReply> list = db.Database.SqlQuery<BriefBidReply>(query, id).ToList();

                Session["LastUpdated"] = DateTime.Now;
                return new JsonResult {
                    Data = new {
                        ReplyType = "Bid",
                        BidReplies = list
                    }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            } else
            {
                //normal
                var list = db.Replies
                .Where(r => r.CreatedDate > notificationRegisterTime && r.RequestId == id)
                .Select(r => new {
                    SupplierName = r.User.Fullname,
                    Avatar = r.User.Avatar,
                    Address = r.User.Address,
                    Description = r.Description,
                    Total = r.Total,
                    CreatedDate = r.CreatedDate,
                    ReplyId = r.Id
                })
                .OrderBy(r => r.CreatedDate)
                .ToList();

                Session["LastUpdated"] = DateTime.Now;
                return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }         
        }

        //GET: update bid rank của Supplier 
        public ActionResult UpdateBidRank(int requestId)
        {
            var userId = User.Identity.GetUserId<int>();

            var reply = db.Replies.Where(r => r.RequestId == requestId && r.SupplierId == userId).FirstOrDefault();
            if (reply != null)
            {
                var rank = db.BidReplies.Where(r => r.ReplyId == reply.Id).FirstOrDefault().Rank;
                return new JsonResult { Data = new { Rank = rank}, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return new JsonResult { Data = new { Success = "FAIL" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // GET: Request
        public ActionResult CreateRequest()
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

        public ActionResult PlaceBidRequest()
        {
            return View();
        }

        public ActionResult CreateBidRequest()
        {
            return View();
        }

        public ActionResult CreateNormalRequest()
        {
            return View();
        }
        
        public ActionResult ChooseRequest()
        {
            return View();
        }

        public ActionResult ViewNormalDetail()
        {
            return View();
        }

        public ActionResult ViewBidDetail()
        {
            return View();
        }

        public ActionResult ShoppingCart()
        {
            return View();
        }
    }
}