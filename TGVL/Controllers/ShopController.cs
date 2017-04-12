using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.IO;

namespace TGVL.Controllers
{
    [Authorize(Roles = "Supplier")]
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
            return RedirectToAction("AddProduct");
        }

        private void PopulateWarehouseData(int supplierId)
        {
            var warehouses = db.Warehouses.Where(w => w.SupplierId == supplierId);

            //var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.CourseID));

            var viewModel = new List<AssignedWarehouseData>();
            foreach (var warehouse in warehouses)
            {
                viewModel.Add(new AssignedWarehouseData
                {
                    WarehouseId = warehouse.Id,
                    Address = warehouse.Address
                    //Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Warehouses = viewModel;
        }

        //GET: Shop/AddProduct
        public ActionResult AddProduct(string searchString)
        {
            var model = new AddProductViewModel();

            var userId = User.Identity.GetUserId<int>();
            

            //TODO: Kiểm tra supplier có warehouse hay không
            var WarehouseId = db.Warehouses.Where(w => w.SupplierId == userId).ToList();
            ViewBag.WarehouseId = new SelectList(WarehouseId, "Id", "Address");

            if (Request.IsAjaxRequest())
            {
                if (!String.IsNullOrWhiteSpace(searchString))
                {
                    PopulateWarehouseData(userId);

                    var productList = db.SysProducts
                        .Where(p => p.Name.Contains(searchString));

                    if (productList == null || !productList.Any())
                    {
                        ViewBag.SysCategoryId = new SelectList(db.SysCategories, "Id", "Name");
                        ViewBag.ManufacturerId = new SelectList(db.Manufacturers, "Id", "Name");
                        ViewBag.UnitTypeId = new SelectList(db.UnitTypes, "Id", "Type");

                        return PartialView("_AddNewProduct");
                    }
                    else
                    {
                        var product = productList.First(); //SysProduct

                        model.ProductId = product.Id;
                        model.ProductName = product.Name;
                        model.CategoryName = product.SysCategory.Name;
                        model.ManufactureName = product.Manufacturer.Name;
                        
                        model.UnitType = product.UnitType.Type;
                    }

                    return PartialView("_Products", model);
                    
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
            


            //return View(model);
            return View();
        }

        // POST: Shop/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(AddProductViewModel model, HttpPostedFileBase uploadImage, int[] selectedWarehouse, int[] quantity)
        {
            if (ModelState.IsValid)
            {
                var responseModel = new ResponeViewModel();
                var userId = User.Identity.GetUserId<int>();
                if (model.ProductId != 0)
                {
                    var productToAdd = db.Products.Where(p => p.SysProductId == model.ProductId && p.SupplierId == userId);

                    //Product đã được supplier add vào trước đó
                    if (productToAdd.Count() != 0)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            responseModel.Message = "Sản phẩm " + productToAdd.Single().SysProduct.Name + " đã được đăng bán tại gian hàng của bạn trước đây.<br/>Nếu muốn chỉnh sửa thông tin sản phẩm, Quý khách vui lòng chọn Cập nhật thông tin sản phẩm để thay đổi.";
                            responseModel.WarehouseProductId = productToAdd.Single().Id;
                            responseModel.Mode = "error";
                            return PartialView("_Response", responseModel);
                        }
                    }

                    // Product có tồn tại trong hệ thống nhưng chưa được add
                    var product = new Product
                    {
                        SysProductId = model.ProductId,
                        SupplierId = userId,
                        UnitPrice = model.UnitPrice,
                    };

                    if (uploadImage != null && uploadImage.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(uploadImage.FileName);

                        var filePath = Path.Combine(Server.MapPath("~/Images/Product"), fileName);

                        if (System.IO.File.Exists(filePath))
                        {
                            ViewBag.Error = "Hinh anh da ton tai";
                        }
                        else
                        {
                            uploadImage.SaveAs(filePath);
                            product.Image = filePath;
                        }
                    }

                    db.Products.Add(product);
                    db.SaveChanges();

                    if (selectedWarehouse != null && quantity != null)
                    {
                        for (int i = 0; i < quantity.Count(); i++)
                        {
                            if(quantity[i]==0)
                            {
                                continue;
                            }
                            var wp = new WarehouseProduct
                            {
                                WarehouseId = selectedWarehouse[i],
                                ProductId = product.Id,
                                Quantity = quantity[i],
                            };
                            db.WarehouseProducts.Add(wp);
                        }
                    }

                    db.SaveChanges();
                    responseModel.WarehouseProductId = product.Id;
                }
                else
                {
                    var sysProduct = new SysProduct
                    {
                        SysCategoryId = model.SysCategoryId,
                        ManufacturerId = model.ManufacturerId,
                        Name = model.ProductName,
                        UnitPrice = model.UnitPrice,
                        UnitTypeId = model.UnitTypeId
                    };
                    db.SysProducts.Add(sysProduct);
                    db.SaveChanges();

                    var product = new Product
                    {
                        SysProductId = sysProduct.Id,
                        SupplierId = userId,
                        UnitPrice = model.UnitPrice,
                    };

                    if (uploadImage != null && uploadImage.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(uploadImage.FileName);

                        var filePath = Path.Combine(Server.MapPath("~/Images/Product"), fileName);

                        if (System.IO.File.Exists(filePath))
                        {
                            ViewBag.Error = "Hinh anh da ton tai";
                        }
                        else
                        {
                            uploadImage.SaveAs(filePath);
                            product.Image = filePath;
                        }
                    }
                    db.Products.Add(product);
                    db.SaveChanges();

                    if (selectedWarehouse != null && quantity != null)
                    {
                        for (int i = 0; i < quantity.Count(); i++)
                        {
                            var wp = new WarehouseProduct
                            {
                                WarehouseId = selectedWarehouse[i],
                                ProductId = product.Id,
                                Quantity = quantity[i],
                            };
                            db.WarehouseProducts.Add(wp);
                        }
                    }

                    responseModel.WarehouseProductId = product.Id;
                }

                responseModel.Message = "Bạn đã đăng thành công sản phẩm " + model.ProductName + ".<br/>Thông tin sản phẩm bạn vừa đăng sẽ được TGVL kiểm duyệt.";
                responseModel.Mode = "success";
                return PartialView("_Response", responseModel);
            }
            return View(model);
        }

        public ActionResult EditDescription (int? id)
        {
            var deal = db.Deals.Find(id);

            var model = new UpdateDescription
            {
                Id = deal.Id,
                Description = deal.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDescription(UpdateDescription model)
        {
            var deal = db.Deals.Find(model.Id);
            deal.Description = model.Description;
            db.Entry(deal).State = EntityState.Modified;
            db.SaveChanges();

            return View();
        }
        public ActionResult ViewProduct()
        {
            return View();
        }

        public ActionResult CreateProduct()
        {
            return View();
        }
    }
}