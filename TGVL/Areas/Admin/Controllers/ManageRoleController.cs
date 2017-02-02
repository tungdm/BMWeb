using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace TGVL.Areas.Admin.Controllers
{
    public class ManageRoleController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();

        // GET: Admin/ManageRole
        public ActionResult Index()
        {
            var model = context.Roles.AsEnumerable();
            return View(model);
        }
        //Create role
        public ViewResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomRole role)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    context.Roles.Add(role);
                    context.SaveChanges();
                }
                return RedirectToAction("Index", "ManageRole");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(role);
        }
        // Delete Role
        public ActionResult Delete(int Id)
        {
            var model = context.Roles.Find(Id);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int Id)
        {
            CustomRole model = null;
            try
            {
                model = context.Roles.Find(Id);
                context.Roles.Remove(model);
                context.SaveChanges();
                return RedirectToAction("Index", "ManageRole");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }
        // Sửa role
        public ActionResult Edit(int Id)
        {
            CustomRole role = context.Roles.Find(Id);
            return View(role);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomRole role)
        {
            context.Entry(role).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();
            return RedirectToAction("Index", "ManageRole");
        }
    }
}