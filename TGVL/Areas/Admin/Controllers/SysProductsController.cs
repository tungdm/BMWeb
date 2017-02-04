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
    public class SysProductsController : Controller
    {
        private BMWEntities db = new BMWEntities();

        // GET: SysProducts
        public ActionResult Index()
        {
            var sysProducts = db.SysProducts.Include(s => s.Manufacturer).Include(s => s.SysCategory);
            return View(sysProducts.ToList());
        }

        // GET: SysProducts/Details/5
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

        // GET: SysProducts/Create
        public ActionResult Create()
        {
            ViewBag.ManufacturerId = new SelectList(db.Manufacturers, "Id", "Name");
            ViewBag.SysCategoryId = new SelectList(db.SysCategories, "Id", "Name");
            return View();
        }

        // POST: SysProducts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(/*[Bind(Include = "Id,ManufacturerId,SysCategoryId,Name,Price,SortOrder,Flag")]*/ SysProduct sysProduct, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                //if (upload != null && upload.ContentLength > 0)
                //{
                //    var avatar = new File()
                //    {
                //        FileName = System.IO.Path.GetFileName(upload.FileName),
                //        FileType = FileType.Avatar,
                //        ContentType = upload.ContentType
                //    };
                //    using (var reader = new System.IO.BinaryReader(upload.InputStream))
                //    {
                //        avatar.Content = reader.ReadBytes(upload.ContentLength);
                //    }
                //    student.Files = new List<File> { avatar };
                //}

                // code mau
                var fileName = Path.GetFileName(upload.FileName);
                // duong dan
                var filePath = Path.Combine(Server.MapPath("~/Areas/Admin/Content/img/manufacturer"), fileName);
                // kiem tra hinh anh da ton tai hay chua
                if (System.IO.File.Exists(filePath))
                {
                    ViewBag.Error = "Hinh anh da ton tai";
                }
                else
                {
                    //fileName = fileName + DateTime.Now;
                    upload.SaveAs(filePath);
                }
                sysProduct.Image = upload.FileName;
                db.SysProducts.Add(sysProduct);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ManufacturerId = new SelectList(db.Manufacturers, "Id", "Name", sysProduct.ManufacturerId);
            ViewBag.SysCategoryId = new SelectList(db.SysCategories, "Id", "Name", sysProduct.SysCategoryId);
            return View(sysProduct);
        }

        // GET: SysProducts/Edit/5
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
            return View(sysProduct);
        }

        // POST: SysProducts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ManufacturerId,SysCategoryId,Name,Price,SortOrder,Flag")] SysProduct sysProduct)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sysProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ManufacturerId = new SelectList(db.Manufacturers, "Id", "Name", sysProduct.ManufacturerId);
            ViewBag.SysCategoryId = new SelectList(db.SysCategories, "Id", "Name", sysProduct.SysCategoryId);
            return View(sysProduct);
        }

        // GET: SysProducts/Delete/5
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

        // POST: SysProducts/Delete/5
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
