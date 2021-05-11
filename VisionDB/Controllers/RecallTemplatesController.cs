using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using VisionDB.Models;

namespace VisionDB.Controllers
{
    public class RecallTemplatesController : VisionDBController
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

            List<RecallTemplate> recallTemplates = db.RecallTemplates.Where(t =>
                t.company.Id == practice.company.Id
                && t.Deleted == null).OrderBy(t => t.Name).ToList();

            List<RecallTemplateViewModel> recallTemplateViewModels = new List<RecallTemplateViewModel>();

            foreach (RecallTemplate recallTemplate in recallTemplates)
            {
                recallTemplateViewModels.Add(new RecallTemplateViewModel
                {
                    Id = recallTemplate.Id,
                    Name = recallTemplate.Name
                });
            }

            return Json(recallTemplateViewModels.ToDataSourceResult(request));
        }

        public JsonResult GetTemplates()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            List<RecallTemplate> recallTemplates = db.RecallTemplates.Where(a => a.company.Id == practice.company.Id && a.Deleted == null).OrderBy(a => a.Name).ToList();
            List<RecallTemplateViewModel> recallTemplateViewModels = new List<RecallTemplateViewModel>();
            recallTemplateViewModels.Add(new RecallTemplateViewModel { Id = Guid.Empty, Name = "None" });

            foreach (RecallTemplate recallTemplate in recallTemplates)
            {
                recallTemplateViewModels.Add(new RecallTemplateViewModel
                {
                    Id = recallTemplate.Id,
                    Name = recallTemplate.Name
                });
            }

            return Json(recallTemplateViewModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Add()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Session["recallDocuments"] = new List<RecallDocument>();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(RecallTemplate template)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (((List<RecallDocument>)Session["recallDocuments"]).Count == 0)
            {
                ViewBag.Error = "No templates added";
                TempData["Error"] = "No templates added";
                return Add();
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            
            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);

                RecallTemplate newRecallTemplate = new RecallTemplate();
                newRecallTemplate.Id = Guid.NewGuid();
                newRecallTemplate.company = practice.company;
                newRecallTemplate.Name = template.Name;

                List<RecallDocument> recallDocuments = (List<RecallDocument>)Session["recallDocuments"];

                foreach (RecallDocument recallDocument in recallDocuments)
                {
                    RecallDocument newRecallDocument = new RecallDocument();
                    newRecallDocument.Id = recallDocument.Id != Guid.Empty ? recallDocument.Id : Guid.NewGuid();
                    newRecallDocument.documentTemplate = db.DocumentTemplates.Find(recallDocument.documentTemplate.Id);
                    newRecallDocument.DateSpanValue = recallDocument.DateSpanValue;
                    newRecallDocument.DateSpanUnit = recallDocument.DateSpanUnit;
                    newRecallDocument.BeforeOrAfterEnum = recallDocument.BeforeOrAfterEnum;
                    newRecallDocument.Deleted = recallDocument.Deleted;
                    newRecallDocument.recallTemplate = newRecallTemplate;

                    db.RecallDocuments.Add(newRecallDocument);
                }

                db.RecallTemplates.Add(newRecallTemplate);

                db.SaveChanges();

                TempData["Message"] = "Template saved";
                return RedirectToAction("Index");
            }

            return Add();
        }

        [HttpGet]
        public ActionResult Edit(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            RecallTemplate template = db.RecallTemplates.Find(Id);

            if (template == null)
            {
                return HttpNotFound();
            }

            Session["recallDocuments"] = GetRecallDocuments(db, template);
            ViewBag.RecallDocuments = GetRecallDocuments(GetRecallDocuments(db, template));

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RecallTemplate template)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();

                db.Entry(template).State = System.Data.Entity.EntityState.Modified;

                foreach (RecallDocument recallDocument in ((List<RecallDocument>)Session["recallDocuments"]))
                {
                    RecallDocument existingRecallDocument = db.RecallDocuments.Find(recallDocument.Id);
                    if (existingRecallDocument != null)
                    {
                        existingRecallDocument.documentTemplate = db.DocumentTemplates.Find(recallDocument.documentTemplate.Id);
                        existingRecallDocument.DateSpanValue = recallDocument.DateSpanValue;
                        existingRecallDocument.DateSpanUnit = recallDocument.DateSpanUnit;
                        existingRecallDocument.BeforeOrAfterEnum = recallDocument.BeforeOrAfterEnum;
                        existingRecallDocument.Deleted = recallDocument.Deleted;
                    }
                    else
                    {
                        RecallDocument newRecallDocument = new RecallDocument();
                        newRecallDocument.Id = recallDocument.Id != Guid.Empty ? recallDocument.Id : Guid.NewGuid();
                        newRecallDocument.documentTemplate = db.DocumentTemplates.Find(recallDocument.documentTemplate.Id);
                        newRecallDocument.DateSpanValue = recallDocument.DateSpanValue;
                        newRecallDocument.DateSpanUnit = recallDocument.DateSpanUnit;
                        newRecallDocument.BeforeOrAfterEnum = recallDocument.BeforeOrAfterEnum;
                        newRecallDocument.Deleted = recallDocument.Deleted;
                        newRecallDocument.recallTemplate = db.RecallTemplates.Find(template.Id);

                        db.RecallDocuments.Add(newRecallDocument);
                    }
                }

                db.SaveChanges();

                TempData["Message"] = "Template saved";
                return RedirectToAction("Index");
                
            }
            return View(template);
        }

        public ActionResult Delete(RecallTemplate template)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            RecallTemplate existingTemplate = db.RecallTemplates.Find(template.Id);
            existingTemplate.Deleted = DateTime.Now;
            db.SaveChanges();

            TempData["Message"] = "Template deleted";
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult AddRecallDocument([DataSourceRequest] DataSourceRequest request, RecallDocumentViewModel recallDocumentViewModel, string Mode)
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            RecallDocument newRecallDocument = new RecallDocument();
            List<RecallDocument> recallDocuments = ((List<RecallDocument>)Session["recallDocuments"]);

            if (Mode != "Create")
            {
                newRecallDocument.Id = Guid.NewGuid();
                recallDocumentViewModel.Id = newRecallDocument.Id;

                if (recallDocumentViewModel.documentTemplate != null)
                {
                    newRecallDocument.documentTemplate = db.DocumentTemplates.Find(new Guid(recallDocumentViewModel.documentTemplate));
                }
                else
                {
                    newRecallDocument.documentTemplate = db.DocumentTemplates.Where(t =>
                        t.company.Id == practice.company.Id
                        && t.Deleted == null).OrderBy(t => t.Name).First();
                }

                if (recallDocumentViewModel.BeforeOrAfterEnum == 0)
                {
                    recallDocumentViewModel.BeforeOrAfterEnum = Enums.BeforeOrAfter.After;
                }
                newRecallDocument.BeforeOrAfterEnum = recallDocumentViewModel.BeforeOrAfterEnum;

                if (recallDocumentViewModel.DateSpanUnit == 0)
                {
                    recallDocumentViewModel.DateSpanUnit = Enums.FrequencyUnit.Days;
                }
                newRecallDocument.DateSpanUnit = recallDocumentViewModel.DateSpanUnit;

                if (recallDocumentViewModel.DateSpanValue == 0)
                {
                    recallDocumentViewModel.DateSpanValue = 1;
                }
                newRecallDocument.DateSpanValue = recallDocumentViewModel.DateSpanValue;

                recallDocumentViewModel.documentTemplate = newRecallDocument.documentTemplate.Name;
                recallDocumentViewModel.TemplateMethodEnum = newRecallDocument.documentTemplate.TemplateMethodEnum;
                recallDocuments.Add(newRecallDocument);
            }
            else
            {
                throw new ApplicationException("Error adding line. Contact support.");
            }

            return Json(new[] { recallDocumentViewModel }.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdateRecallDocument([DataSourceRequest] DataSourceRequest request, RecallDocumentViewModel recallDocumentViewModel)
        {
            if (recallDocumentViewModel != null)
            {
                CustomersDataContext db = new CustomersDataContext();
                Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
                Practice practice = db.Practices.Find(practiceId);
                foreach (RecallDocument existingRecallDocument in ((List<RecallDocument>)Session["recallDocuments"]))
                {
                    if (existingRecallDocument.Id == recallDocumentViewModel.Id)
                    {
                        existingRecallDocument.documentTemplate = db.DocumentTemplates.Where(d =>
                            d.Deleted == null
                            && d.company.Id == practice.company.Id
                            && d.Name == recallDocumentViewModel.documentTemplate).First();
                        existingRecallDocument.DateSpanUnit = recallDocumentViewModel.DateSpanUnit;
                        existingRecallDocument.DateSpanValue = recallDocumentViewModel.DateSpanValue;
                        existingRecallDocument.BeforeOrAfterEnum = recallDocumentViewModel.BeforeOrAfterEnum;

                    }
                }
            }

            return Json(new[] { recallDocumentViewModel }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DeleteRecallDocument([DataSourceRequest] DataSourceRequest request, RecallDocumentViewModel recallDocumentViewModel)
        {
            if (recallDocumentViewModel != null)
            {
                foreach (RecallDocument existingRecallDocument in ((List<RecallDocument>)Session["recallDocuments"]))
                {
                    if (existingRecallDocument.Id == recallDocumentViewModel.Id)
                    {
                        existingRecallDocument.Deleted = DateTime.Now;
                    }
                }
            }

            return Json(new[] { recallDocumentViewModel }.ToDataSourceResult(request, ModelState));
        }

        internal List<RecallDocument> GetRecallDocuments(CustomersDataContext db, RecallTemplate recallTemplate)
        {
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            return db.RecallDocuments.Where(t =>
                t.documentTemplate.company.Id == practice.company.Id
                && t.recallTemplate.Id == recallTemplate.Id
                && t.Deleted == null).OrderBy(t => t.documentTemplate.Name).ToList();
        }

        internal List<RecallDocument> GetRecallDocuments(CustomersDataContext db, RecallTemplate recallTemplate, Practice practice)
        {
            return db.RecallDocuments.Where(t =>
                t.documentTemplate.company.Id == practice.company.Id
                && t.recallTemplate.Id == recallTemplate.Id
                && t.Deleted == null).OrderBy(t => t.documentTemplate.Name).ToList();
        }

        private List<RecallDocumentViewModel> GetRecallDocuments(IList<RecallDocument> recallDocuments)
        {
            List<RecallDocumentViewModel> recallDocumentViewModels = new List<RecallDocumentViewModel>();

            foreach (RecallDocument recallTemplate in recallDocuments)
            {
                recallDocumentViewModels.Add(new RecallDocumentViewModel
                {
                    Id = recallTemplate.Id,
                    documentTemplate = recallTemplate.documentTemplate.Name,
                    DateSpanValue = recallTemplate.DateSpanValue,
                    DateSpanUnit = recallTemplate.DateSpanUnit,
                    BeforeOrAfterEnum = recallTemplate.BeforeOrAfterEnum,
                    TemplateMethodEnum = recallTemplate.documentTemplate.TemplateMethodEnum
                });
            }
            return recallDocumentViewModels;
        }
    }
}