using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TGVL.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;

namespace TGVL.Areas.Admin.Controllers
{

    public class ManageUserController : Controller
    {

        ApplicationDbContext context = new ApplicationDbContext();
        // GET: Admin/ManageUser
        public ActionResult Index()
        {
            IEnumerable<Models.ApplicationUser> model = context.Users.AsEnumerable();

            return View(model);
        }
        // Edit user
        public ActionResult Edit(int Id)
        {
            Models.ApplicationUser model = context.Users.Find(Id);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Models.ApplicationUser model, HttpPostedFileBase fileUpload)
        {
            try
            {
                //// Lưu tên file
                //var fileName = Path.GetFileName(fileUpload.FileName);
                //// Lưu đường dẫn
                //var path = Path.Combine(Server.MapPath("~/Areas/Admin/Images"), fileName);
                //fileUpload.SaveAs(path);
                //model.Avatar = fileUpload.FileName;
                context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                return RedirectToAction("Index", "ManageUser");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
        // Edit role của 1 user
        public ActionResult EditRole(int Id)
        {
            Models.ApplicationUser model = context.Users.Find(Id);
            ViewBag.RoleName = new SelectList(context.Roles.ToList().
                Where(item => model.Roles.FirstOrDefault(r => r.RoleId == item.Id) != null).ToList(), "Id", "Name");
            ViewBag.RoleId = new SelectList(context.Roles.ToList().
                Where(item => model.Roles.FirstOrDefault(r => r.RoleId == item.Id) == null).ToList(), "Id", "Name");
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToRole(int UserId, int[] RoleId)
        {
            Models.ApplicationUser model = context.Users.Find(UserId);
            if (RoleId != null && RoleId.Count() > 0)
            {
                foreach (int item in RoleId)
                {
                    //CustomRole role = context.Roles.Find(RoleId);
                    model.Roles.Add(new CustomUserRole() { UserId = UserId, RoleId = item });
                }
                context.SaveChanges();
            }
            ViewBag.RoleId = new SelectList(context.Roles.ToList().
                Where(item => model.Roles.FirstOrDefault(r => r.RoleId == item.Id) == null).ToList(), "Id", "Name");
            return RedirectToAction("EditRole", "ManageUser", new { Id = UserId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleFromUser(int UserId, int RoleId)
        {
            Models.ApplicationUser model = context.Users.Find(UserId);
            model.Roles.Remove(model.Roles.Single(m => m.RoleId == RoleId));
            context.SaveChanges();
            ViewBag.RoleId = new SelectList(context.Roles.ToList().Where(item => model.Roles.FirstOrDefault(r => r.RoleId == item.Id) == null).ToList(), "Name");
            return RedirectToAction("EditRole", new { Id = UserId });
        }
        public ActionResult Delete(int Id)
        {
            var model = context.Users.Find(Id);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int Id)
        {
            Models.ApplicationUser model = null;
            try
            {
                model = context.Users.Find(Id);
                context.Users.Remove(model);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Delete", model);
            }
        }
    }
}
