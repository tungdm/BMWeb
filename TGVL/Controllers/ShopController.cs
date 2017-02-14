using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;

namespace TGVL.Controllers
{
    [Authorize(Roles="Supplier")]
    public class ShopController : Controller
    {
        private BMWEntities db = new BMWEntities();
        private ApplicationUserManager _userManager;

        public ShopController()
        {
        }

        public ShopController(ApplicationUserManager userManager)
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


        // GET: Shop
        public async Task<ActionResult> Index()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            var model = new IndexShopViewModel
            {
                Username = user.UserName
            };
            return View(model);
        }
    }
}