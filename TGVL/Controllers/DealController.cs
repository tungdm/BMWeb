using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using System.Globalization;
using System.Collections;
using System.Data.Entity;

namespace TGVL.Controllers
{
    public class DealController : Controller
    {
        private BMWEntities db = new BMWEntities();
        private ApplicationUserManager _userManager;

        public DealController()
        {
        }

        public DealController(ApplicationUserManager userManager)
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

        public object GetFromConfig { get; private set; }

        // GET: Deal
        public ActionResult Index()
        {
            return View();
        }

        // GET: Deal
        public ActionResult Create()
        {
            return View();
        }

        //TungDM
        // GET: Deal/Details/1
        public ActionResult Details(int? id)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           
            Deal deal = db.Deals.Find(id);
            
            var model = new DealDetailsViewModel
            {
                SupplierName = deal.User.Fullname,
                Id = deal.Id,
                Title = deal.Title,
                UnitPrice = deal.UnitPrice,
                UnitType = deal.Product.SysProduct.UnitType.Type,
                Discount = deal.Discount,
                PriceSave = Math.Ceiling(deal.UnitPrice - (deal.UnitPrice * deal.Discount / 100)),
                Quantity = deal.Quantity,
                DueDate = deal.DueDate,
                DueDateCountdown = deal.DueDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                ProductImage = deal.Product.Image,
                ProductDetails = deal.Product.Description,
                Description = deal.Description,
                ShortDescription = deal.ShortDescription,
                CustomerQuantity = 1,
                NumBuyer = deal.NumBuyer,
                Expired = deal.Expired
            };
            var shops = db.Warehouses.Where(w => w.SupplierId == deal.SupplierId).ToList();

            ViewBag.ListShop = shops;
            
            var ShopAddress = new ArrayList();

            var cnt = shops.Count;

            var result = new string[cnt];
            var infoWindowContent = new string[cnt];
            for (int i = 0; i< shops.Count; i++)
            {
                var place = "['" + shops[i].Address + "', " + shops[i].Lat + ", " + shops[i].Lng + "]";
                var info = "['<div class=\"info_content\"><h3>" + shops[i].Address + "</h3></div>']";
                result[i] = place;
                infoWindowContent[i] = info;
                
            }

            ViewBag.Result = result;
            ViewBag.Info = infoWindowContent;
            

            if (model.ProductDetails == null)
            {
                model.Product = deal.Product.SysProduct;
            }
            //Similar deal (same category)
            var categoryId = deal.Product.SysProduct.SysCategoryId;
            var similarDeals = db.Deals.Where(d => d.Product.SysProduct.SysCategoryId == categoryId && d.Id != id &&!d.Expired).OrderByDescending(d => d.NumBuyer).ToList();
            var simiDeals = new List<SimilarDeal>();
            
            foreach(var item in similarDeals)
            {
                var simiDeal = new SimilarDeal {
                    Id = item.Id,
                    Title = item.Title,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    PriceSave = Math.Ceiling(item.UnitPrice - (item.UnitPrice * item.Discount / 100)),
                    Image = item.Product.Image,
                    NumBuyer = item.NumBuyer,
                    DueDate = item.DueDate,
                    DueDateCountdown = item.DueDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                };
                simiDeals.Add(simiDeal);
            }
            model.SimilarDeals = simiDeals;

            //Similar deal (same supplier)
            var supplierId = deal.SupplierId;
            var sameSup = db.Deals.Where(d => d.SupplierId == supplierId && d.Id != id && !d.Expired).OrderByDescending(d => d.NumBuyer).ToList();
            var sameSupDeals = new List<SimilarDeal>();
            foreach (var item in sameSup)
            {
                var sameDeal = new SimilarDeal
                {
                    Id = item.Id,
                    Title = item.Title,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    PriceSave = Math.Ceiling(item.UnitPrice - (item.UnitPrice * item.Discount / 100)),
                    Image = item.Product.Image,
                    NumBuyer = item.NumBuyer,
                    DueDate = item.DueDate,
                    DueDateCountdown = item.DueDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                };
                sameSupDeals.Add(sameDeal);
            }
            model.SameSuppliers = sameSupDeals;
            return View(model);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(DealDetailsViewModel model)
        {
            var data = new CartDetails
            {
                DealId = model.Id,
                Type = "deal",
                Quantity = model.CustomerQuantity
            };
            
            var cartSession = (CartViewModel)Session["Cart"];
            var count = 0;

            if (cartSession != null)
            {
                var check = cartSession.CartDetails.Where(c => c.DealId == model.Id).FirstOrDefault();
                if (check != null)
                {
                    cartSession.CartDetails.Remove(check);
                    check.Quantity += model.CustomerQuantity;
                    cartSession.CartDetails.Add(check);
                } else
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
                    Product = Session["Cart"],
                    Count = count
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult Expired(int dealId)
        {
            var deal = db.Deals.Find(dealId);
            if (!deal.Expired)
            {
                deal.Expired = true;
                db.Entry(deal).State = EntityState.Modified;
                db.SaveChanges();
                return new JsonResult
                {
                    Data = new
                    {
                        Success = "Success",
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            } else
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = "Already expired",
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

    }
}