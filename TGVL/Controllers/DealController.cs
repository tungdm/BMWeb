using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using System.Globalization;


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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deal deal = db.Deals.Find(id);

            var model = new DealDetailsViewModel
            {
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
                CustomerQuantity = 1
            };
            //var test = deal.DueDate.ToString("0:yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (model.ProductDetails == null)
            {
                model.Product = deal.Product.SysProduct;
            }
            return View(model);
        }

        //POST
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

    }
}