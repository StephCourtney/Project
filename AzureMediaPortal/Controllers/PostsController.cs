﻿using AzureMediaPortal.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace AzureMediaPortal.Controllers {
    [Authorize]
    public class PostsController : Controller {
        private AzureMediaPortalContext db = new AzureMediaPortalContext();

        //
        // GET: /Posts/

        public ActionResult Index() {

            return View(db.Posts.ToList());
        }

        //
        // GET: /Posts/Details/5

        public ActionResult Details(int id = 0) {
            Post post = db.Posts.Find(id);
            if (post == null) {
                return HttpNotFound();
            }
            return View(post);
        }


        //
        // GET: /Posts/Edit/5

        public ActionResult Edit(int id = 0) {

            Post post = db.Posts.Find(id);
            if (post == null) {
                return HttpNotFound();
            }
            return View(post);
        }

        //
        // POST: /Posts/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Post post) {
            if (ModelState.IsValid) {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        //
        // GET: /Posts/Delete/5

        public ActionResult Delete(int id = 0) {
            Post post = db.Posts.Find(id);
            if (post == null) {
                return HttpNotFound();
            }
            return View(post);
        }

        //
        // POST: /Posts/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}