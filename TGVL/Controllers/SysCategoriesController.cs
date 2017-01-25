using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TGVL;

namespace TGVL.Controllers
{
    public class SysCategoriesController : Controller
    {
        private BMWEntities db = new BMWEntities();

        // GET: SysCategories
        public async Task<ActionResult> Index()
        {
            var sysCategories = db.SysCategories.Include(s => s.SysCategory1);
            return View(await sysCategories.ToListAsync());
        }

        // GET: SysCategories/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SysCategory sysCategory = await db.SysCategories.FindAsync(id);
            if (sysCategory == null)
            {
                return HttpNotFound();
            }
            return View(sysCategory);
        }

        // GET: SysCategories/Create
        public ActionResult Create()
        {
            ViewBag.ParentId = new SelectList(db.SysCategories, "Id", "Name");
            return View();
        }

        // POST: SysCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ParentId,Name,Image,SortOrder,Flag")] SysCategory sysCategory)
        {
            if (ModelState.IsValid)
            {
                db.SysCategories.Add(sysCategory);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.SysCategories, "Id", "Name", sysCategory.ParentId);
            return View(sysCategory);
        }

        // GET: SysCategories/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SysCategory sysCategory = await db.SysCategories.FindAsync(id);
            if (sysCategory == null)
            {
                return HttpNotFound();
            }
            ViewBag.ParentId = new SelectList(db.SysCategories, "Id", "Name", sysCategory.ParentId);
            return View(sysCategory);
        }

        // POST: SysCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ParentId,Name,Image,SortOrder,Flag")] SysCategory sysCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sysCategory).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ParentId = new SelectList(db.SysCategories, "Id", "Name", sysCategory.ParentId);
            return View(sysCategory);
        }

        // GET: SysCategories/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SysCategory sysCategory = await db.SysCategories.FindAsync(id);
            if (sysCategory == null)
            {
                return HttpNotFound();
            }
            return View(sysCategory);
        }

        // POST: SysCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SysCategory sysCategory = await db.SysCategories.FindAsync(id);
            db.SysCategories.Remove(sysCategory);
            await db.SaveChangesAsync();
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
