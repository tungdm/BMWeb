using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using System.Web.Script.Serialization;
using System.IO;
using TGVL.LucenceSearch;
using System.Data.Entity;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace TGVL.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private BMWEntities db = new BMWEntities();
        private ApplicationUserManager _userManager;
        private static string[] VietNamChar = new string[]
        {
           "aAeEoOuUiIdDyY",
           "áàạảãâấầậẩẫăắằặẳẵ",
           "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
           "éèẹẻẽêếềệểễ",
           "ÉÈẸẺẼÊẾỀỆỂỄ",
           "óòọỏõôốồộổỗơớờợởỡ",
           "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
           "úùụủũưứừựửữ",
           "ÚÙỤỦŨƯỨỪỰỬỮ",
           "íìịỉĩ",
           "ÍÌỊỈĨ",
           "đ",
           "Đ",
           "ýỳỵỷỹ",
           "ÝỲỴỶỸ"
        };

        public HomeController()
        {
        }
        public HomeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
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
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult ProductAutocomplete(string term)
        {
            if (term != null)
            {
                var model = db.SysProducts
                .Where(p => p.Name.Contains(term))
                .Take(10)
                .Select(p => new
                {
                    label = p.Name
                });
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("AddProduct");
        }

        public ActionResult Search(string searchString)
        {
            //TODO: validate client-side
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                return RedirectToAction("SearchResult", new { searchString });
            }

            return new EmptyResult();
        }

        public ActionResult SearchResult(string searchString)
        {
            var searchResult = new LuceneResult();
            if (searchString.Length == 1)
            {
                searchResult = GoLucene.Search(searchString);
            }
            else
            {
                searchResult = GoLucene.SearchDefault(searchString);
            }

            if (searchResult.SearchResult.Count() == 1)
            {
                var sysProductId = searchResult.SearchResult.FirstOrDefault().Id;
                CreateMap(sysProductId); //create recomendation map shop
                return RedirectToAction("ViewDetail", new { id = sysProductId, searchString = searchString });
            }
            var model = new LuceneSearch
            {
                LuceneResult = searchResult,
                SearchString = searchString
            };
            var userId = User.Identity.GetUserId<int>();

            return View(model);
        }

        public JsonResult CreateMapFromAjax(int sysProductId, string sysProductName)
        {
            CreateMap(sysProductId);
            var seoUrl = GenerateSlug(sysProductName, sysProductId);

            return new JsonResult
            {
                Data = new
                {
                    Message = "Success",
                    SysProductId = sysProductId,
                    SeoUrl = seoUrl
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public void CreateMap(int sysProductId)
        {
            string _Json = string.Empty;
            string _path = string.Empty;

            var query = "SELECT [dbo].[Warehouses].[Id] AS id, [dbo].[Users].[Fullname] AS name, [dbo].[Warehouses].[Lat] AS lat, "
                + "[dbo].[Warehouses].[Lng] as lng, [dbo].[Warehouses].[Address] AS address, [dbo].[Warehouses].[Administrative_area_level_1] AS city, "
                + "[dbo].[Users].[PhoneNumber] AS phone, [dbo].[Users].[AverageGrade] as rating, [dbo].[Products].[UnitPrice] as price, "
                + "[dbo].[Products].[Id] AS productId "
                + "FROM [dbo].[SysProducts], [dbo].[Products], [dbo].[WarehouseProducts], [dbo].[Warehouses], [dbo].[Users] "
                + "WHERE [dbo].[Products].[SysProductId] = [dbo].[SysProducts].[Id] "
                + "AND [dbo].[Products].[Id] =  [dbo].[WarehouseProducts].[ProductId] "
                + "AND [dbo].[WarehouseProducts].[WarehouseId] = [dbo].[Warehouses].[Id] "
                + "AND [dbo].[Warehouses].[SupplierId] = [dbo].[Users].[Id] "
                + "AND [dbo].[SysProducts].[Id] = {0} "
                + "ORDER BY rating DESC";

            List<Shop> data = db.Database.SqlQuery<Shop>(query, sysProductId).ToList();

            _Json = new JavaScriptSerializer().Serialize(data);

            _path = Server.MapPath("~/Store/");

            string filename = (string)Session["FilenameJson"];


            if (filename == null)
            {
                Guid g = Guid.NewGuid();

                filename = Convert.ToBase64String(g.ToByteArray());
                filename = filename.Replace("=", "");
                filename = filename.Replace("+", "");
                filename = filename.Replace("/", "");
                filename = filename.Replace("\\", "");

                Session["FilenameJson"] = filename;
            }
            System.IO.File.WriteAllText(_path + filename + "_.json", _Json);

        }


        public ActionResult CreateIndex()
        {
            var query = "SELECT [dbo].[SysProducts].[Id], [dbo].[SysProducts].[Name], [dbo].[SysProducts].[Image], "
                + "[dbo].[SysProducts].[Description], [dbo].[SysProducts].[UnitPrice], [dbo].[Manufacturers].[Name] AS ManufactureName, "
                + "[dbo].[UnitTypes].[Type] AS UnitType "
                + "FROM[dbo].[SysProducts], [dbo].[Manufacturers], [dbo].[UnitTypes] "
                + "WHERE[dbo].[Manufacturers].[Id] = [dbo].[SysProducts].[ManufacturerId] "
                + "AND[dbo].[UnitTypes].[Id] = [dbo].[SysProducts].[UnitTypeId]";

            List<ProductSearchResult> allData = db.Database.SqlQuery<ProductSearchResult>(query).ToList();

            GoLucene.AddUpdateLuceneIndex(allData);
            TempData["Result"] = "Search index was created successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult CreateIndexRequest()
        {
            var allRequest = db.Requests;
            var list = new List<LuceneRequest>();

            foreach (var request in allRequest)
            {
                var luceneRequest = new LuceneRequest();
                var requestId = request.Id;
                luceneRequest.Id = requestId;
                luceneRequest.Avatar = request.User.Avatar;
                luceneRequest.Title = request.Title;
                luceneRequest.CustomerName = request.User.Fullname;
                luceneRequest.StartDate = request.StartDate;
                luceneRequest.DueDate = request.DueDate;
                luceneRequest.Flag = (int)request.Flag;
                luceneRequest.Image = request.Image;

                var query = "SELECT [dbo].[SysProducts].[Name] "
                            + "FROM[dbo].[Requests], [dbo].[RequestProducts], [dbo].[SysProducts] "
                            + "WHERE[dbo].[Requests].[Id] = [dbo].[RequestProducts].[RequestId] "
                            + "AND[dbo].[RequestProducts].[SysProductId] = [dbo].[SysProducts].[Id] "
                            + "AND[dbo].[Requests].[Id] = {0}";
                var listProductRaw = db.Database.SqlQuery<ReplyProducts>(query, requestId).ToList();

                string listProduct = "";
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
                luceneRequest.ListProduct = listProduct;
                list.Add(luceneRequest);
            }
            LuceneSimilar.AddUpdateLuceneIndex(list);
            TempData["Result"] = "Search index was created successfully!";
            return RedirectToAction("Index");
        }

        [Authorize]
        public JsonResult GetNotificationReplies()
        {
            int userId = User.Identity.GetUserId<int>();

            var query = "SELECT [dbo].[Notifications].[Id], [dbo].[Notifications].[RequestId], [dbo].[Notifications].[ReplyId], [dbo].[Notifications].[CreatedDate], "
                    + "[dbo].[Notifications].[Message], [dbo].[Notifications].[IsSeen], [dbo].[Notifications].[IsClicked], [dbo].[Users].[Fullname] "
                    + "FROM[dbo].[Notifications], [dbo].[Users] "
                    + "WHERE[dbo].[Notifications].[UserId] = [dbo].[Users].[Id] "
                    + "AND[dbo].[Notifications].[UserId] = {0} "
                    + "ORDER BY CASE WHEN[dbo].[Notifications].[IsSeen] = 'False' THEN 0 else 1 END, [dbo].[Notifications].[CreatedDate] DESC ";

            IEnumerable<MyNotification> data = db.Database.SqlQuery<MyNotification>(query, userId).ToList();

            query = "UPDATE [dbo].[Notifications] "
                    + "SET[dbo].[Notifications].[IsSeen] = 1 "
                    + "WHERE[dbo].[Notifications].[UserId] = {0} ";
            db.Database.ExecuteSqlCommand(query, userId);

            Session.Clear();

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        [Authorize]
        public async System.Threading.Tasks.Task<ActionResult> DeliveryAddress(ShoppingCart model)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());

            var cartSession = (CartViewModel)Session["Cart"];

            if (cartSession != null)
            {
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
                            SupplierId = deal.SupplierId,
                            Image = deal.Product.Image,
                            ProductName = deal.Product.SysProduct.Name,
                            Quantity = c.Quantity,
                            UnitPrice = Math.Ceiling(deal.UnitPrice - (deal.UnitPrice * deal.Discount / 100)),
                            UnitType = deal.Product.SysProduct.UnitType.Type,
                            MiniTotal = Math.Ceiling(deal.UnitPrice - (deal.UnitPrice * deal.Discount / 100)) * c.Quantity
                        };
                        model.ShoppingCartProducts.Add(scProducts);
                        total += scProducts.MiniTotal;
                    }
                    else
                    {
                        //muangay
                        var product = db.Products.Find(c.ProductId);

                        var scProducts = new ShoppingCartProducts
                        {
                            ProductId = product.Id,
                            Type = "muangay",
                            SupplierId = product.SupplierId,
                            SupplierName = product.User.Fullname,
                            Image = product.Image,
                            ProductName = product.SysProduct.Name,
                            Quantity = c.Quantity,
                            UnitPrice = (decimal)product.UnitPrice,
                            UnitType = product.SysProduct.UnitType.Type,
                            MiniTotal = (decimal)product.UnitPrice * c.Quantity
                        };
                        model.ShoppingCartProducts.Add(scProducts);
                        total += scProducts.MiniTotal;
                    }
                }
                model.Total = total;
            }

            var order = new OrderViewModel
            {
                CustomerId = user.Id,
                CustomerFullName = user.Fullname,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                ShoppingCart = model
            };
            Session["Order"] = order;
            return View(order);
        }

        public ActionResult Muangay(int productId)
        {
            var product = db.Products.Find(productId);
            var model = new MuaNgayViewModel
            {
                Id = productId,
                UnitPrice = (decimal)product.UnitPrice,
                UnitType = product.SysProduct.UnitType.Type,
                ProductName = product.SysProduct.Name,
                Quantity = 1
            };
            ViewBag.MaxLengthInputNumberSmall = db.Settings.Where(s => s.SettingName == "MaxLengthInputNumberSmall").FirstOrDefault().SettingValue;
            return PartialView("_Muangay", model);
        }



        public ActionResult ShoppingCart()
        {
            var cartSession = (CartViewModel)Session["Cart"];
            var model = new ShoppingCart();
            if (cartSession != null)
            {
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
                            SupplierId = deal.SupplierId,
                            SupplierName = deal.User.Fullname,
                            Image = deal.Product.Image,
                            ProductName = deal.Product.SysProduct.Name,
                            Quantity = c.Quantity,
                            UnitPrice = Math.Ceiling(deal.UnitPrice - (deal.UnitPrice * deal.Discount / 100)),
                            UnitType = deal.Product.SysProduct.UnitType.Type,
                            MiniTotal = Math.Ceiling(deal.UnitPrice - (deal.UnitPrice * deal.Discount / 100)) * c.Quantity
                        };
                        model.ShoppingCartProducts.Add(scProducts);
                        total += scProducts.MiniTotal;
                    }
                    else
                    {
                        //muangay
                        var product = db.Products.Find(c.ProductId);

                        var scProducts = new ShoppingCartProducts
                        {
                            ProductId = product.Id,
                            Type = "muangay",
                            SupplierId = product.SupplierId,
                            SupplierName = product.User.Fullname,
                            Image = product.Image,
                            ProductName = product.SysProduct.Name,
                            Quantity = c.Quantity,
                            UnitPrice = (decimal)product.UnitPrice,
                            UnitType = product.SysProduct.UnitType.Type,
                            MiniTotal = (decimal)product.UnitPrice * c.Quantity
                        };
                        model.ShoppingCartProducts.Add(scProducts);
                        total += scProducts.MiniTotal;
                    }
                }
                model.Total = total;
                model.ShoppingCartProducts.OrderBy(c => c.ProductName);
                model.MaxLengthInputNumberSmall = db.Settings.Where(s => s.SettingName == "MaxLengthInputNumberSmall").FirstOrDefault().SettingValue;
                return View(model);
            }
            return View(model);
        }

        public JsonResult UpdateQuantity(string type, int id, int newQty)
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
                        check.Quantity = newQty;
                        cartSession.CartDetails.Add(check);
                    }
                }
                else
                {
                    var check = cartSession.CartDetails.Where(c => c.ProductId == id).FirstOrDefault();
                    if (check != null)
                    {
                        cartSession.CartDetails.Remove(check);
                        check.Quantity = newQty;
                        cartSession.CartDetails.Add(check);
                    }
                }


                Session["Cart"] = cartSession; //save vao session

                return new JsonResult
                {
                    Data = new
                    {
                        Success = "Success"
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
                }
                else
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
                        RemoveElement = type + "_" + id,
                        Count = cartSession.CartDetails.Count()
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

        public async System.Threading.Tasks.Task<ActionResult> UpdateInfo(OrderViewModel model)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());

            if (user != null)
            {
                user.Fullname = model.CustomerFullName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;

                await UserManager.UpdateAsync(user);
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }

            return PartialView("_UpdateAddress", model);

        }

        public async System.Threading.Tasks.Task<ActionResult> UpdateExistedAddress(string type)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            var model = new OrderViewModel
            {
                CustomerFullName = user.Fullname,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };
            return PartialView("_AddNewAddress", model);

        }

        [Authorize]
        public async System.Threading.Tasks.Task<ActionResult> CheckOut(int? replyId)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            var order = new OrderViewModel();

            if (replyId != null)
            {
                order.Reply = db.Replies.Find(replyId);
                order.IsRequestOrder = true;
                return new JsonResult
                {
                    Data = new
                    {
                        Success = "Success",
                        Message = "Đơn yêu cầu thành công!",
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

            } else
            {
                order = (OrderViewModel)Session["Order"];
                order.IsRequestOrder = false;
            }

            order.CustomerFullName = user.Fullname;
            order.Address = user.Address;
            order.PhoneNumber = user.PhoneNumber;
            order.AllTypeOfPayments = db.Payments;
            return View(order);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page";

            return View();
        }



        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Index()
        {
            var model = new HydridViewModel();
           
            var newListHotdeal = new List<DealBriefViewModel>();
            var newListNewdeal = new List<DealBriefViewModel>();
            var newListHotshop = new List<HotShopViewModel>();

            var HotShop = db.Users.Where(d => d.Flag == 1).OrderByDescending(d => d.AverageGrade).ToList();

            foreach (var hs in HotShop)
            {
                var hsmodel = new HotShopViewModel
                {
                    ShopId = hs.Id,
                    Avatar = hs.Avatar,
                    Rating = (float)hs.AverageGrade,
                    ShopName = hs.Fullname,
                    Address = hs.Address
                };
                newListHotshop.Add(hsmodel);
            }


            var HotDeal = db.Deals.Where(d => d.Expired == false).OrderByDescending(d => d.NumBuyer).ToList();

            foreach (var hd in HotDeal)
            {
                var hdmodel = new DealBriefViewModel
                {
                    Id = hd.Id,
                    Title = hd.Title,
                    UnitPrice = hd.UnitPrice,
                    Discount = hd.Discount,
                    SavePrice = Math.Ceiling(hd.UnitPrice - (hd.UnitPrice * hd.Discount / 100)),
                    Image = hd.Product.Image,
                    NumBuyer = hd.NumBuyer,

                };
                newListHotdeal.Add(hdmodel);
            }

            var NewDeal = db.Deals.Where(d => d.Expired == false).OrderByDescending(d => d.CreatedDate).ToList();

            foreach (var nd in HotDeal)
            {
                var ndmodel = new DealBriefViewModel
                {
                    Id = nd.Id,
                    Title = nd.Title,
                    UnitPrice = nd.UnitPrice,
                    Discount = nd.Discount,
                    SavePrice = Math.Ceiling(nd.UnitPrice - (nd.UnitPrice * nd.Discount / 100)),
                    Image = nd.Product.Image,
                    NumBuyer = nd.NumBuyer,
                };

                newListNewdeal.Add(ndmodel);
            }
            model.Hotshop = newListHotshop;
            model.Hotdeal = newListHotdeal;
            model.Newdeal = newListNewdeal;
            return View(model);
        }

 

        public ActionResult ViewDetail(int id, string searchString)
        {
            var product = db.SysProducts.Find(id);
            var productId = product.Id;

            //var TopRecShop = db.Settings.Where(s => s.SettingName == "TopRecShop").FirstOrDefault().SettingValue;

            var query = "SELECT [dbo].[Warehouses].[Id] AS id, [dbo].[Users].[Fullname] AS name, [dbo].[Warehouses].[Lat] AS lat, "
                + "[dbo].[Warehouses].[Lng] as lng, [dbo].[Warehouses].[Address] AS address, [dbo].[Warehouses].[Administrative_area_level_1] AS city, "
                + "[dbo].[Users].[PhoneNumber] AS phone, [dbo].[Users].[AverageGrade] as rating, [dbo].[Products].[UnitPrice] as price, "
                + "[dbo].[Products].[Id] AS productId "
                + "FROM [dbo].[SysProducts], [dbo].[Products], [dbo].[WarehouseProducts], [dbo].[Warehouses], [dbo].[Users] "
                + "WHERE [dbo].[Products].[SysProductId] = [dbo].[SysProducts].[Id] "
                + "AND [dbo].[Products].[Id] =  [dbo].[WarehouseProducts].[ProductId] "
                + "AND [dbo].[WarehouseProducts].[WarehouseId] = [dbo].[Warehouses].[Id] "
                + "AND [dbo].[Warehouses].[SupplierId] = [dbo].[Users].[Id] "
                + "AND [dbo].[SysProducts].[Id] = {0} "
                + "ORDER BY rating DESC";

            List<Shop> data = db.Database.SqlQuery<Shop>(query, id).ToList();

            //Sản phẩm tương tự
            var categoryId = product.SysCategoryId;
            query = "SELECT [dbo].[SysProducts].[Id], [dbo].[SysProducts].[Name], [dbo].[SysProducts].[UnitPrice], [dbo].[UnitTypes].[Type] AS UnitType, [dbo].[SysProducts].[Image] "
                  + "FROM[dbo].[SysProducts], [dbo].[UnitTypes] "
                  + "WHERE[dbo].[SysProducts].[UnitTypeId] = [dbo].[UnitTypes].[Id] "
                  + "AND[dbo].[SysProducts].[SysCategoryId] = {0} "
                  + "AND[dbo].[SysProducts].[Id] <> {1} ";
            List<SimiliarProduct> simiProducts = db.Database.SqlQuery<SimiliarProduct>(query, categoryId, productId).ToList();
            foreach (var pr in simiProducts)
            {
                pr.NumShops = db.Products.Where(p => p.SysProductId == pr.Id).Count();
            }

            var userId = User.Identity.GetUserId<int>();

            var model = new SearchResultViewModel
            {
                SysProduct = product,
                NumOfShops = data.Count(),
                ListShops = data,
                SimiliarProducts = simiProducts,
                SearchString = searchString == null ? product.Name : searchString
            };
            return View(model);
        }

        public ActionResult SearchRequestResult()
        {
            return View();
        }

        public ActionResult SearchShopResult()
        {
            return View();
        }

        public ActionResult EditDescription(int? id)
        {
            var product = db.SysProducts.Find(id);

            var model = new UpdateDescription
            {
                Id = product.Id,
                Description = product.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(MuaNgayViewModel model)
        {
            var data = new CartDetails
            {
                ProductId = model.Id,
                Type = "muangay",
                Quantity = model.Quantity
            };

            var cartSession = (CartViewModel)Session["Cart"];
            var count = 0;

            if (cartSession != null)
            {
                var check = cartSession.CartDetails.Where(c => c.ProductId == model.Id).FirstOrDefault();
                if (check != null)
                {
                    cartSession.CartDetails.Remove(check);
                    check.Quantity += model.Quantity;
                    cartSession.CartDetails.Add(check);
                }
                else
                {
                    cartSession.CartDetails.Add(data);
                }
                Session["Cart"] = cartSession; //save vao session
                count = cartSession.CartDetails.Count();
            }
            else
            {
                var cartmodel = new CartViewModel();
                cartmodel.CartDetails = new List<CartDetails>();

                cartmodel.CartDetails.Add(data);

                Session["Cart"] = cartmodel; //save vao session
                count = cartmodel.CartDetails.Count();
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = "Success",
                    Type = "Muangay",
                    Product = Session["Cart"],
                    Count = count
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public string GenerateSlug(string Title, int Id)
        {
            Title = Title.Replace("-", "");

            string phrase = string.Format("{0}-{1}", Id, Title);

            string str = ReplaceUnicode(phrase).ToLower();

            //str = RemoveDiacritics(phrase);
            
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 95 ? str.Length : 95).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        public string ReplaceUnicode(string strInput)
        {
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                {
                    strInput = strInput.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }
            }
            return strInput;
        }

        public string RemoveDiacritics(string text)
        {
            var noApostrophes = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(text));
            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(text);
            var test = Encoding.ASCII.GetString(bytes);

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDescription(UpdateDescription model)
        {
            var product = db.SysProducts.Find(model.Id);
            product.Description = model.Description;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();

            return View();
        }

        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Admin()
        {
            IEnumerable<Setting> listSetting = db.Settings.ToList();
            ViewBag.ListSetting = listSetting;

            return View();
        }

        [System.Web.Mvc.Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddConfig()
        {
            ViewBag.SettingTypeId = new SelectList(db.SettingTypes, "Id", "Type");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddConfig(Setting setting)
        {
            if (ModelState.IsValid)
            {
                db.Settings.Add(setting);
                db.SaveChanges();
                return RedirectToAction("Admin");
            }
            ViewBag.SettingTypeId = new SelectList(db.SettingTypes, "Id", "Type");
            return View(setting);
        }

        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult EditConfig(int? Id)
        {
            
            Setting setting = db.Settings.Find(Id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            ViewBag.SettingTypeId = new SelectList(db.SettingTypes, "Id", "Type");
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditConfig(Setting setting)
        {
            if (ModelState.IsValid)
            {
                db.Entry(setting).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Admin");
            }
            ViewBag.SettingTypeId = new SelectList(db.SettingTypes, "Id", "Type");
            return View(setting);
        }

        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult DeleteConfig(int? Id)
        {

            Setting setting = db.Settings.Find(Id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        [HttpPost, ActionName("DeleteConfig")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int Id)
        {
            Setting setting = db.Settings.Find(Id);
            db.Settings.Remove(setting);
            db.SaveChanges();
            return RedirectToAction("Admin");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // View HotDeal All
        public ActionResult ViewHotDeal()
        {
            var newListHotdeal = new List<DealBriefViewModel>();
            
            var HotDeal = db.Deals.Where(d => d.Expired == false).OrderByDescending(d => d.NumBuyer).ToList();

            foreach (var hd in HotDeal)
            {
                var hdmodel = new DealBriefViewModel
                {
                    Id = hd.Id,
                    Title = hd.Title,
                    UnitPrice = hd.UnitPrice,
                    Discount = hd.Discount,
                    SavePrice = Math.Ceiling(hd.UnitPrice - (hd.UnitPrice * hd.Discount / 100)),
                    Image = hd.Product.Image,
                    NumBuyer = hd.NumBuyer,

                };
                newListHotdeal.Add(hdmodel);
            }
     
            return View(newListHotdeal);
        }

        // View NewDeal All
        public ActionResult ViewNewDeal()
        {
            var newListNewdeal = new List<DealBriefViewModel>();
            
            var NewDeal = db.Deals.Where(d => d.Expired == false).OrderByDescending(d => d.CreatedDate).ToList();

            foreach (var nd in NewDeal)
            {
                var ndmodel = new DealBriefViewModel
                {
                    Id = nd.Id,
                    Title = nd.Title,
                    UnitPrice = nd.UnitPrice,
                    Discount = nd.Discount,
                    SavePrice = Math.Ceiling(nd.UnitPrice - (nd.UnitPrice * nd.Discount / 100)),
                    Image = nd.Product.Image,
                    NumBuyer = nd.NumBuyer,
                };

                newListNewdeal.Add(ndmodel);
            }
           
            return View(newListNewdeal);
        }

        // View HotShop All
        public ActionResult ViewHotShop()
        {
            var newListHotshop = new List<HotShopViewModel>();

            var HotShop = db.Users.Where(d => d.Flag == 1).OrderByDescending(d => d.AverageGrade).ToList();

            foreach (var hs in HotShop)
            {
                var hsmodel = new HotShopViewModel
                {
                    ShopId = hs.Id,
                    Avatar = hs.Avatar,
                    Rating = (float)hs.AverageGrade,
                    ShopName = hs.Fullname,
                    Address = hs.Address
                };
                newListHotshop.Add(hsmodel);
            }
            
            return View(newListHotshop);
        }
    }
}