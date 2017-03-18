using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using System.Web.Script.Serialization;

namespace TGVL.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private BMWEntities db = new BMWEntities();
        private ApplicationUserManager _userManager;

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
                        //normal
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
                            Image = deal.Product.Image,
                            ProductName = deal.Product.SysProduct.Name,
                            Quantity = c.Quantity,
                            UnitPrice = Math.Ceiling(deal.UnitPrice - (deal.UnitPrice * deal.Discount / 100)),
                            UnitType = deal.Product.SysProduct.UnitType.Type,
                            MiniTotal = Math.Ceiling(deal.UnitPrice - (deal.UnitPrice * deal.Discount / 100)) * c.Quantity
                        };
                        model.ShoppingCartProducts.Add(scProducts);
                        total += scProducts.MiniTotal;
                    } else
                    {
                        //normal
                    }          
                }
                model.Total = total;
                model.ShoppingCartProducts.OrderBy(c => c.ProductName);
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
        public async System.Threading.Tasks.Task<ActionResult> CheckOut()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            var order = (OrderViewModel)Session["Order"];
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
            return RedirectToAction("AddProduct");
        }


        public ActionResult SearchProduct(string searchString)
        {
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                string _Json = string.Empty;
                string _path = string.Empty;
                var product = db.SysProducts.Where(p => p.Name == searchString).FirstOrDefault();
                var productId = product.Id;

                var query = "SELECT [dbo].[Warehouses].[Id] AS id, [dbo].[Users].[Fullname] AS name, [dbo].[Warehouses].[Lat] AS lat, "
                    + "[dbo].[Warehouses].[Lng] as lng, [dbo].[Warehouses].[Address] AS address, [dbo].[Warehouses].[Administrative_area_level_1] AS city, "
                    + "[dbo].[Users].[PhoneNumber] AS phone, [dbo].[Users].[AverageGrade] as rating "
                    + "FROM [dbo].[SysProducts], [dbo].[Products], [dbo].[WarehouseProducts], [dbo].[Warehouses], [dbo].[Users] "
                    + "WHERE [dbo].[Products].[SysProductId] = [dbo].[SysProducts].[Id] "
                    + "AND [dbo].[Products].[Id] =  [dbo].[WarehouseProducts].[ProductId] "
                    + "AND [dbo].[WarehouseProducts].[WarehouseId] = [dbo].[Warehouses].[Id] "
                    + "AND [dbo].[Warehouses].[SupplierId] = [dbo].[Users].[Id] "
                    + "AND [dbo].[SysProducts].[Id] = {0} ";

                List<Shop> data = db.Database.SqlQuery<Shop>(query, productId).ToList();

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

            

            return RedirectToAction("About");
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

    }
}