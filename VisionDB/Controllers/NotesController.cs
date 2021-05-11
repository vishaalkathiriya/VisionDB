using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using Kendo.Mvc.Extensions;

namespace VisionDB.Controllers
{
    [Authorize]
    public class NotesController : VisionDBController
    {
        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public ActionResult _Read([DataSourceRequest] DataSourceRequest request)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            List<Note> notes = db.Notes.Where(t =>
                t.practice.Id == practice.Id
                && t.customer == null
                && t.Deleted == null).OrderBy(t => t.Description).ToList();

            List<NoteViewModel> noteViewModels = new List<NoteViewModel>();

            foreach (Note note in notes)
            {
                noteViewModels.Add(new NoteViewModel
                {
                    Id = note.Id,
                    Description = note.Description.Length > Models.Note.MaxCharsForDescriptionSummary ? note.Description.Substring(0, Models.Note.MaxCharsForDescriptionSummary) + "..." : note.Description,
                    CreatedTimestamp = note.CreatedTimestamp
                });
            }

            return Json(noteViewModels.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult Add(Guid? CustomerId)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Customer customer = null;
            if (CustomerId != null)
            {
                CustomersDataContext db = new CustomersDataContext();
                customer = db.Customers.Find(CustomerId);

                var validationResult = db.Entry(customer).GetValidationResult();

                if (!validationResult.IsValid)
                {
                    TempData["Error"] = validationResult.ValidationErrors.ToList()[0].ErrorMessage;
                    return RedirectToAction("Edit", "Customers", new { customer.Id });
                }
            }

            Note note = new Note();
            note.customer = customer != null ? customer : null;
            note.CustomerId = customer != null ? customer.Id : Guid.Empty;
            return View(note);
        }

        [HttpPost]
        public ActionResult Add(Note note)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                note.Id = Guid.NewGuid();
                note.CreatedTimestamp = DateTime.Now;
                note.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                note.practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
                db.Notes.Add(note);
                if (note.CustomerId != null)
                {
                    if (note.customer == null)
                    {
                        note.customer = db.Customers.Find(note.CustomerId);
                    }
                    note.customer.LastUpdated = DateTime.Now;
                }

                db.SaveChanges();
                TempData["Message"] = "Note saved";
                if (note.customer != null)
                {
                    return RedirectToAction("Customer", "Customers", new { note.customer.Id });
                }
                else
                {
                    return RedirectToAction("Index", "Notes");
                }
            }
            else
            {
                //todo: show error
                return View();
            }
        }

        [HttpGet]
        public ActionResult Note(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            Note note = db.Notes.Find(Id);

            return View(note);
        }

        [HttpPost]
        public ActionResult Note(Note note)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();

                db.Entry(note).State = EntityState.Modified;

                db.SaveChanges();
                TempData["Message"] = "Note saved";
                if (note.customer != null)
                {
                    return RedirectToAction("Customer", "Customers", new { note.customer.Id });
                }
                else
                {
                    return RedirectToAction("Index", "Notes");
                }
            }
            else
            {
                //todo: show error
                return View();
            }
        }

        public ActionResult Delete(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Note existingNote = db.Notes.Find(Id);
                existingNote.Deleted = DateTime.Now;
                existingNote.DeletedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                if (existingNote.customer != null)
                {
                    existingNote.customer.LastUpdated = DateTime.Now;
                }

                db.SaveChanges();

                TempData["Message"] = "Note deleted";
                if (existingNote.customer != null)
                {
                    return RedirectToAction("Customer", "Customers", new { existingNote.customer.Id });
                }
                else
                {
                    return RedirectToAction("Index", "Notes");
                }
            }
            else
            {
                //todo: standardise error messages
                ViewBag.Error = "Unable to delete note";
                return View();
            }
        }

        public List<Note> GetNotes(Customer customer)
        {
            CustomersDataContext db = new CustomersDataContext();

            var results = from row in db.Notes
                          select row;

            if (customer != null)
            {
                results = results.Where(c => c.customer.Id == customer.Id);
            }
            else
            {
                results = results.Where(c => false);
            }

            return results.ToList();
        }
    }
}