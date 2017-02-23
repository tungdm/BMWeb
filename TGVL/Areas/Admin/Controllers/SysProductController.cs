using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TGVL;

namespace TGVL.Areas.Admin.Controllers
{
    public class SysProductController : Controller
    {
        private BMWEntities db = new BMWEntities();

        // GET: Admin/SysProduct
        public ActionResult Index()
        {
            var sysProducts = db.SysProducts.Include(s => s.Manufacturer).Include(s => s.SysCategory).Include(s => s.UnitType);
            return View(sysProducts.ToList());
        }

        // GET: Admin/SysProduct/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SysProduct sysProduct = db.SysProducts.Find(id);
            if (sysProduct == null)
            {
                return HttpNotFound();
            }
            return View(sysProduct);
        }

        // GET: Admin/SysProduct/Create
        public ActionResult Create()
        {
            ViewBag.ManufacturerId = new SelectList(db.Manufacturers, "Id", "Name");
            ViewBag.SysCategoryId = new SelectList(db.SysCategories, "Id", "Name");
            ViewBag.UnitTypeId = new SelectList(db.UnitTypes, "Id", "Type");
            return View();
        }

        // POST: Admin/SysProduct/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SysCategoryId,Name,UnitPrice,SortOrder,ManufacturerId,UnitTypeId,Flag")] SysProduct sysProduct, HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(uploadImage.FileName);

                    var filePath = Path.Combine(Server.MapPath("~/Images/Product/SysProduct"), fileName);
                    
                    var physicPath = Server.MapPath("~/Images/Product/SysProduct" + fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        ViewBag.Error = "Hinh anh da ton tai";
                    }
                    else
                    {
                        uploadImage.SaveAs(filePath);
                        sysProduct.Image = fileName;
                    }
                }

                db.SysProducts.Add(sysProduct);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ManufacturerId = new SelectList(db.Manufacturers, "Id", "Name", sysProduct.ManufacturerId);
            ViewBag.SysCategoryId = new SelectList(db.SysCategories, "Id", "Name", sysProduct.SysCategoryId);
            ViewBag.UnitTypeId = new SelectList(db.UnitTypes, "Id", "Type", sysProduct.UnitTypeId);
            return View(sysProduct);
        }

        // GET: Admin/SysProduct/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SysProduct sysProduct = db.SysProducts.Find(id);
            if (sysProduct == null)
            {
                return HttpNotFound();
            }
            ViewBag.ManufacturerId = new SelectList(db.Manufacturers, "Id", "Name", sysProduct.ManufacturerId);
            ViewBag.SysCategoryId = new SelectList(db.SysCategories, "Id", "Name", sysProduct.SysCategoryId);
            ViewBag.UnitTypeId = new SelectList(db.UnitTypes, "Id", "Type", sysProduct.UnitTypeId);
            return View(sysProduct);
        }

        // POST: Admin/SysProduct/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SysCategoryId,Name,UnitPrice,SortOrder,ManufacturerId,UnitTypeId,Flag")] SysProduct sysProduct, HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(uploadImage.FileName);

                    var filePath = Path.Combine(Server.MapPath("~/Images/Product/SysProduct"), fileName);

                    var physicPath = Server.MapPath("~/Images/Product/SysProduct" + fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        ViewBag.Error = "Hinh anh da ton tai";
                    }
                    else
                    {
                        uploadImage.SaveAs(filePath);
                        sysProduct.Image = fileName;
                    }
                }

                db.Entry(sysProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ManufacturerId = new SelectList(db.Manufacturers, "Id", "Name", sysProduct.ManufacturerId);
            ViewBag.SysCategoryId = new SelectList(db.SysCategories, "Id", "Name", sysProduct.SysCategoryId);
            ViewBag.UnitTypeId = new SelectList(db.UnitTypes, "Id", "Type", sysProduct.UnitTypeId);
            return View(sysProduct);
        }

        // GET: Admin/SysProduct/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SysProduct sysProduct = db.SysProducts.Find(id);
            if (sysProduct == null)
            {
                return HttpNotFound();
            }
            return View(sysProduct);
        }

        // POST: Admin/SysProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SysProduct sysProduct = db.SysProducts.Find(id);
            db.SysProducts.Remove(sysProduct);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
