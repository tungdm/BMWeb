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
using PagedList.Mvc;
using PagedList;
using TGVL.LucenceSearch;
using System.Globalization;

namespace TGVL.Controllers
{
    [System.Web.Mvc.Authorize]
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
        [System.Web.Mvc.Authorize(Roles = "Customer")]
        public async Task<ActionResult> Create(string mode, string searchString, string[] selectedProduct, int[] quantity)
        {
            //Setting config
            var configs = db.Settings.Where(s => s.SettingTypeId == 1).ToList(); //1 <=> Create Request

            var model = new RequestProductViewModel();
            model.Flag = mode;

            if (Request.IsAjaxRequest())
            {
                if (!String.IsNullOrWhiteSpace(searchString))
                {
                    //var productList = db.SysProducts
                    //    .Where(p => p.Name.Contains(searchString));

                    var searchResult = new LuceneResult();

                    searchResult = GoLucene.Search(searchString);
                    model.SearchResult = searchResult.SearchResult;

                    if (model.SearchResult == null || !model.SearchResult.Any())
                    {
                        //Sản phẩm hiện k có trong hệ thống
                        model.Message = "Sản phẩm hiện k có trong hệ thống";
                        model.SuggestWords = searchResult.SuggestWords;
                        //return Json(null, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                       // model.Products = productList.ToList();

                        if (selectedProduct != null)
                        {
                            for (int i = 0; i < selectedProduct.Count(); i++)
                            {
                                var key = "product" + selectedProduct[i];
                                Session[key] = quantity[i];
                            }

                            model.SelectedProduct = new List<ProductSearchResult>();
                            var modifiedSearchResult = new List<ProductSearchResult>();

                            foreach (var productId in selectedProduct)
                            {
                                //var productToAdd = db.SysProducts.Find(int.Parse(productId));
                                var query = "SELECT [dbo].[SysProducts].[Id], [dbo].[SysProducts].[Name], [dbo].[SysProducts].[Image], "
                                        +" [dbo].[SysProducts].[Description], [dbo].[SysProducts].[UnitPrice], "
                                        + "[dbo].[Manufacturers].[Name] AS ManufactureName, [dbo].[UnitTypes].[Type] AS UnitType "
                                        + "FROM[dbo].[SysProducts], [dbo].[Manufacturers], [dbo].[UnitTypes] "
                                        + "WHERE[dbo].[Manufacturers].[Id] = [dbo].[SysProducts].[ManufacturerId] "
                                        + "AND[dbo].[UnitTypes].[Id] = [dbo].[SysProducts].[UnitTypeId] "
                                        + "AND[dbo].[SysProducts].[Id] = {0}";
                                ProductSearchResult data = db.Database.SqlQuery<ProductSearchResult>(query, productId).FirstOrDefault();

                                //if (model.Products.Contains(productToAdd))
                                //{
                                //    model.Products.Remove(productToAdd);
                                //}

                                var index = model.SearchResult.ToList().FindIndex(item => item.Id == int.Parse(productId));
                                var removeObj = model.SearchResult.FirstOrDefault(x => x.Id == int.Parse(productId));
                                if (removeObj !=null)
                                {
                                    model.SearchResult.Remove(removeObj);
                                }
                                

                                model.SelectedProduct.Add(data);
                            }

                            model.ListQuantity = quantity;

                            if (model.SearchResult.Count == 0)
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
            model2.Flag = mode;
            model2.AllTypeOfHouses = db.Houses;
            model2.AllTypeOfPayments = db.Payments;
            model2.ReceivingAddress = user.Address;

            
            model2.MinNumSeletedProduct = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MinNumSeletedProduct").SingleOrDefault().SettingValue;
            model2.MaxLengthInputNumberBig = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MaxLengthInputNumberBig").SingleOrDefault().SettingValue;
            model2.MaxLengthInputNumberSmall = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MaxLengthInputNumberSmall").SingleOrDefault().SettingValue;
            model2.MinBidPrice = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MinBidPrice").SingleOrDefault().SettingValue;
            model2.MinLengthInputText = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MinLengthInputText").SingleOrDefault().SettingValue;
            model2.MaxLengthInputTextSmall = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MaxLengthInputTextSmall").SingleOrDefault().SettingValue;
            model2.MinTimeRange = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MinTimeRange").SingleOrDefault().SettingValue;
            model2.MaxTimeRange = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MaxTimeRange").SingleOrDefault().SettingValue;
            model2.MinDateDeliveryRange = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MinDateDeliveryRange").SingleOrDefault().SettingValue;


            //ViewBag.PaymentType = new SelectList(db.Payments, "Id", "Type");
            //ViewBag.TypeOfHouse = new SelectList(db.Houses, "Id", "Type");
            return View(model2);
        }

        //TungDM
        // GET: Request
        public ActionResult SelectProduct(RequestProductViewModel model, string[] selectedProduct)
        {
            var model2 = new CreateRequestViewModel();

            if (Request.IsAjaxRequest())
            {
                if (selectedProduct != null)
                {
                    //model.Products = new List<SysProduct>();

                    var test = new List<RequestedProductWithQuantity>();

                    foreach (var productId in selectedProduct)
                    {
                        //var productToAdd = db.SysProducts.Find(int.Parse(productId));
                        //model.SelectedProduct.Add(productToAdd);

                        var query = "SELECT [dbo].[SysProducts].[Id], [dbo].[SysProducts].[Name], [dbo].[Manufacturers].[Name] AS ManufactureName, [dbo].[SysProducts].[UnitPrice], [dbo].[UnitTypes].[Type], [dbo].[SysProducts].[Image] "
                            + "FROM[dbo].[SysProducts], [dbo].[Manufacturers], [dbo].[UnitTypes] "
                            + "WHERE[dbo].[SysProducts].[ManufacturerId] = [dbo].[Manufacturers].[Id] "
                            + "AND[dbo].[SysProducts].[UnitTypeId] = [dbo].[UnitTypes].[Id] "
                            + "AND[dbo].[SysProducts].[Id] = {0} ";
                        RequestedProduct data = db.Database.SqlQuery<RequestedProduct>(query, productId).FirstOrDefault();
                        var requestedProductWithQuantity = new RequestedProductWithQuantity
                        {
                            RequestedProduct = data,
                            Quantity = 0
                        };
                        test.Add(requestedProductWithQuantity);
                    }
                    //Setting config
                    var configs = db.Settings.Where(s => s.SettingTypeId == 1).ToList(); //1 <=> Create Request
                    model2.MaxLengthInputNumberSmall = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MaxLengthInputNumberSmall").SingleOrDefault().SettingValue;
                    model2.MinNumSeletedProduct = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MinNumSeletedProduct").SingleOrDefault().SettingValue;
                    model2.MinBidPrice = configs.Select(c => new { SettingValue = c.SettingValue, SettingName = c.SettingName }).Where(c => c.SettingName == "MinBidPrice").SingleOrDefault().SettingValue;

                    model2.RequestProducts = test;
                    return PartialView("_SelectedProductsBid", model2);
                } else
                {
                    return new JsonResult {
                        Data = new
                        {
                            Success = "Nothing"
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
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

        //TungDM
        // POST: Shop/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize(Roles = "Customer")]
        public async Task<ActionResult> Create(CreateRequestViewModel model, string mode, string[] selectedProduct, int[] quantity, int sum)
        {
            if (ModelState.IsValid)
            {
                if (selectedProduct == null)
                {
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = "Fail",
                            ErrorType = "RequireProduct",
                            Message = "Chọn ít nhất một sản phẩm",                            
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                } else
                {
                    var minTotal = db.Settings.Where(s => s.SettingTypeId == 1 && s.SettingName == "MinBidPrice").FirstOrDefault().SettingValue;

                    if (mode == "bid" && sum < minTotal)
                    {
                        return new JsonResult
                        {
                            Data = new
                            {
                                Success = "Fail",
                                ErrorType = "GreaterThanMin",
                                Min = minTotal,
                                Sum = sum
                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }

                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());

                    var request = new Request
                    {
                        CustomerId = user.Id,
                        DeliveryAddress = model.ReceivingAddress,
                        ReceivingDate = model.ReceivingDate,
                        Descriptions = WebUtility.HtmlDecode(model.Description),
                        StartDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(model.TimeRange),
                        PaymentId = model.PaymentType,
                        Title = model.Title,
                        TypeOfHouse = model.TypeOfHouse,
                        Expired = false,
                        Flag = 0
                    };

                    if (mode == "bid")
                    {
                        request.Flag = 1; //set flag = 1: bid request
                    }

                    db.Requests.Add(request);

                    foreach (var p in model.RequestProducts)
                    {
                        var requestProduct = new RequestProduct
                        {
                            RequestId = request.Id,
                            SysProductId = p.RequestedProduct.Id,
                            Quantity = p.Quantity,
                        };
                        db.RequestProducts.Add(requestProduct);
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
                    //return RedirectToAction("Index");
  
                    return JavaScript("window.location = '" + Url.Action("Details", "Request", new { id = request.Id }) + "'");
                }

            }
            var errorModel = from x in ModelState.Keys
                             where ModelState[x].Errors.Count > 0
                             select new
                             {
                                 key = x,
                                 error = ModelState[x].Errors.Select(y => y.ErrorMessage).ToArray()
                             };

            ViewBag.PaymentType = new SelectList(db.Payments, "Id", "Type");
            ViewBag.TypeOfHouse = new SelectList(db.Houses, "Id", "Type");
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
                //normal request
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
            else if (request.Flag == 1)
            {
                //Bid request
                ViewBag.DueDateCountdown = request.DueDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();

                    if (await UserManager.IsInRoleAsync(userId, "Customer") && userId == request.CustomerId)
                    {
                        //View all bid reply: cho customer sở hữu request đó
                        var query = "SELECT [dbo].[BidReplies].[Rank], [dbo].[Replies].[SupplierId], [dbo].[Requests].[CustomerId], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                            + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users], [dbo].[Requests] "
                            + "WHERE [dbo].[Replies].[RequestId] = {0} "
                            + "AND [dbo].[Replies].[RequestId] = [dbo].[Requests].[Id]"
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

                        var query = "SELECT [dbo].[BidReplies].[Rank], [dbo].[Replies].[SupplierId], [dbo].[Requests].[CustomerId], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                            + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users], [dbo].[Requests] "
                            + "WHERE [dbo].[Replies].[RequestId] = {0} "
                            + "AND [dbo].[Replies].[RequestId] = [dbo].[Requests].[Id]"
                            + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND [dbo].[Replies].[Id] = [dbo].[BidReplies].[ReplyId] "
                            + "AND [dbo].[Replies].[SupplierId] = {1} "
                            + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";
                        IEnumerable<BriefBidReply> data = db.Database.SqlQuery<BriefBidReply>(query, id, userId).ToList();
                        ViewBag.BidReplies = data;
                    }
                }

            }
            ViewBag.Expired = request.Expired;

            return View(request);
        }

        //TungDM
        //GET: Request/UpdateClientReplies
        //username: client name
        //id: requestId
        public JsonResult UpdateClientReplies(int id, string username, int newreplyId)
        {
            var request = db.Requests.FirstOrDefault(r => r.Id == id);
            var newreply = db.Replies.FirstOrDefault(r => r.Id == newreplyId);
            var userId = UserManager.FindByName(username).Id; //id của client
            if (newreply.SupplierId == userId || request.CustomerId == userId)
            {
                //Client là supplier vừa mới create reply, hoặc là customer, bỏ qua
                return new JsonResult { Data = new { Message = "Ok"}, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            } else
            {
                var query = "SELECT [dbo].[Requests].[CustomerId], [dbo].[Users].[Id] AS SupplierId, [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[Description], [dbo].[Replies].[CreatedDate], [dbo].[Replies].[Id] "
                    + "FROM [dbo].[Replies], [dbo].[Users], [dbo].[Requests] "
                    + "WHERE [dbo].[Replies].[RequestId] = {0} "
                    + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                    + "AND [dbo].[Requests].[Id] = [dbo].[Replies].[RequestId] "
                    + "ORDER BY CASE WHEN [dbo].[Replies].[SupplierId] = {1} THEN 0 else 1 END, [dbo].[Replies].[CreatedDate] DESC ";
                IEnumerable<BriefReply> data = db.Database.SqlQuery<BriefReply>(query, id, userId).ToList();

                if (data.Count() != 0)
                {
                    var rep = db.Replies.FirstOrDefault(r => r.SupplierId == userId);
                    if (rep != null)
                    {
                        return new JsonResult { Data = new { data = data, replyId = rep.Id }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    } else
                    {
                        return new JsonResult { Data = new { data = data }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    
                }
            }

            return new JsonResult { Data = new { Message = "Fail" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        //TungDM
        //GET: Request/UpdateClientReplies
        //username: client name
        //id: requestId
        //newreplyId: reply vừa edit
        public JsonResult UpdateClientRepliesEdit(int id, string username, int newreplyId)
        {
            var request = db.Requests.FirstOrDefault(r => r.Id == id);
            var editreply = db.Replies.FirstOrDefault(r => r.Id == newreplyId);

            var userId = UserManager.FindByName(username).Id; //id của client
            if (editreply.SupplierId == userId )
            {
                //Client là supplier vừa mới create reply, bỏ qua
                return new JsonResult { Data = new { Message = "Ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                var query = "SELECT [dbo].[Requests].[CustomerId], [dbo].[Users].[Id] AS SupplierId, [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[Description], [dbo].[Replies].[CreatedDate], [dbo].[Replies].[Id] "
                    + "FROM [dbo].[Replies], [dbo].[Users], [dbo].[Requests] "
                    + "WHERE [dbo].[Replies].[RequestId] = {0} "
                    + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                    + "AND [dbo].[Requests].[Id] = [dbo].[Replies].[RequestId] "
                    + "ORDER BY CASE WHEN [dbo].[Replies].[SupplierId] = {1} THEN 0 else 1 END, [dbo].[Replies].[CreatedDate] DESC ";
                IEnumerable<BriefReply> data = db.Database.SqlQuery<BriefReply>(query, id, userId).ToList();

                if (data.Count() != 0)
                {
                    if (request.CustomerId == userId) //client là owner của request
                    {
                        return new JsonResult { Data = new { data = data, flag = "owner" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    } else
                    {
                        var rep = db.Replies.FirstOrDefault(r => r.SupplierId == userId);
                        if (rep != null)
                        {
                            return new JsonResult { Data = new { data = data, replyId = rep.Id }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        }
                        else
                        {
                            return new JsonResult { Data = new { data = data }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        }
                    }
                }
            }

            return new JsonResult { Data = new { Message = "Fail" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        //TungDM
        //GET: Request/UpdateReplies
        public JsonResult UpdateReplies(int id, string username)
        {
            var test = Session["LastUpdated"];
            var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;

            var request = db.Requests.FirstOrDefault(r => r.Id == id);

            if (request.Flag == 1)
            {
                //bid

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
                var userId = UserManager.FindByName(username).Id;
                
                var query = "SELECT [dbo].[Requests].[CustomerId], [dbo].[Users].[Id] AS SupplierId, [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[Description], [dbo].[Replies].[CreatedDate], [dbo].[Replies].[Id] "
                        + "FROM [dbo].[Replies], [dbo].[Users], [dbo].[Requests] "
                        + "WHERE [dbo].[Replies].[RequestId] = {0} "
                        + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                        + "AND [dbo].[Requests].[Id] = [dbo].[Replies].[RequestId] "
                        + "ORDER BY CASE WHEN [dbo].[Replies].[SupplierId] = {1} THEN 0 else 1 END, [dbo].[Replies].[CreatedDate] DESC ";
                IEnumerable<BriefReply> data = db.Database.SqlQuery<BriefReply>(query, id, userId).ToList();


                //var list = db.Replies
                //.Where(r => r.CreatedDate > notificationRegisterTime && r.RequestId == id)
                //.Select(r => new {
                //    SupplierName = r.User.Fullname,
                //    Avatar = r.User.Avatar,
                //    Address = r.User.Address,
                //    Description = r.Description,
                //    Total = r.Total,
                //    CreatedDate = r.CreatedDate,
                //    ReplyId = r.Id
                //})
                //.OrderBy(r => r.CreatedDate)
                //.ToList();

                Session["LastUpdated"] = DateTime.Now;
                return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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

        //TungDM: show message bid request expired
        public ActionResult Expired()
        {
            var userId = User.Identity.GetUserId<int>(); //customerID 

           
            var request = db.Requests.Where(r => r.Flag == 1 && r.CustomerId == userId && r.Expired == true).OrderByDescending(r => r.DueDate).FirstOrDefault();
            var message = "Yêu cầu \"" + request.Title + "\" của bạn vừa mới kết thúc!";

            //Create noti
            var notify = new Notification
            {
                RequestId = request.Id,
                UserId = userId,
                Message = message,
                CreatedDate = DateTime.Now,
                IsSeen = false
            };
            db.Notifications.Add(notify);
            db.SaveChanges();

            return new JsonResult { Data = new { Message = message, RequestId = request.Id }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // GET: Request
        public ActionResult Confirm(int id)
        {
            Reply reply = db.Replies.Find(id);

            var model = new ReplyDetails
            {
                Id = reply.Id,
                Total = reply.Total,

                //Description = reply.Description,
                DeliveryDate = reply.DeliveryDate,
                ShippingFee = (int)reply.ShippingFee,
                Discount = (int)reply.Discount,
                //Rank = reply.Flag == 1 ? reply.BidReply.Rank : 0,
                Flag = (int)reply.Flag
            };

            var query = "SELECT [dbo].[ReplyProducts].[Id] AS [ReplyProductId], [dbo].[Products].[Id], [dbo].[Products].[UnitPrice], "
                   + "[dbo].[Products].[Image], [dbo].[SysProducts].[Name], [dbo].[UnitTypes].[Type], [dbo].[ReplyProducts].[Quantity], "
                   + "[dbo].[ReplyProducts].[Quantity] * [dbo].[Products].[UnitPrice] AS Total "
                   + "FROM [dbo].[Replies], [dbo].[ReplyProducts], [dbo].[Products], [dbo].[SysProducts], [dbo].[UnitTypes] "
                   + "WHERE [dbo].[Replies].[Id] = [dbo].[ReplyProducts].[ReplyId] "
                   + "AND [dbo].[ReplyProducts].[ProductId] = [dbo].[Products].[Id] "
                   + "AND [dbo].[Products].[SysProductId] = [dbo].[SysProducts].[Id] "
                   + "AND [dbo].[SysProducts].[UnitTypeId] = [dbo].[UnitTypes].[Id] "
                   + "AND [dbo].[Replies].[Id] = {0}";
            IList<RepliedProduct> data = db.Database.SqlQuery<RepliedProduct>(query, id).ToList();
            ViewBag.RepliedProduct = data;
            ViewBag.Customer = UserManager.FindById(reply.Request.CustomerId);
            ViewBag.Supplier = UserManager.FindById(reply.SupplierId);

            //model.ReplyProducts = data;

            return View(model);
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

        public ActionResult Bid(int? page)
        {
            var pageSize = 5;
            var pageNumber = (page??1);
            var userId = User.Identity.GetUserId<int>();
            var query = "SELECT[dbo].[Requests].[Id], [dbo].[Requests].[Title],[dbo].[Requests].[DeliveryAddress],[dbo].[Requests].[DueDate],[dbo].[Requests].[ReceivingDate],[dbo].[Requests].[Descriptions],[dbo].[Users].[UserName], [dbo].[Users].[Avatar] "
            + "FROM[dbo].[Requests], [dbo].[Users] "
            + "WHERE[dbo].[Requests].[Flag] = 1 AND [dbo].[Requests].[CustomerId] = [dbo].[Users].[Id] ";
            IList<RequestFloorModel> data = db.Database.SqlQuery<RequestFloorModel>(query).ToList();

            ViewBag.ListBidRequest = data;
            return View(data.ToPagedList(pageNumber,pageSize));
        }

        public ActionResult Normal(int? page)
        {
            var pageSize = 5;
            var pageNumber = (page ?? 1);
            var userId = User.Identity.GetUserId<int>();
            var query = "SELECT[dbo].[Requests].[Id], [dbo].[Requests].[Title],[dbo].[Requests].[DeliveryAddress],[dbo].[Requests].[ReceivingDate],[dbo].[Requests].[Descriptions],[dbo].[Users].[UserName] "
            + "FROM[dbo].[Requests], [dbo].[Users] "
            + "WHERE[dbo].[Requests].[Flag] = 0 AND [dbo].[Requests].[CustomerId] = [dbo].[Users].[Id] ";
            IList<RequestFloorModel> data = db.Database.SqlQuery<RequestFloorModel>(query).ToList();

            ViewBag.ListNormalRequest = data;
            //var request = new Request();
            //model.Title = model.Title;
            //foreach (var p in model.RequestProducts)
            //{
            //    var requestProduct = new RequestProduct
            //    {
            //        RequestId = request.Id,
            //        SysProductId = p.RequestedProduct.Id,

            //    };
            //    db.RequestProducts.Add(requestProduct);
            //}
            return View(data.ToPagedList(pageNumber,pageSize));
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

        public ActionResult CheckOut()
        {
            return View();
        }

        public ActionResult DealDetail()
        {
            return View();
        }

        public ActionResult CreateDeal()
        {
            return View();
        }
    }
}