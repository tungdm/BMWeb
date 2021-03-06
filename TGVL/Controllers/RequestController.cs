﻿using Microsoft.AspNet.Identity.Owin;
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
using System.Data.Entity;
using System.Net.Mail;
using System.IO;

namespace TGVL.Controllers
{
    [System.Web.Mvc.Authorize]
    public class RequestController : Controller
    {
        private BMWEntities db = new BMWEntities();
        private ApplicationUserManager _userManager;
        private List<FilterRequest> list1 = new List<FilterRequest> {
            new FilterRequest { Name = "Mới nhất" , Value = 1},
            new FilterRequest { Name = "Nổi bật" , Value = 2},
            new FilterRequest { Name = "Phù hợp nhất" , Value = 0}
        };

        private List<FilterRequest> list2 = new List<FilterRequest> {
            new FilterRequest { Name = "Tất cả" , Value = 2},
            new FilterRequest { Name = "Mua thường" , Value = 0},
            new FilterRequest { Name = "Đấu thầu" , Value = 1}
        };

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
        public ActionResult Index(int? page, int? sort, int? type, string searchString)
        {
            ViewBag.list1 = list1;
            ViewBag.list2 = list2;

            var pageSize = 5;
            var pageNumber = (page ?? 1);
            int? sortNumber = sort;
            if (sortNumber == null || sortNumber > 2 || sortNumber < 1)
            {
                sortNumber = 1; //default
            }

            int? typeNumber = type;
            if (typeNumber == null || typeNumber > 2 || typeNumber < 0)
            {
                typeNumber = 2; //default
            }
            
            if (searchString != null && searchString != "")
            {
                sortNumber = 0; //tim kiem phu hop nhat
            }

            ViewBag.page = pageNumber;
            ViewBag.sort = sortNumber;
            ViewBag.type = typeNumber;

            var userId = User.Identity.GetUserId<int>();

            var requestFilter = new List<Request>();
            
            if (searchString == null || searchString == "")
            {
                
                if (typeNumber == 2) //all
                {
                    var requests = db.Requests.Where(r => r.StatusId == 1);
                    if (sortNumber == 1) //mới nhất
                    {
                        requests = requests.OrderByDescending(r => r.StartDate);
                    }
                    requestFilter = requests.ToList();
                }
                else //normal hoặc bid
                {
                    var requests = db.Requests.Where(r => r.StatusId == 1 && r.Flag == typeNumber);
                    if (sortNumber == 1) //mới nhất
                    {
                        requests = requests.OrderByDescending(r => r.StartDate);
                    }
                    requestFilter = requests.ToList();
                }

                //var requests = db.Requests.Where(r => r.StatusId == 1).OrderByDescending(r => r.StartDate).ToList();
                var allRequest = new List<RequestFloorModel>();

                foreach (var item in requestFilter)
                {
                    string listProduct = "";
                    var requestId = item.Id;

                    var query = "SELECT Distinct [dbo].[SysProducts].[Name] "
                            + "FROM[dbo].[RequestProducts], [dbo].[SysProducts] "
                            + "WHERE[dbo].[RequestProducts].[SysProductId] = [dbo].[SysProducts].[Id] "
                            + "AND[dbo].[RequestProducts].[RequestId] = {0} ";
                    var listProductRaw = db.Database.SqlQuery<ReplyProducts>(query, requestId).ToList();

                    for (int i = 0; i < listProductRaw.Count; i++)
                    {
                        if (i == (listProductRaw.Count - 1))
                        {
                            listProduct = listProduct + listProductRaw[i].Name;
                        }
                        else
                        {
                            listProduct = listProduct + listProductRaw[i].Name + ", ";
                        }
                    }

                    var m = new RequestFloorModel
                    {
                        Id = requestId,
                        Title = item.Title,
                        Slug = new HomeController().GenerateSlug(item.Title, requestId),
                        DeliveryAddress = item.DeliveryAddress,
                        Descriptions = item.Descriptions,
                        UserName = item.User.Fullname,
                        Avatar = item.User.Avatar,
                        //Image = item.Image,
                        StartDate = item.StartDate,
                        NumReplies = db.Replies.Where(r => r.RequestId == requestId).Count(),
                        Flag = (int)item.Flag,
                        ListProducts = listProduct
                    };
                    if (m.Flag == 1)
                    {
                        m.DueDateCountdown = item.DueDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    allRequest.Add(m);
                }

                if (sortNumber == 2)
                {
                    allRequest = allRequest.OrderByDescending(r => r.NumReplies).ThenByDescending(r => r.StartDate).ToList();
                }

                return View(allRequest.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                searchString = searchString.Trim();
                searchString = System.Text.RegularExpressions.Regex.Replace(searchString, @"\s+", " ");
                var LuceneFilter = new List<LuceneRequest>();

                var hit_limits = db.Requests.Where(r => r.StatusId == 1).Count();

                var searchResult = new LuceneRequestResult();

                if (searchString.Length == 1)
                {
                    searchResult = LuceneSimilar.Search(searchString, hit_limits);
                }
                else
                {
                    searchResult = LuceneSimilar.SearchDefault(searchString, hit_limits);
                }

                if (searchResult.SimilarResult.Count == 0)
                {
                    var allRequests = db.Requests.Where(r => r.StatusId == 1);

                    var test = db.RequestProducts.Where(r => r.SysProduct.Name.Contains(searchString)).Select(r => r.RequestId).Distinct();

                    var ListRequest = new List<LuceneRequest>();

                    if (!test.Any())
                    {
                        ViewBag.SearchString = searchString;
                        return View();
                    }
                    foreach (var rId in test)
                    {
                        var request = db.Requests.Find(rId);

                        var luceneRequest = new LuceneRequest();
                        var requestId = request.Id;
                        luceneRequest.Id = requestId;
                        luceneRequest.Avatar = request.User.Avatar;
                        luceneRequest.Title = request.Title;
                        luceneRequest.CustomerName = request.User.Fullname;
                        luceneRequest.StartDate = request.StartDate;
                        luceneRequest.DueDate = request.DueDate;
                        luceneRequest.Flag = (int)request.Flag;
                        //luceneRequest.Image = request.Image;

                        ListRequest.Add(luceneRequest);
                    }
                    searchResult.SimilarResult = ListRequest;
                }


                //searchResult = LuceneSimilar.SearchDefault(searchString, hit_limits);
                ViewBag.SearchString = searchString;
                if (typeNumber == 2) //all
                {
                    var requests = searchResult.SimilarResult.ToList();
                    if (sortNumber == 1) //mới nhất
                    {
                        requests = requests.OrderByDescending(r => r.StartDate).ToList();
                    }
                    LuceneFilter = requests;
                }
                else //normal hoặc bid
                {
                    var requests = searchResult.SimilarResult.Where(r => r.Flag == typeNumber).ToList();
                    if (sortNumber == 1) //mới nhất
                    {
                        requests = requests.OrderByDescending(r => r.StartDate).ToList();
                    }
                    LuceneFilter = requests;
                }
                var allRequest = new List<RequestFloorModel>();

                foreach (var item in LuceneFilter)
                {
                    var requestId = item.Id;
                    var luRe = db.Requests.Find(requestId);
                    string listProduct = "";

                    var query = "SELECT Distinct [dbo].[SysProducts].[Name] "
                            + "FROM[dbo].[RequestProducts], [dbo].[SysProducts] "
                            + "WHERE[dbo].[RequestProducts].[SysProductId] = [dbo].[SysProducts].[Id] "
                            + "AND[dbo].[RequestProducts].[RequestId] = {0} ";
                    var listProductRaw = db.Database.SqlQuery<ReplyProducts>(query, requestId).ToList();

                    for (int i = 0; i < listProductRaw.Count; i++)
                    {
                        if (i == (listProductRaw.Count - 1))
                        {
                            listProduct = listProduct + listProductRaw[i].Name;
                        }
                        else
                        {
                            listProduct = listProduct + listProductRaw[i].Name + ", ";
                        }
                    }

                    var m = new RequestFloorModel
                    {
                        Id = requestId,
                        Title = item.Title,
                        Slug = new HomeController().GenerateSlug(item.Title, requestId),
                        DeliveryAddress = luRe.DeliveryAddress,
                        Descriptions = luRe.Descriptions,
                        UserName = luRe.User.Fullname,
                        Avatar = luRe.User.Avatar,
                        //Image = item.Image,
                        StartDate = item.StartDate,
                        NumReplies = db.Replies.Where(r => r.RequestId == requestId).Count(),
                        Flag = item.Flag,
                        ListProducts = listProduct
                    };

                    if (m.Flag == 1)
                    {
                        m.DueDateCountdown = item.DueDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    allRequest.Add(m);
                }

                if (sortNumber == 2)
                {
                    allRequest = allRequest.OrderByDescending(r => r.NumReplies).ThenByDescending(r => r.StartDate).ToList();
                }

                return View(allRequest.ToPagedList(pageNumber, pageSize));
            }
        }

        public async Task<ActionResult> Manage()
        {
            //Xem tất cả request
            var userId = User.Identity.GetUserId<int>();
            if (await UserManager.IsInRoleAsync(userId, "Customer"))
            {
                IEnumerable<Request> listRequests = db.Requests.Where(x => x.CustomerId == userId && x.StatusId != 4).ToList();
                ViewBag.ListRequests = listRequests;
                ViewBag.MaxDateCancelRequest = db.Settings.Where(s => s.SettingName == "MaxDateCancelRequest").FirstOrDefault().SettingValue;
            } else if (await UserManager.IsInRoleAsync(userId, "Supplier"))
            {
                var listId = db.Replies.Where(r => r.SupplierId == userId && r.BidReply.Flag != 9 && r.Request.StatusId != 4).Select(r => new { Id = r.RequestId }).ToList();
                var listRequests = new List<Request>();
                foreach (var id in listId)
                {
                    var request = db.Requests.Find(id.Id);
                    listRequests.Add(request);
                }
                ViewBag.ListRequests = listRequests;
            }
            
            
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

                    if (searchString.Length == 1)
                    {
                        searchResult = GoLucene.Search(searchString);
                    }
                    else
                    {
                        searchResult = GoLucene.SearchDefault(searchString);
                    }
                   
                    model.SearchResult = searchResult.SearchResult;

                    if (model.SearchResult == null || !model.SearchResult.Any())
                    {
                        //Sản phẩm hiện k có trong hệ thống
                        model.Message = "Sản phẩm hiện không có trong hệ thống";
                        model.SuggestWords = searchResult.SuggestWords;
                        if (selectedProduct != null)
                        {
                            for (int i = 0; i < selectedProduct.Count(); i++)
                            {
                                var key = "product" + selectedProduct[i];
                                Session[key] = quantity[i];
                            }
                            model.SelectedProduct = new List<ProductSearchResult>();
                            foreach (var productId in selectedProduct)
                            {
                                var query = "SELECT [dbo].[SysProducts].[Id], [dbo].[SysProducts].[Name], [dbo].[SysProducts].[Image], "
                                        + " [dbo].[SysProducts].[Description], [dbo].[SysProducts].[UnitPrice], "
                                        + "[dbo].[Manufacturers].[Name] AS ManufactureName, [dbo].[UnitTypes].[Type] AS UnitType "
                                        + "FROM[dbo].[SysProducts], [dbo].[Manufacturers], [dbo].[UnitTypes] "
                                        + "WHERE[dbo].[Manufacturers].[Id] = [dbo].[SysProducts].[ManufacturerId] "
                                        + "AND[dbo].[UnitTypes].[Id] = [dbo].[SysProducts].[UnitTypeId] "
                                        + "AND[dbo].[SysProducts].[Id] = {0}";
                                ProductSearchResult data = db.Database.SqlQuery<ProductSearchResult>(query, productId).FirstOrDefault();
                                model.SelectedProduct.Add(data);
                            }
                        }
                        
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
                                model.Message = "Bạn đã chọn tất cả sản phẩm thuộc danh mục này. Xin hãy chọn sản phẩm khác";
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

            var userId = User.Identity.GetUserId<int>();
            var user = await UserManager.FindByIdAsync(userId);
            var model2 = new CreateRequestViewModel();
            model2.CustomerName = user.Fullname;
            model2.Email = user.Email;
            model2.Address = user.Address;
            model2.Phone = user.PhoneNumber;
            model2.Flag = mode;
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
            model2.MaxYearInput = db.Settings.Where(s => s.SettingName == "MaxYearInput").FirstOrDefault().SettingValue;

            var numOfUnseen = db.Notifications.Where(n => n.UserId == userId && n.IsSeen == false).Count();
            Session["UnSeenNoti"] = numOfUnseen;

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
        public async Task<ActionResult> Create(CreateRequestViewModel model, string[] selectedProduct, int[] quantity, int sum)
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

                    if (model.Flag == "bid" && sum < minTotal)
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

                    var userId = User.Identity.GetUserId<int>();
                    var user = await UserManager.FindByIdAsync(userId);

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
                        Flag = 0,
                        StatusId = 1, //Init
                    };

                    if (model.Flag == "bid")
                    {
                        request.Flag = 1; //set flag = 1: bid request
                    }


                    //var uploadImage = model.ImageUrl;

                    //if (uploadImage != null && uploadImage.ContentLength > 0)
                    //{
                    //    var fileName = Path.GetFileName(uploadImage.FileName);

                    //    fileName = Guid.NewGuid().ToString() + fileName;
                         
                    //    var filePath = Path.Combine(Server.MapPath("~/Images/Request"), fileName);

                    //    if (System.IO.File.Exists(filePath))
                    //    {
                    //        return new JsonResult
                    //        {
                    //            Data = new
                    //            {
                    //                Success = "Fail",
                    //                ErrorType = "Image",
                    //            },
                    //            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //        };
                    //    }
                    //    else
                    //    {
                    //        uploadImage.SaveAs(filePath);
                    //        request.Image = fileName;
                    //    }
                    //}

                    db.Requests.Add(request);
                    db.SaveChanges();

                    //Add doc to index
                    string listProduct = "";
                    var luceneRequest = new LuceneRequest
                    {
                        Id = request.Id,
                        Avatar = user.Avatar,
                        //Image = request.Image,
                        CustomerName = user.Fullname,
                        Title = model.Title,
                        StartDate = request.StartDate,
                        DueDate = request.DueDate,
                        Flag = (int)request.Flag
                    };

                    for (int i = 0; i < model.RequestProducts.Count; i++)
                    {
                        if (i == (model.RequestProducts.Count - 1))
                        {
                            listProduct = listProduct + model.RequestProducts[i].RequestedProduct.Name;
                        }
                        else
                        {
                            listProduct = listProduct + model.RequestProducts[i].RequestedProduct.Name + ", ";
                        }

                        var requestProduct = new RequestProduct
                        {
                            RequestId = request.Id,
                            SysProductId = model.RequestProducts[i].RequestedProduct.Id,
                            Quantity = model.RequestProducts[i].Quantity,
                        };
                        db.RequestProducts.Add(requestProduct);
                    }
                    luceneRequest.ListProduct = listProduct;
                    LuceneSimilar.AddUpdateLuceneIndex(luceneRequest);

                    if (model.Flag == "bid")
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

                    var numOfUnseen = db.Notifications.Where(n => n.UserId == userId && n.IsSeen == false).Count();
                    Session["UnSeenNoti"] = numOfUnseen;

                    //return RedirectToAction("Index");
                    var seoUrl = new HomeController().GenerateSlug(request.Title, request.Id);

                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = "Success",
                            RequestId = request.Id,
                            SeoUrl = seoUrl,
                            Message = "Đơn yêu cầu thành công!",
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };

                    //return JavaScript("window.location = '" + Url.Action("Details", "Request", new { id = request.Id }) + "'");
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

            //Request tương tự
            var similarquery = "SELECT [dbo].[SysProducts].[Name] "
                        + "FROM[dbo].[Requests], [dbo].[RequestProducts], [dbo].[SysProducts] "
                        + "WHERE[dbo].[Requests].[Id] = [dbo].[RequestProducts].[RequestId] "
                        + "AND[dbo].[RequestProducts].[SysProductId] = [dbo].[SysProducts].[Id] "
                        + "AND[dbo].[Requests].[Id] = {0}";
            var listProductRaw = db.Database.SqlQuery<ReplyProducts>(similarquery, id).ToList();

            string listProduct = "";
            for (int i = 0; i < listProductRaw.Count; i++)
            {
                if (i == (listProductRaw.Count - 1))
                {
                    listProduct = listProduct + listProductRaw[i].Name;
                }
                else
                {
                    listProduct = listProduct + listProductRaw[i].Name + ",";
                }
            }
            var searchResult = new LuceneRequestResult();
            var hit_limit = 3;

            searchResult = LuceneSimilar.SearchDefault(listProduct, hit_limit);
            foreach (var item in searchResult.SimilarResult)
            {
                var requestId = item.Id;
                item.Slug = new HomeController().GenerateSlug(item.Title, item.Id);
                item.NumReplies = db.Replies.Where(r => r.RequestId == requestId).Count();
                if (item.Flag == 1)
                {
                    item.DueDateCountdown = item.DueDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }
            }
            ViewBag.SimilarRequest = searchResult;

            if (request.Flag == 0)
            {
                //normal request
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    //Normar request
                    var reply = db.Replies
                                        .Where(r => r.SupplierId == userId && r.RequestId == id);

                    var requestId = (int)id;

                    ViewBag.Repliable = reply.Count() == 1 ? false : true;

                    var query = "SELECT [dbo].[Requests].[CustomerId], [dbo].[Users].[Id] AS SupplierId, [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[Description], [dbo].[Replies].[CreatedDate], [dbo].[Replies].[Id] "
                        + "FROM [dbo].[Replies], [dbo].[Users], [dbo].[Requests] "
                        + "WHERE [dbo].[Replies].[RequestId] = {0} "
                        + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                        + "AND [dbo].[Requests].[Id] = [dbo].[Replies].[RequestId] "
                        + "ORDER BY CASE WHEN [dbo].[Replies].[SupplierId] = {1} THEN 0 else 1 END, [dbo].[Replies].[CreatedDate] DESC ";
                    IEnumerable<BriefReply> data = db.Database.SqlQuery<BriefReply>(query, requestId, userId).ToList();
                    ViewBag.Replies = data;
                    ViewBag.AutoReplyId = 0;

                }
            }
            else if (request.Flag == 1) //Bid request
            {
                ViewBag.DueDateCountdown = request.DueDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                ViewBag.NumBidder = db.Replies.Where(r => r.RequestId == id && r.BidReply.Flag != 9).Count();

                if (User.Identity.IsAuthenticated)
                {
                    if (request.StatusId == 3)
                    {
                        var winnerReply = db.Replies.Where(r => r.RequestId == request.Id && (bool)r.Selected).FirstOrDefault();
                        ViewBag.Winner = winnerReply;
                        ViewBag.Retracted = false;
                        ViewBag.Finished = true;
                    } else
                    {
                        ViewBag.Finished = false;
                        var requestId = (int)id;

                        var userId = User.Identity.GetUserId<int>();

                        if (await UserManager.IsInRoleAsync(userId, "Customer") && userId == request.CustomerId)
                        {
                            var query = "";

                            if (request.StatusId == 2) //hoan tat
                            {
                                //View 5 top bid reply: cho customer sở hữu request đó
                                query = "SELECT TOP (5) [dbo].[BidReplies].[Rank], [dbo].[Replies].[SupplierId], [dbo].[Requests].[CustomerId], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], "
                                    + "[dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                                    + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users], [dbo].[Requests] "
                                    + "WHERE [dbo].[Replies].[RequestId] = {0} "
                                    + "AND [dbo].[Replies].[RequestId] = [dbo].[Requests].[Id]"
                                    + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                                    + "AND [dbo].[Replies].[Id] = [dbo].[BidReplies].[ReplyId] "
                                    + "AND [dbo].[BidReplies].[Flag] <> 9 "
                                    + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";

                            } else
                            {
                                //View all bid reply: cho customer sở hữu request đó
                                query = "SELECT [dbo].[BidReplies].[Rank], [dbo].[Replies].[SupplierId], [dbo].[Requests].[CustomerId], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], "
                                    + "[dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                                    + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users], [dbo].[Requests] "
                                    + "WHERE [dbo].[Replies].[RequestId] = {0} "
                                    + "AND [dbo].[Replies].[RequestId] = [dbo].[Requests].[Id]"
                                    + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                                    + "AND [dbo].[Replies].[Id] = [dbo].[BidReplies].[ReplyId] "
                                    + "AND [dbo].[BidReplies].[Flag] <> 9 "
                                    + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";
                            }

                            IEnumerable<BriefBidReply> data = db.Database.SqlQuery<BriefBidReply>(query, requestId).ToList();
                            ViewBag.BidReplies = data;
                            ViewBag.AutoReplyId = 0;

                            //setting config
                            var maxBan = db.Settings.Where(s => s.SettingName == "MaxBanned").FirstOrDefault().SettingValue;

                            var banNum = db.ListBanneds.Where(l => l.RequestId == request.Id).Count();
                            if (banNum < maxBan && request.StatusId == 1)
                            {
                                ViewBag.Banable = true; 
                            } else
                            {
                                ViewBag.Banable = false;
                            }
                        }
                        else if (await UserManager.IsInRoleAsync(userId, "Supplier"))
                        {
                            var reply = db.Replies
                                            .Where(r => r.SupplierId == userId && r.RequestId == id);

                            if (reply.Count() == 1) //current supplier đã reply
                            {
                                var min = reply.FirstOrDefault().BidReply.MinBidPrice;
                                var lowestPrice = reply.FirstOrDefault().Request.BidRequest.LowestPrice;
                                var currentPrice = reply.FirstOrDefault().Total;

                                //Kiểm tra có auto bid hay ko
                                if (min != null && min < lowestPrice && currentPrice > lowestPrice)
                                {
                                    ViewBag.AutoBid = true;
                                    ViewBag.AutoReplyId = reply.FirstOrDefault().Id;
                                }
                                else
                                {
                                    ViewBag.AutoReplyId = 0;
                                    ViewBag.AutoBid = false;
                                }

                                if (reply.FirstOrDefault().BidReply.Flag != 9)  //chưa rút thầu
                                {
                                    if (request.StatusId == 1)
                                    {
                                        var query = "SELECT [dbo].[BidReplies].[Rank], [dbo].[Replies].[SupplierId], [dbo].[Requests].[CustomerId], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], "
                                        + "[dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
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
                                    else if (request.StatusId == 2)
                                    {
                                        //View all bid reply: cho customer sở hữu request đó
                                        var query = "SELECT [dbo].[BidReplies].[Rank], [dbo].[Replies].[SupplierId], [dbo].[Requests].[CustomerId], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], "
                                            + "[dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                                            + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users], [dbo].[Requests] "
                                            + "WHERE [dbo].[Replies].[RequestId] = {0} "
                                            + "AND [dbo].[Replies].[RequestId] = [dbo].[Requests].[Id]"
                                            + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                                            + "AND [dbo].[Replies].[Id] = [dbo].[BidReplies].[ReplyId] "
                                            + "AND [dbo].[BidReplies].[Flag] <> 9 "
                                            + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";

                                        IEnumerable<BriefBidReply> data = db.Database.SqlQuery<BriefBidReply>(query, id).ToList();
                                        ViewBag.BidReplies = data;
                                    }

                                    ViewBag.Bidable = false;
                                    ViewBag.Retracted = false;

                                    var MaxDateRetract = db.Settings.Where(s => s.SettingName == "MaxDateRetract").FirstOrDefault().SettingValue;
                                    DateTime repCreateDate = (DateTime)reply.FirstOrDefault().CreatedDate;
                                    var max = repCreateDate.AddDays(MaxDateRetract);
                                    var today = DateTime.Now;

                                    if (max >= request.DueDate)
                                    {
                                        ViewBag.Retractable = false;
                                    }
                                    else
                                    {
                                        if (today >= max)
                                        {
                                            ViewBag.Retractable = false;
                                        }
                                        else
                                        {
                                            ViewBag.Retractable = true;
                                        }
                                    }
                                }
                                else
                                {
                                    ViewBag.Bidable = false;
                                    ViewBag.Retracted = true;
                                }
                            }
                            else
                            {
                                ViewBag.Bidable = true;
                                ViewBag.Retracted = false;
                            }

                        }
                    }
                    
                }

            }

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
                            + "AND [dbo].[BidReplies].[Flag] <> 9 "
                            + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";

                IEnumerable<BriefBidReply> list = db.Database.SqlQuery<BriefBidReply>(query, id).ToList();
             
                Session["LastUpdated"] = DateTime.Now;

                var maxBan = db.Settings.Where(s => s.SettingName == "MaxBanned").FirstOrDefault().SettingValue;

                var banNum = db.ListBanneds.Where(l => l.RequestId == id).Count();

                return new JsonResult {
                    Data = new {
                        ReplyType = "Bid",
                        BidReplies = list,
                        NumBidder = list.Count(),
                        Banable = banNum < maxBan ? "true" : "false"
                    }, JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
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
            var request = db.Requests.Find(reply.RequestId);
            var bidRequest = db.BidRequests.Find(request.Id);

            if (reply != null)
            {
                var bidreply = db.BidReplies.Where(r => r.ReplyId == reply.Id).FirstOrDefault();
                var rank = bidreply.Rank;
                var oldRank = bidreply.OldRank;
                var min = bidreply.MinBidPrice;
                var step = bidreply.Deduction;
                
                return new JsonResult { Data = new {
                    ReplyId = reply.Id,
                    Rank = rank,
                    OldRank = oldRank,
                    Min = min,
                    Step = step
                }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return new JsonResult { Data = new { Success = "FAIL" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        //Expired in Page
        public ActionResult Expired(int requestId)
        {
            var request = db.Requests.Find(requestId);
            //LuceneSimilar.ClearLuceneIndexRecord(requestId);

            if (request.StatusId == 1) //first person
            {
                request.StatusId = 2;
                db.Entry(request).State = EntityState.Modified;
            }
            db.SaveChanges();
            var currentUserId = User.Identity.GetUserId<int>();
            
            var reply = db.Replies.Where(r => r.SupplierId == currentUserId && r.RequestId == requestId);
            if (reply.Count() == 1 || currentUserId == request.CustomerId)//supplier có bid trong request này hoac la owner
            {
                var query = "SELECT TOP (5) [dbo].[BidReplies].[Rank], [dbo].[Replies].[SupplierId], [dbo].[Requests].[CustomerId], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], "
                            + "[dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                            + "FROM [dbo].[Replies], [dbo].[BidReplies], [dbo].[Users], [dbo].[Requests] "
                            + "WHERE [dbo].[Replies].[RequestId] = {0} "
                            + "AND [dbo].[Replies].[RequestId] = [dbo].[Requests].[Id]"
                            + "AND [dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND [dbo].[Replies].[Id] = [dbo].[BidReplies].[ReplyId] "
                            + "AND [dbo].[BidReplies].[Flag] <> 9 "
                            + "ORDER BY [dbo].[BidReplies].[Rank] ASC ";
                IEnumerable<BriefBidReply> data = db.Database.SqlQuery<BriefBidReply>(query, requestId).ToList();
                
                return new JsonResult
                {
                    Data = new
                    {
                        Message = "Success",
                        RequestId = requestId,
                        IsOwner = currentUserId == request.CustomerId ? true : false,
                        ListBid = data
                    },

                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return new JsonResult {
                Data = new {
                    Message = "Success",
                    RequestId = requestId,
                    IsGuest = true,
                },

                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //Bid request only
        public ActionResult ExpiredOutside()
        {
            var currentUserId = User.Identity.GetUserId<int>();
            var request = db.Requests.Where(r => r.CustomerId == currentUserId && r.StatusId == 2).OrderByDescending(r => r.DueDate).FirstOrDefault();
            if (request != null)
            {
                LuceneSimilar.ClearLuceneIndexRecord(request.Id);

                var message = "Yêu cầu \"" + request.Title + "\" của bạn vừa mới kết thúc 2!";
                var query = "SELECT [dbo].[UserRoles].[UserId] "
                            + "FROM[dbo].[UserRoles] "
                            + "WHERE[dbo].[UserRoles].[RoleId] = 1 ";
                var adminId = db.Database.SqlQuery<int>(query).FirstOrDefault();

                var notify = new Notification
                {
                    RequestId = request.Id,
                    UserId = currentUserId,
                    SenderId = adminId, //System Admin
                    Message = message,
                    CreatedDate = DateTime.Now,
                    IsSeen = false,
                    IsClicked = false,
                    Flag = 2, //expired
                };
                db.Notifications.Add(notify);

                var numOfUnseen = Session["UnSeenNoti"] == null ? 0 : (int)Session["UnSeenNoti"];
                numOfUnseen += 1;
                Session["UnSeenNoti"] = numOfUnseen;

                db.SaveChanges();

                //SignalR
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                //Call customer update noti count
                var customer = UserManager.FindById(request.CustomerId).UserName;
                notificationHub.Clients.User(customer).notify("added");

                return new JsonResult
                {
                    Data = new
                    {
                        Message = "Success",
                        RequestId = request.Id
                    },

                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            } else
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Message = "Request chuyển trạng thái khác",
                    },

                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
                
            
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
                Flag = (int)reply.Flag,
                RequestId = reply.RequestId
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
            + "WHERE[dbo].[Requests].[Flag] = 1 AND [dbo].[Requests].[CustomerId] = [dbo].[Users].[Id] AND [dbo].[Requests].[StatusId] = 1 ";
            IList<RequestFloorModel> data = db.Database.SqlQuery<RequestFloorModel>(query).ToList();
            foreach (var request in data)
            {
                request.Slug = new HomeController().GenerateSlug(request.Title, request.Id);
            }

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
            + "WHERE[dbo].[Requests].[Flag] = 0 AND [dbo].[Requests].[CustomerId] = [dbo].[Users].[Id] AND [dbo].[Requests].[StatusId] = 1";
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


        public ActionResult UpdateNumOfUnseen()
        {
            var numOfUnseen = Session["UnSeenNoti"] == null ? 0 : (int)Session["UnSeenNoti"];
            numOfUnseen += 1;
            Session["UnSeenNoti"] = numOfUnseen;
            return new JsonResult
            {
                Data = new
                {
                    Message = "Success"
                },

                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        
        //GET
        public ActionResult Cancel(int requestId)
        {
            var model = new CancelRequest {
                RequestId = requestId
            };
            return PartialView("_CancelRequest", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancel(CancelRequest model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());

                var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress("thegioivatlieu123@gmail.com"));
                message.From = new MailAddress(user.Email);  
                message.Subject = "Your email subject";
                message.Body = string.Format(body, user.Fullname, user.Email, model.Reason);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "thegioivatlieu123@gmail.com", 
                        Password = "capstoneproject123" 
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                    return RedirectToAction("Index");
                }
            }

            return new JsonResult
            {
                Data = new
                {
                    Message = "Success"
                },

                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //GET
        public ActionResult Ban(int replyId)
        {
            var requestId = db.Replies.Find(replyId).RequestId;

            var maxBan = db.Settings.Where(s => s.SettingName == "MaxBanned").FirstOrDefault().SettingValue;

            var banNum = db.ListBanneds.Where(l => l.RequestId == requestId).Count();
            if (banNum < maxBan)
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Message = "Success"
                    },

                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Message = "Fail"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        public ActionResult GetNumBidder(int requestId)
        {
            var numBidder = db.Replies.Where(r => r.RequestId == requestId && r.BidReply.Flag != 9).Count();

            return new JsonResult
            {
                Data = new
                {
                    Message = "Success",
                    NumBidder = numBidder
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
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