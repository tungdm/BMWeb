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
                    DeliveryDate = model.ReceivingDate,
                    Descriptions = WebUtility.HtmlDecode(model.Description),
                    StartDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(model.TimeRange),
                    PaymentId = model.PaymentType,
                    Title = model.Title,
                    TypeOfHouse = model.TypeOfHouse
                };
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
                        CreatedDate = request.StartDate
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
        public ActionResult Details(int? id)
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

            var query = "SELECT [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[Description], [dbo].[Replies].[CreatedDate], [dbo].[Replies].[Id] "
                    + "FROM[dbo].[Replies], [dbo].[Users] "
                    + "WHERE[dbo].[Replies].[RequestId] = {0} "
                    + "AND[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id]"
                    + "ORDER BY [dbo].[Replies].[CreatedDate] DESC"; ;
            IEnumerable<BriefReply> data = db.Database.SqlQuery<BriefReply>(query, id).ToList();
            ViewBag.Replies = data;

            return View(request);
        }

        //TungDM
        //GET: Request/UpdateReplies
        public JsonResult UpdateReplies(int id)
        {
            var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;

            //NotificationComponent NC = new NotificationComponent();
            //var list = NC.GetReplies(notificationRegisterTime).ToList();
            var userId = User.Identity.GetUserId<int>();

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

            //update session here for get only new added contacts (notification)
            Session["LastUpdate"] = DateTime.Now;
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
    }
}