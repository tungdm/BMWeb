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

        //GET: Shop/AddProduct
        public ActionResult AddProduct(string searchString)
        {
            var model = new AddProductViewModel();

            var userId = User.Identity.GetUserId<int>();
            var WarehouseId = db.Warehouses.Where(w => w.SupplierId == userId).ToList();
            ViewBag.WarehouseId = new SelectList(WarehouseId, "Id", "Address");


            if (!String.IsNullOrWhiteSpace(searchString))
            {
                //model.Products = db.SysProducts
                //.Include(p => p.SysCategory)
                //.Include(p => p.ProductAttributes)
                //.Where(p => p.Name.Contains(searchString));


                //if (model.Products == null || !model.Products.Any())
                //{
                //    return PartialView("_AddNewProduct");
                //}


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
                    var product = productList.First();

                    model.ProductId = product.Id;
                    model.ProductName = product.Name;
                    model.CategoryName = product.SysCategory.Name;
                    model.ManufactureName = product.Manufacturer.Name;
                    model.ProductAttributes = product.ProductAttributes;
                    model.UnitType = product.UnitType.Type;
                }
            }


            if (Request.IsAjaxRequest())
            {
                return PartialView("_Products", model);
            }

            //return View(model);
            return View();
        }

        // POST: Shop/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(AddProductViewModel model, HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                var responseModel = new ResponeViewModel();
                if (model.ProductId != 0)
                {
                   
                    var productToAdd = db.WarehouseProducts.Where(w => w.SysProductId == model.ProductId);

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
                    var warehouseProduct = new WarehouseProduct
                    {
                        WarehouseId = model.WarehouseId,
                        SysProductId = model.ProductId,
                        Quantity = model.Quantity,
                        UnitPrice = model.UnitPrice
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
                            warehouseProduct.Image = filePath;
                        }
                    }

                    db.WarehouseProducts.Add(warehouseProduct);
                    db.SaveChanges();
           
                    responseModel.WarehouseProductId = warehouseProduct.Id;
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
                    var productId = sysProduct.Id;

                    var warehouseProduct = new WarehouseProduct
                    {
                        WarehouseId = model.WarehouseId,
                        SysProductId = productId,
                        Quantity = model.Quantity,
                        UnitPrice = model.UnitPrice
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
                            warehouseProduct.Image = filePath;
                        }
                    }
                    db.WarehouseProducts.Add(warehouseProduct);
                    db.SaveChanges();

                    responseModel.WarehouseProductId = warehouseProduct.Id;

                    
                }

                responseModel.Message = "Bạn đã đăng thành công sản phẩm " + model.ProductName + ".<br/>Thông tin sản phẩm bạn vừa đăng sẽ được TGVL kiểm duyệt.";
                responseModel.Mode = "success";
                return PartialView("_Response", responseModel);
            }
            return View(model);
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