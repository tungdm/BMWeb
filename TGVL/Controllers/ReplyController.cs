using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;

namespace TGVL.Controllers
{
    public class ReplyController : Controller
    {
        private BMWEntities db = new BMWEntities();
        private ApplicationUserManager _userManager;

        public ReplyController()
        {
        }

        public ReplyController(ApplicationUserManager userManager)
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


        // GET: Reply
        public ActionResult Create(int requestId)
        {
            var model = new ReplyViewModel();

            Request request = db.Requests.Find(requestId);

            var supplierID = User.Identity.GetUserId<int>();
            var warehouse = db.Warehouses.Where(w => w.SupplierId == supplierID);

            //Kiểm tra supplier có warehouse hay k
            if (warehouse == null || !warehouse.Any())
            {
                model.Message = "Chưa có cửa hàng";
                return PartialView("_Response", model);
            }

            var requestedProduct = request.RequestProducts.ToList();

            var listRP = new List<int>();
            foreach (var p in requestedProduct)
            {
                listRP.Add(p.SysProductId);
            }

            if (requestedProduct != null)
            {
                if (requestedProduct.Count() == 0)
                {
                    //Customer không nêu rõ sản phẩm nào
                    //Supplier tự chọn sản phẩm
                }
                else
                {
                    //Kiểm tra xem supplier có đủ điều kiện về sản phẩm hay không
                    var supProducts = db.Products.Where(p => p.SupplierId == supplierID).ToList();
                    var listSP = new List<int>();
                    foreach (var p in supProducts)
                    {
                        listSP.Add(p.SysProductId);
                    }

                    if (listSP.Intersect(listRP).Count() == listRP.Count())
                    {
                        //Supplier đủ sản phẩm yêu cầu

                        var test2 = new List<ReplyProductViewModel>();

                        string query = "SELECT [dbo].[Products].[Id], [dbo].[Products].[UnitPrice], [dbo].[Products].[Image], [dbo].[SysProducts].[Name], [dbo].[UnitTypes].[Type] "
                            + "FROM [dbo].[Products], [dbo].[SysProducts], [dbo].[UnitTypes] "
                            + "WHERE [dbo].[Products].[SysProductId] = [dbo].[SysProducts].[Id] "
                            + "AND [dbo].[SysProducts].[UnitTypeId] = [dbo].[UnitTypes].[Id] "
                            + "AND [dbo].[Products].[SupplierId] = {0} "
                            + "AND [dbo].[Products].[SysProductId] = {1} ";
                            

                        for (int i = 0; i < listRP.Count(); i++)
                        {
                            var rp = listRP[i];

                            WarehouseProductViewModel data = db.Database.SqlQuery<WarehouseProductViewModel>(query, supplierID, rp).SingleOrDefault();


                            if (data != null)
                            {
                                var replyProduct = new ReplyProductViewModel();
                                replyProduct.Product = data;
                                replyProduct.Quantity = requestedProduct[i].Quantity;
                                test2.Add(replyProduct);
                            }
                        }

                        model.ReplyProducts = test2;
                        return PartialView("_Response", model);
                    }
                    else
                    {
                        //Supplier k đủ sp yêu cầu
                        model.Message = "Bạn không đủ sản phẩm để phản hồi yêu cầu này.";
                        return PartialView("_Response", model);
                    }
                }
            }

            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Create(ReplyViewModel model, int requestId, int[] replyProduct, int[] quantity, decimal[] price)
        {
            if (ModelState.IsValid)
            {
                var reply = new Reply
                {
                    RequestId = requestId,
                    SupplierId = User.Identity.GetUserId<int>(),
                    Description = model.Description,
                    Total = model.Total,
                    CreatedDate = DateTime.Now
                };
                db.Replies.Add(reply);

                if (replyProduct != null && quantity != null)
                {
                    for (int i = 0; i < quantity.Count(); i++)
                    {
                        if (quantity[i] == 0)
                        {
                            continue;
                        }

                        var rp = new ReplyProduct
                        {
                            ReplyId = reply.Id,
                            ProductId = replyProduct[i],
                            Quantity = quantity[i],
                            Price = price[i]
                        };
                        db.ReplyProducts.Add(rp);
                    }
                }
                
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                var request = db.Requests.Find(requestId);

                var notify = new Notification
                {
                    ReplyId = reply.Id,
                    UserId = request.CustomerId,
                    CreatedDate = reply.CreatedDate,
                    IsSeen = false
                };
                db.Notifications.Add(notify);

                db.SaveChanges();
                return new JsonResult { Data = new { replyId = reply.Id, name = user.Fullname, total = model.Total, address = user.Address, description = model.Description, avatar = user.Avatar }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return View();
        }
    }
}