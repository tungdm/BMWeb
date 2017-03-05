using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Hubs;
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
                return new JsonResult
                {
                    Data = new
                    {
                        Message = "Chưa có cửa hàng"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

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

                        decimal? total = 0;

                        for (int i = 0; i < listRP.Count(); i++)
                        {
                            var rp = listRP[i];

                            WarehouseProductViewModel data = db.Database.SqlQuery<WarehouseProductViewModel>(query, supplierID, rp).SingleOrDefault();

                            if (data != null)
                            {
                                var replyProduct = new ReplyProductViewModel();
                                replyProduct.Product = data;
                                replyProduct.Quantity = requestedProduct[i].Quantity;
                                total += data.UnitPrice * requestedProduct[i].Quantity;
                                test2.Add(replyProduct);
                            }
                        }

                        model.ReplyProducts = test2;
                        model.DeliveryDate = request.ReceivingDate;
                        model.Total = (decimal)total;
                        return PartialView("_Response", model);
                    }
                    else
                    {
                        //Supplier k đủ sp yêu cầu
                        return new JsonResult
                        {
                            Data = new
                            {
                                Message = "Bạn không đủ sản phẩm để phản hồi yêu cầu này."
                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };

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
                var request = db.Requests.Find(requestId);

                var reply = new Reply
                {
                    RequestId = requestId,
                    SupplierId = User.Identity.GetUserId<int>(),
                    Description = model.Description,
                    Total = model.Total,
                    DeliveryDate = model.DeliveryDate,
                    CreatedDate = DateTime.Now,
                    Flag = request.Flag == 1 ? 1 : 0 //normal reply: flag = 0; bid reply: flag = 1
                };

                db.Replies.Add(reply);

                //Create bid reply
                if (request.Flag == 1)
                {
                    var bidReply = new BidReply
                    {
                        ReplyId = reply.Id,
                        Rank = -1,  //chưa update rank
                        CreatedDate = reply.CreatedDate,
                        Flag = 0
                    };
                    db.BidReplies.Add(bidReply);
                }

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

                var notify = new Notification
                {
                    ReplyId = reply.Id,
                    UserId = request.CustomerId,
                    CreatedDate = reply.CreatedDate,
                    IsSeen = false
                };
                db.Notifications.Add(notify);

                db.SaveChanges();

                if (request.Flag == 1)
                {
                    //Update rank
                    var query = "Update BidReplies "
                                + "SET [Rank] = t2.[Rank] "
                                + "FROM BidReplies t1 "
                                + "LEFT OUTER JOIN "
                                + "("
                                + "SELECT BidReplies.ReplyId, Replies.RequestId, Rank() OVER (PARTITION BY RequestId ORDER BY Total asc) as [Rank] "
                                + "FROM BidReplies, Replies "
                                + "WHERE BidReplies.ReplyId = Replies.Id "
                                + ") as t2 "
                                + "ON t1.ReplyId = t2.ReplyId "
                                + "AND t2.RequestId = {0} ";

                    db.Database.ExecuteSqlCommand(query, requestId);

                    //SignalR - Call another supplier update bid rank
                    var listUser = new List<string>();
                    var newestDate = reply.CreatedDate;

                    query   = "SELECT[dbo].[Users].[UserName] "
                            + "FROM[dbo].[Replies], [dbo].[Requests], [dbo].[Users] "
                            + "WHERE[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND[dbo].[Replies].[RequestId] = [dbo].[Requests].[Id] "
                            + "AND[dbo].[Requests].[Id] = {0} "
                            + "AND[dbo].[Replies].[CreatedDate] < {1}";
                    List<string> listUser2 = db.Database.SqlQuery<string>(query, requestId, newestDate).ToList();
                    if (listUser2.Count() != 0)
                    {
                        foreach (var u in listUser2)
                        {
                            listUser.Add(u);
                        }

                        var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                        foreach (var userId in listUser)
                        {
                            notificationHub.Clients.User(userId).notify("update");
                        }
                    }

                    return new JsonResult
                    {
                        Data = new
                        {
                            ReplyType = "Bid",
                            ReplyId = reply.Id,  
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    //return normal reply của supplier vừa reply
                    return new JsonResult
                    {
                        Data = new
                        {
                            ReplyType = "Normal",
                            ReplyId = reply.Id,
                            SupplierName = user.Fullname,
                            Total = model.Total,
                            Address = user.Address,
                            Description = model.Description,
                            Avatar = user.Avatar,
                            CreatedDate = reply.CreatedDate,
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }

            var errorModel = from x in ModelState.Keys
                             where ModelState[x].Errors.Count > 0
                             select new
                             {
                                 key = x,
                                 error = ModelState[x].Errors.Select(y => y.ErrorMessage).ToArray()
                             };

            return new JsonResult
            {
                Data = new
                {
                    Success = "Fail",
                    Errors = errorModel
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        // GET: 
        public ActionResult GetRank(int id)
        {
            var query = "SELECT[dbo].[BidReplies].[Rank], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], [dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
                        + "FROM[dbo].[Replies], [dbo].[BidReplies], [dbo].[Users] "
                        + "WHERE[dbo].[Replies].[Id] = {0} "
                        + "AND[dbo].[BidReplies].[ReplyId] = [dbo].[Replies].[Id] "
                        + "AND[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] ";

            BriefBidReply data = db.Database.SqlQuery<BriefBidReply>(query, id).SingleOrDefault();

            return new JsonResult
            {
                Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }
    }
}