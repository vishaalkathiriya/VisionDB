using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using Kendo.Mvc.Extensions;

namespace VisionDB.Controllers
{
    [Authorize]
    public class DocumentTemplatesController : VisionDBController
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

            List<DocumentTemplate> documentTemplates = db.DocumentTemplates.Where(t =>
                t.company.Id == practice.company.Id
                && t.Deleted == null).OrderBy(t => t.Name).ToList();

            List<DocumentTemplateViewModel> documentTemplateViewModels = new List<DocumentTemplateViewModel>();

            foreach (DocumentTemplate documentTemplate in documentTemplates)
            {
                documentTemplateViewModels.Add(new DocumentTemplateViewModel { 
                    Id = documentTemplate.Id, 
                    Name = documentTemplate.Name, 
                    TemplateTypeEnum = documentTemplate.TemplateTypeEnum, 
                    TemplateMethodEnum = documentTemplate.TemplateMethodEnum,
                    Deleted = documentTemplate.Deleted, 
                    TemplateHtml = documentTemplate.TemplateHtml });
            }

            return Json(documentTemplateViewModels.ToDataSourceResult(request));
        }

        public JsonResult GetTemplates()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            List<DocumentTemplate> documentTemplates = db.DocumentTemplates.Where(a => a.company.Id == practice.company.Id && a.Deleted == null).OrderBy(a => a.Name).ToList();
            List<DocumentTemplateViewModel> documentTemplateViewModels = new List<DocumentTemplateViewModel>();

            foreach (DocumentTemplate documentTemplate in documentTemplates)
            {
                documentTemplateViewModels.Add(new DocumentTemplateViewModel
                {
                    Id = documentTemplate.Id,
                    Name = documentTemplate.Name
                });
            }

            return Json(documentTemplateViewModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Add()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(DocumentTemplate template)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);

                template.Id = Guid.NewGuid();
                template.company = practice.company;
                template.TemplateHtml = HttpUtility.HtmlDecode(template.TemplateHtml);
                db.DocumentTemplates.Add(template);

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

            DocumentTemplate template = db.DocumentTemplates.Find(Id);
            if (template == null)
            {
                return HttpNotFound();
            }
            
            template.TemplateHtml = HttpUtility.HtmlDecode(template.TemplateHtml);

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DocumentTemplate template)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                template.TemplateHtml = HttpUtility.HtmlDecode(template.TemplateHtml);
                db.Entry(template).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();

                TempData["Message"] = "Template saved";
                return RedirectToAction("Index");
            }
            return View(template);
        }

        public ActionResult Delete(DocumentTemplate template)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            DocumentTemplate existingTemplate = db.DocumentTemplates.Find(template.Id);
            existingTemplate.Deleted = DateTime.Now;
            db.SaveChanges();

            TempData["Message"] = "Template deleted";
            return RedirectToAction("Index");
        }

        public static string PopulateDocument(DocumentTemplate documentTemplate, Customer customer)
        {
            string result = HttpUtility.HtmlDecode(documentTemplate.TemplateHtml);

            result = result.Replace("[" + Enums.TemplatePlaceholder.contact_lens_exam_due_date.ToString() + "]", customer.NextDueDateContactLensExam.HasValue ? customer.NextDueDateContactLensExam.Value.ToShortDateString() : "");
            result = result.Replace("[" + Enums.TemplatePlaceholder.contact_lens_exam_last_test_date.ToString() + "]", customer.PreviousContactLensExamDate.HasValue ? customer.PreviousContactLensExamDate.Value.ToShortDateString() : "");
            result = result.Replace("[" + Enums.TemplatePlaceholder.eye_exam_due_date.ToString() + "]", customer.NextDueDateEyeExam.HasValue ? customer.NextDueDateEyeExam.Value.ToShortDateString() : "");
            result = result.Replace("[" + Enums.TemplatePlaceholder.eye_exam_last_test_date.ToString() + "]", customer.PreviousEyeExamDate.HasValue ? customer.PreviousEyeExamDate.Value.ToShortDateString() : "");
            result = result.Replace("[" + Enums.TemplatePlaceholder.patient_address.ToString() + "]", customer.FullAddressHtml);
            result = result.Replace("[" + Enums.TemplatePlaceholder.patient_name.ToString() + "]", customer.TitleAndName());
            result = result.Replace("[" + Enums.TemplatePlaceholder.patient_number.ToString() + "]", customer.Number);
            result = result.Replace("[" + Enums.TemplatePlaceholder.practice_address.ToString() + "]", customer.practice.FullAddressHtml);
            result = result.Replace("[" + Enums.TemplatePlaceholder.practice_email.ToString() + "]", customer.practice.Email);
            result = result.Replace("[" + Enums.TemplatePlaceholder.practice_name.ToString() + "]", customer.practice.Name);
            result = result.Replace("[" + Enums.TemplatePlaceholder.practice_telephone.ToString() + "]", customer.practice.Tel);
            result = result.Replace("[" + Enums.TemplatePlaceholder.todays_date.ToString() + "]", DateTime.Now.ToShortDateString());

            return result;
        }
    }
}