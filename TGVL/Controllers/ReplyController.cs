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
        //GET
        public ActionResult AutoBidAgain(int replyId)
        {
            var reply = db.Replies.Find(replyId);

            if (reply != null)
            {
                var requestId = reply.RequestId;
                Request request = db.Requests.Find(requestId);
                var bidRequest = db.BidRequests.Find(requestId);
 
                if (request.StatusId == 2)
                {
                    //request expired
                    return new JsonResult
                    {
                        Data = new
                        {
                            Error = "Expired",
                            Message = "Request Expired."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                if (request.StatusId == 3)
                {
                    //request completed
                    return new JsonResult
                    {
                        Data = new
                        {
                            Error = "Completed",
                            Message = "Request Completed."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var min = reply.BidReply.MinBidPrice;
                var step = reply.BidReply.Deduction;

                var currentPrice = reply.Total;

                var lowestPrice = bidRequest.LowestPrice;

                if (min >= lowestPrice)
                {
                    reply.Total = (decimal)min;
                }
                else
                {
                    var r = Math.Floor((currentPrice - lowestPrice) / (decimal)step) + 1;
                    var newPrice = currentPrice - ((decimal)step * r);
                    if (newPrice <= min)
                    {
                        reply.Total = (decimal)min;
                    }
                    else
                    {
                        reply.Total = newPrice;
                    }
                    bidRequest.LowestPrice = reply.Total;
                    bidRequest.Leader = reply.SupplierId;
                    db.Entry(bidRequest).State = EntityState.Modified;
                }

                db.Entry(reply).State = EntityState.Modified;
                db.SaveChanges();

                //update rank
                var query = "Update BidReplies "
                            + "SET [Rank] = t2.[Rank], [OldRank] = t1.[Rank]"
                            + "FROM BidReplies t1 "
                            + "LEFT OUTER JOIN "
                            + "("
                            + "SELECT BidReplies.ReplyId, Replies.RequestId, Rank() OVER (PARTITION BY RequestId ORDER BY Total asc) as [Rank] "
                            + "FROM BidReplies, Replies "
                            + "WHERE BidReplies.ReplyId = Replies.Id "
                            + "AND  BidReplies.Flag <> 9 "
                            + ") as t2 "
                            + "ON t1.ReplyId = t2.ReplyId "
                            + "AND t2.RequestId = {0} ";

                db.Database.ExecuteSqlCommand(query, requestId);

                query = "SELECT[dbo].[Users].[UserName] "
                        + "FROM[dbo].[Replies], [dbo].[Requests], [dbo].[Users] "
                        + "WHERE[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                        + "AND[dbo].[Replies].[RequestId] = [dbo].[Requests].[Id] "
                        + "AND[dbo].[Requests].[Id] = {0} "
                        + "AND[dbo].[Replies].[Id] <> {1}";
                List<string> listUser = db.Database.SqlQuery<string>(query, requestId, replyId).ToList();

                if (listUser.Count() != 0)
                {
                    var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                    notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer

                    foreach (var userId in listUser)
                    {
                        notificationHub.Clients.User(userId).notify("update"); //noti supplier
                    }
                }
                else
                {
                    var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                    notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer
                }

                //update bid info
                return new JsonResult
                {
                    Data = new
                    {
                        Success = "Success",
                        Type = "Changed",
                        ReplyType = "Bid",
                        ReplyId = reply.Id,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            return new JsonResult
            {
                Data = new
                {
                    Success = "Success",
          
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        //GET
        public ActionResult AutoBid(int replyId)
        {
            Reply reply = db.Replies.Find(replyId);
            var model = new AutobidViewModel
            {
                ReplyId = replyId,
                CurentPrice = string.Format("{0:C0}", reply.Total),
                MinimumPrice = reply.BidReply.MinBidPrice == null ? "0" : string.Format("{0:N0}", reply.BidReply.MinBidPrice),
                Deduction = reply.BidReply.Deduction == null ? "0" : string.Format("{0:N0}", reply.BidReply.Deduction),
                Type = reply.BidReply.MinBidPrice == null ? "new" : "edit"
            };
            ViewBag.MinDeduction = db.Settings.Where(s => s.SettingName == "MinDeduction").FirstOrDefault().SettingValue;
            return PartialView("_Autobid", model);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetAutoBid(AutobidViewModel model)
        {
            var replyId = model.ReplyId;

            var reply = db.Replies.FirstOrDefault(r => r.Id == model.ReplyId);
            if (reply != null)
            {
                Request request = db.Requests.Find(reply.RequestId);
                var requestId = reply.RequestId;
                

                if (request.StatusId == 2)
                {
                    //request expired
                    return new JsonResult
                    {
                        Data = new
                        {
                            Error = "Expired",
                            Message = "Request Expired."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                if (request.StatusId == 3)
                {
                    //request completed
                    return new JsonResult
                    {
                        Data = new
                        {
                            Error = "Completed",
                            Message = "Request Completed."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var min = decimal.Parse(model.MinimumPrice);
                var step = decimal.Parse(model.Deduction);

                reply.BidReply.MinBidPrice = min;
                reply.BidReply.Deduction = step;
                db.Entry(reply).State = EntityState.Modified;

                var currentPrice = reply.Total;
                var bidRequest = db.BidRequests.Find(request.Id);

                var leaderId = bidRequest.Leader;
                var currentSupplierId = User.Identity.GetUserId<int>();

                var lowestPrice = bidRequest.LowestPrice;

                if (currentPrice >= lowestPrice && leaderId != currentSupplierId)
                {
                    if (min >= lowestPrice)
                    {
                        reply.Total = min;
                    } else
                    {
                        var r = Math.Floor((currentPrice - lowestPrice) / step) + 1;
                        var newPrice = currentPrice - (step * r);
                        if (newPrice <= min)
                        {
                            reply.Total = min;
                        } else
                        {
                            reply.Total = newPrice;
                        }
                        bidRequest.LowestPrice = reply.Total;
                        bidRequest.Leader = reply.SupplierId;
                        db.Entry(bidRequest).State = EntityState.Modified;
                    }

                    db.Entry(reply).State = EntityState.Modified;
                    db.SaveChanges();

                    //update rank
                    var query = "Update BidReplies "
                               + "SET [Rank] = t2.[Rank], [OldRank] = t1.[Rank]"
                               + "FROM BidReplies t1 "
                               + "LEFT OUTER JOIN "
                               + "("
                               + "SELECT BidReplies.ReplyId, Replies.RequestId, Rank() OVER (PARTITION BY RequestId ORDER BY Total asc) as [Rank] "
                               + "FROM BidReplies, Replies "
                               + "WHERE BidReplies.ReplyId = Replies.Id "
                               + "AND  BidReplies.Flag <> 9 "
                               + ") as t2 "
                               + "ON t1.ReplyId = t2.ReplyId "
                               + "AND t2.RequestId = {0} ";

                    db.Database.ExecuteSqlCommand(query, requestId);

                    query = "SELECT[dbo].[Users].[UserName] "
                            + "FROM[dbo].[Replies], [dbo].[Requests], [dbo].[Users] "
                            + "WHERE[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND[dbo].[Replies].[RequestId] = [dbo].[Requests].[Id] "
                            + "AND[dbo].[Requests].[Id] = {0} "
                            + "AND[dbo].[Replies].[Id] <> {1}";
                    List<string> listUser = db.Database.SqlQuery<string>(query, requestId, replyId).ToList();

                    if (listUser.Count() != 0)
                    {
                        var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                        notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer

                        foreach (var userId in listUser)
                        {
                            notificationHub.Clients.User(userId).notify("update"); //noti supplier
                        }
                    }
                    else
                    {
                        var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                        notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer
                    }

                    //update bid info
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = "Success",
                            Type = "Changed",
                            ReplyType = "Bid",
                            ReplyId = reply.Id,
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                } else
                {
                    db.SaveChanges();
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = "Success",
                            Type = "Notchanged",
                            ReplyType = "Bid",
                            ReplyId = reply.Id,
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                    

            }
            return View();
        }

        public ActionResult RemoveAutoBid(int replyId)
        {
            var bidReply = db.BidReplies.Find(replyId);
            bidReply.MinBidPrice = null;
            bidReply.Deduction = null;
            db.Entry(bidReply).State = EntityState.Modified;
            db.SaveChanges();

            return new JsonResult
            {
                Data = new
                {
                    Success = "Success",
                   
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //GET: Reply/Details/6
        public ActionResult Details(int replyId, string type)
        {
            Reply reply = db.Replies.Find(replyId);

            var replyProducts = reply.ReplyProducts.ToList();

            var model = new ReplyDetails
            {
                Id = reply.Id,
                Total = reply.Total,
               
                Description = reply.Description,
                DeliveryDate = reply.DeliveryDate,
                ShippingFee = (int) reply.ShippingFee,
                Discount = (int) reply.Discount,
                Rank = reply.Flag == 1 ? (int)reply.BidReply.Rank : 0,
                Flag = (int) reply.Flag,
                BidPrice = string.Format("{0:N0}", reply.Total),
                DueDate = reply.Request.DueDate,
                MinDateDeliveryRange = db.Settings.Where(s => s.SettingTypeId == 1 && s.SettingName == "MinDateDeliveryRange").FirstOrDefault().SettingValue

            };

            var query = "SELECT [dbo].[ReplyProducts].[Id] AS [ReplyProductId], [dbo].[Products].[Id], [dbo].[Products].[UnitPrice], [dbo].[Products].[Image], [dbo].[SysProducts].[Name], [dbo].[UnitTypes].[Type], [dbo].[ReplyProducts].[Quantity] "
                   + "FROM [dbo].[Replies], [dbo].[ReplyProducts], [dbo].[Products], [dbo].[SysProducts], [dbo].[UnitTypes] "
                   + "WHERE [dbo].[Replies].[Id] = [dbo].[ReplyProducts].[ReplyId] "
                   + "AND [dbo].[ReplyProducts].[ProductId] = [dbo].[Products].[Id] "
                   + "AND [dbo].[Products].[SysProductId] = [dbo].[SysProducts].[Id] "
                   + "AND [dbo].[SysProducts].[UnitTypeId] = [dbo].[UnitTypes].[Id] "
                   + "AND [dbo].[Replies].[Id] = {0}";
            IList<RepliedProduct> data = db.Database.SqlQuery<RepliedProduct>(query, replyId).ToList();
            ViewBag.RepliedProduct = data;
            model.ReplyProducts = data;

            if (type == "Edit" && reply.SupplierId == User.Identity.GetUserId<int>())
            {
                return PartialView("_Edit", model);
            }
            return PartialView("_Details", model);
        }

        // GET: Reply/Create
        public ActionResult Create(int requestId)
        {
            var model = new ReplyViewModel();

            Request request = db.Requests.Find(requestId);
            model.Flag = (int) request.Flag;

            if (request.StatusId == 2)
            {
                //request expired
                return new JsonResult
                {
                    Data = new
                    {
                        Error = "Expired",
                        Message = "Request Expired."
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            if (request.StatusId == 3)
            {
                //request completed
                return new JsonResult
                {
                    Data = new
                    {
                        Error = "Completed",
                        Message = "Request Completed."
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

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
       
                        model.ReplyProductsTest = test2;
                        model.DeliveryDate = request.ReceivingDate;
                        model.Flag = (int) request.Flag;
                        model.Total = (decimal)total;
                        model.BidPrice = string.Format("{0:N0}", total);
                        model.DueDate = request.DueDate;
                        model.MinDateDeliveryRange = db.Settings.Where(s => s.SettingTypeId == 1 && s.SettingName == "MinDateDeliveryRange").FirstOrDefault().SettingValue;


                        return PartialView("_Response", model);
                    }
                    else
                    {
                        //Supplier k đủ sp yêu cầu
                        return new JsonResult
                        {
                            Data = new
                            {
                                Message = "Bạn không có sản phẩm phù hợp yêu cầu này."
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

                if (request.StatusId == 2)
                {
                    //request expired
                    return new JsonResult
                    {
                        Data = new
                        {
                            Error = "Expired",
                            Message = "Request Expired."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                if (request.StatusId == 3)
                {
                    //request completed
                    return new JsonResult
                    {
                        Data = new
                        {
                            Error = "Completed",
                            Message = "Request Completed."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                var message = "";
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>()); //current supplier
                var total = decimal.Parse(model.BidPrice);
                if (model.Discount > 0)
                {
                    total = Math.Ceiling(total - (total * model.Discount / 100));
                }

                //Create normal reply
                var reply = new Reply
                {
                    RequestId = requestId,
                    SupplierId = User.Identity.GetUserId<int>(),
                    Description = model.Description,
                    Total = total,
                    ShippingFee = model.ShippingFee,
                    Discount = model.Discount,
                    DeliveryDate = model.DeliveryDate,
                    CreatedDate = DateTime.Now,
                    Flag = request.Flag == 1 ? 1 : 0, //normal reply: flag = 0; bid reply: flag = 1
                    Selected = false
                };

                db.Replies.Add(reply);

                //Create bid reply
                if (request.Flag == 1)
                {
                    var lowsestPrice = request.BidRequest.LowestPrice; //update lowest price
                    if (total < lowsestPrice || lowsestPrice == 0)
                    {
                        request.BidRequest.LowestPrice = total;
                        request.BidRequest.Leader = User.Identity.GetUserId<int>();

                        db.Entry(request).State = EntityState.Modified;
                    }

                    var bidReply = new BidReply
                    {
                        ReplyId = reply.Id,
                        Rank = -1,  //chưa update rank
                        CreatedDate = reply.CreatedDate,
                        Flag = 0
                    };
                    db.BidReplies.Add(bidReply);
                    message = user.Fullname + " đã tham gia đấu thầu yêu cầu \"" + request.Title + "\" của bạn.";
                } else
                {
                    message = user.Fullname + " đã phản hồi yêu cầu \"" + request.Title + "\" của bạn.";
                }


                //Reply Product
                foreach (var p in model.ReplyProductsTest)
                {
                    var rp = new ReplyProduct
                    {
                        ReplyId = reply.Id,
                        ProductId = p.Product.Id,
                        Quantity = (int)p.Quantity,
                        UnitPrice = p.Product.UnitPrice
                    };
                    db.ReplyProducts.Add(rp);
                }

                //Create noti
                var notify = new Notification
                {
                    ReplyId = reply.Id,
                    RequestId = requestId,
                    UserId = request.CustomerId,
                    SenderId = user.Id,
                    CreatedDate = reply.CreatedDate,
                    Message = message,
                    IsSeen = false,
                    IsClicked = false,
                    Flag = 1, //reply
                };
                db.Notifications.Add(notify);

                db.SaveChanges();

                //SignalR
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                //Call all client update num-bidder
                notificationHub.Clients.All.broadcastMessage("updatenumbidder", requestId);   //noti all client

                //Call customer update noti count
                var customer = UserManager.FindById(request.CustomerId).UserName;
                notificationHub.Clients.User(customer).notify("added");

                if (request.Flag == 0)
                {
                    var newestDate = reply.CreatedDate;

                    notificationHub.Clients.User(customer).notify("updatereplytable");

                    //Call all client update reply table
                    notificationHub.Clients.All.broadcastMessage("updatereplytable", requestId, reply.Id);   //noti all client

                    //return normal reply của supplier vừa reply
                    return new JsonResult
                    {
                        Data = new
                        {
                            ReplyType = "Normal",
                            ReplyId = reply.Id,
                            SupplierName = user.Fullname,
                            Total = total,
                            Address = user.Address,
                            Description = model.Description,
                            Avatar = user.Avatar,
                            CreatedDate = reply.CreatedDate,
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };

                }
                else if (request.Flag == 1)
                {
                    //Update rank
                    var query = "Update BidReplies "
                                + "SET [Rank] = t2.[Rank], [OldRank] = t1.[Rank]"
                                + "FROM BidReplies t1 "
                                + "LEFT OUTER JOIN "
                                + "("
                                + "SELECT BidReplies.ReplyId, Replies.RequestId, Rank() OVER (PARTITION BY RequestId ORDER BY Total asc) as [Rank] "
                                + "FROM BidReplies, Replies "
                                + "WHERE BidReplies.ReplyId = Replies.Id "
                                + "AND  BidReplies.Flag <> 9 "
                                + ") as t2 "
                                + "ON t1.ReplyId = t2.ReplyId "
                                + "AND t2.RequestId = {0} ";

                    db.Database.ExecuteSqlCommand(query, requestId);

                    //SignalR - Call another supplier update bid rank
                   
                    var newestDate = reply.CreatedDate;
                    query = "SELECT[dbo].[Users].[UserName] "
                            + "FROM[dbo].[Replies], [dbo].[Requests], [dbo].[Users] "
                            + "WHERE[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND[dbo].[Replies].[RequestId] = [dbo].[Requests].[Id] "
                            + "AND[dbo].[Requests].[Id] = {0} "
                            + "AND[dbo].[Replies].[CreatedDate] < {1}";
                            
                    List<string> listUser = db.Database.SqlQuery<string>(query, requestId, newestDate).ToList();

                    notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable");

                    if (listUser.Count() != 0)
                    {
                        //var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                        foreach (var userId in listUser)
                        {
                            notificationHub.Clients.User(userId).notify("update");
                        }
                    }

                    //Retractable 
                    var MaxDateRetract = db.Settings.Where(s => s.SettingName == "MaxDateRetract").FirstOrDefault().SettingValue;
                    DateTime repCreateDate = (DateTime)reply.CreatedDate;
                    var max = repCreateDate.AddDays(MaxDateRetract);
                    var retractable = false;

                    if (max < request.DueDate)
                    {
                        retractable = true;
                    }

                    return new JsonResult
                    {
                        Data = new
                        {
                            ReplyType = "Bid",
                            ReplyId = reply.Id,  
                            Retractable = retractable
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

        //POST: Reply/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ReplyDetails model)
        {
            if (ModelState.IsValid)
            {
                var reply = db.Replies.FirstOrDefault(r => r.Id == model.Id);
                if (reply != null)
                {
                    Request request = db.Requests.Find(reply.RequestId);

                    if (request.StatusId == 2)
                    {
                        //request expired
                        return new JsonResult
                        {
                            Data = new
                            {
                                Error = "Expired",
                                Message = "Request Expired."
                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }

                    if (request.StatusId == 3)
                    {
                        //request completed
                        return new JsonResult
                        {
                            Data = new
                            {
                                Error = "Completed",
                                Message = "Request Completed."
                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }

                    var oldTotal = reply.Total;
                                         
                    reply.ShippingFee = model.ShippingFee;
                    reply.Discount = model.Discount;
                    reply.Description = model.Description;
                    reply.DeliveryDate = model.DeliveryDate;

                    foreach (var rp in model.ReplyProducts)
                    {
                        var replyProduct = db.ReplyProducts.FirstOrDefault(r => r.Id == rp.ReplyProductId);
                        if (replyProduct != null)
                        {
                            replyProduct.Quantity = rp.Quantity;
                        }
                    }

                    if (reply.Flag == 0)
                    {
                        //normal
                        reply.Total = model.Total;
                        db.SaveChanges();

                        //SignalR - Call all Client update table reply
                        var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                        notificationHub.Clients.All.broadcastMessage("updatereplytable2", reply.RequestId, reply.Id);   //noti all client

                        return new JsonResult
                        {
                            Data = new
                            {
                                Success = "Success",
                                NewTotal = model.Total,
                                ReplyId = model.Id
                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                    else if (reply.Flag == 1 && oldTotal >= decimal.Parse(model.BidPrice))
                    {
                        //bid         
                        var newBidPrice = decimal.Parse(model.BidPrice);
                        reply.Total = newBidPrice; //new total
                        var lowestPrice = request.BidRequest.LowestPrice;

                        if (newBidPrice < lowestPrice)  //update lowest price
                        {
                            request.BidRequest.LowestPrice = newBidPrice;
                            request.BidRequest.Leader = User.Identity.GetUserId<int>();

                            db.Entry(request).State = EntityState.Modified;
                        }

                        //Update rank
                        reply.BidReply.OldRank = reply.BidReply.Rank;
                        db.SaveChanges();

                        var requestId = reply.RequestId;
                        var query = "Update BidReplies "
                                    + "SET [Rank] = t2.[Rank], [OldRank] = t1.[Rank] "
                                    + "FROM BidReplies t1 "
                                    + "LEFT OUTER JOIN "
                                    + "("
                                    + "SELECT BidReplies.ReplyId, Replies.RequestId, Rank() OVER (PARTITION BY RequestId ORDER BY Total asc) as [Rank] "
                                    + "FROM BidReplies, Replies "
                                    + "WHERE BidReplies.ReplyId = Replies.Id "
                                    + "AND  BidReplies.Flag <> 9 "
                                    + ") as t2 "
                                    + "ON t1.ReplyId = t2.ReplyId "
                                    + "AND t2.RequestId = {0} ";

                        var result = db.Database.ExecuteSqlCommand(query, requestId);
                            
                        var supplierId = User.Identity.GetUserId<int>();

                        query = "SELECT[dbo].[Users].[UserName] "
                                + "FROM[dbo].[Replies], [dbo].[Requests], [dbo].[Users] "
                                + "WHERE[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                                + "AND[dbo].[Replies].[RequestId] = [dbo].[Requests].[Id] "
                                + "AND[dbo].[Requests].[Id] = {0} "
                                + "AND[dbo].[Replies].[SupplierId] != {1} ";


                        List<string> listUser = db.Database.SqlQuery<string>(query, requestId, supplierId).ToList();
                        if (listUser.Count() != 0)
                        {
                            var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                            notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer

                            foreach (var userId in listUser)
                            {
                                notificationHub.Clients.User(userId).notify("update"); //noti supplier
                            }
                        } else
                        {
                            var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                            notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer
                        }
                            
                        //update bid info
                        return new JsonResult
                        {
                            Data = new
                            {
                                Success = "Success",
                                ReplyType = "Bid",
                                ReplyId = reply.Id,
                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    } else
                    {
                        return new JsonResult
                        {
                            Data = new
                            {
                                Success = "Fail",
                                ReplyType = "Bid",
                                Message = "Giá bid mới phải bằng hoặc nhỏ hơn giá cũ",
                                OldTotal = (decimal)reply.Total
                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                    
                }
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = "Fail",
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult Ban(int replyId)
        {
            var reply = db.Replies.Find(replyId);
            var banNum = db.ListBanneds.Where(l => l.RequestId == reply.RequestId).Count();
            var maxBan = db.Settings.Where(s => s.SettingName == "MaxBanned").FirstOrDefault().SettingValue;

            var ban = new ListBanned
            {
                RequestId = reply.RequestId,
                SupplierId = reply.SupplierId,
                CreatedDate = DateTime.Now,
            };
            

            db.ListBanneds.Add(ban);

            reply.BidReply.Flag = 9;
            db.Entry(reply).State = EntityState.Modified;

            db.SaveChanges();

            var message = reply.Request.User.Fullname + " đã cấm bạn đặt thầu trong yêu cầu \"" + reply.Request.Title + "\" của họ.";
            
            //create noti
            var notify = new Notification
            {
                ReplyId = reply.Id,
                RequestId = reply.RequestId,
                UserId = reply.User.Id,
                SenderId = reply.Request.User.Id,
                CreatedDate = DateTime.Now,
                Message = message,
                IsSeen = false,
                IsClicked = false,
                Flag = 3, // ban
            };
            db.Notifications.Add(notify);

            db.SaveChanges();
            var requestId = reply.RequestId;
            //SignalR
            var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            //Call all client update num-bidder
            notificationHub.Clients.All.broadcastMessage("updatenumbidder", requestId);   //noti all client

            //Call customer update noti count
            var supplierName = reply.User.UserName;
            notificationHub.Clients.User(supplierName).notify("added");

            //Update rank
            var query = "Update BidReplies "
                        + "SET [Rank] = t2.[Rank], [OldRank] = t1.[Rank] "
                        + "FROM BidReplies t1 "
                        + "LEFT OUTER JOIN "
                        + "("
                        + "SELECT BidReplies.ReplyId, Replies.RequestId, Rank() OVER (PARTITION BY RequestId ORDER BY Total asc) as [Rank] "
                        + "FROM BidReplies, Replies "
                        + "WHERE BidReplies.ReplyId = Replies.Id "
                        + "AND  BidReplies.Flag <> 9 "
                        + ") as t2 "
                        + "ON t1.ReplyId = t2.ReplyId "
                        + "AND t2.RequestId = {0} ";

            var result = db.Database.ExecuteSqlCommand(query, requestId);

            //var bansupplierId = reply.SupplierId;

            query = "SELECT[dbo].[Users].[UserName] "
                    + "FROM[dbo].[Replies], [dbo].[Requests], [dbo].[Users] "
                    + "WHERE[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                    + "AND[dbo].[Replies].[RequestId] = [dbo].[Requests].[Id] "
                    + "AND[dbo].[Requests].[Id] = {0} ";

            List<string> listUser = db.Database.SqlQuery<string>(query, requestId).ToList();

            if (listUser.Count() != 0)
            {
                notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer

                foreach (var userId in listUser)
                {
                    notificationHub.Clients.User(userId).notify("update"); //noti supplier
                }
            }
            else
            {
                notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer
            }

            banNum = maxBan - banNum - 1;
            return new JsonResult
            {
                Data = new
                {
                    Success = "Success",                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
                    Num = banNum
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult Retract(int replyId)
        {
            var reply = db.Replies.FirstOrDefault(r => r.Id == replyId);

            var MaxDateRetract = db.Settings.Where(s => s.SettingName == "MaxDateRetract").FirstOrDefault().SettingValue;
            DateTime repCreateDate = (DateTime) reply.CreatedDate;
            var max = repCreateDate.AddDays(MaxDateRetract);
            var today = DateTime.Now;
            var retractable = false;
            if (today < max && max < reply.Request.DueDate)
            {
                retractable = true; //Có thể rút thầu
            }

            if (retractable)
            {
                Request request = db.Requests.Find(reply.RequestId);

                if (request.StatusId == 2)
                {
                    //request expired
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = "Fail",
                            Message = "Request Expired."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                if (request.StatusId == 3)
                {
                    //request completed
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = "Fail",
                            Message = "Request Completed."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                if (reply.Flag == 1) //bid
                {
                    //bid            
                    reply.BidReply.Flag = 9; //retract
                    var message = reply.User.Fullname + " đã rút đơn thầu khỏi  yêu cầu \"" + request.Title + "\" của bạn.";
                    //create noti
                    var notify = new Notification
                    {
                        ReplyId = reply.Id,
                        RequestId = reply.RequestId,
                        UserId = request.CustomerId,
                        SenderId = reply.SupplierId,
                        CreatedDate = DateTime.Now,
                        Message = message,
                        IsSeen = false,
                        IsClicked = false,
                        Flag = 4, // retract
                    };
                    db.Notifications.Add(notify);

                    db.SaveChanges();
                    var requestId = reply.RequestId;
                    //SignalR
                    var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                    //Call all client update num-bidder
                    notificationHub.Clients.All.broadcastMessage("updatenumbidder", requestId);   //noti all client

                    //Call customer update noti count
                    var customerName = request.User.UserName;
                    notificationHub.Clients.User(customerName).notify("added");

                    //Update rank
                    
                    var query = "Update BidReplies "
                                + "SET [Rank] = t2.[Rank], [OldRank] = t1.[Rank] "
                                + "FROM BidReplies t1 "
                                + "LEFT OUTER JOIN "
                                + "("
                                + "SELECT BidReplies.ReplyId, Replies.RequestId, Rank() OVER (PARTITION BY RequestId ORDER BY Total asc) as [Rank] "
                                + "FROM BidReplies, Replies "
                                + "WHERE BidReplies.ReplyId = Replies.Id "
                                + "AND  BidReplies.Flag <> 9 "
                                + ") as t2 "
                                + "ON t1.ReplyId = t2.ReplyId "
                                + "AND t2.RequestId = {0} ";

                    var result = db.Database.ExecuteSqlCommand(query, requestId);

                    var supplierId = User.Identity.GetUserId<int>();

                    query = "SELECT[dbo].[Users].[UserName] "
                            + "FROM[dbo].[Replies], [dbo].[Requests], [dbo].[Users] "
                            + "WHERE[dbo].[Replies].[SupplierId] = [dbo].[Users].[Id] "
                            + "AND[dbo].[Replies].[RequestId] = [dbo].[Requests].[Id] "
                            + "AND[dbo].[Requests].[Id] = {0} "
                            + "AND[dbo].[Replies].[SupplierId] != {1} ";


                    List<string> listUser = db.Database.SqlQuery<string>(query, requestId, supplierId).ToList();
                    if (listUser.Count() != 0)
                    {
                        notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer

                        foreach (var userId in listUser)
                        {
                            notificationHub.Clients.User(userId).notify("update"); //noti supplier
                        }
                    }
                    else
                    {
                        notificationHub.Clients.User(reply.Request.User.UserName).notify("updatebidtable"); //noti customer
                    }

                    //update bid info
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = "Success",
                            ReplyType = "Bid",
                            ReplyId = reply.Id,
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = "Fail",
                    Message = "Quá thời hạn rút thầu."
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        // GET: 
        public ActionResult GetRank(int id)
        {
            var query = "SELECT[dbo].[BidReplies].[Rank], [dbo].[Users].[Fullname], [dbo].[Users].[Avatar], [dbo].[Users].[Address], "
                        + "[dbo].[Replies].[Total], [dbo].[Replies].[DeliveryDate], [dbo].[Replies].[Id] "
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