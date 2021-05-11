using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;

namespace VisionDB.Controllers
{
    [Authorize]
    public class CompaniesController : VisionDBController
    {
        [HttpGet]
        public ActionResult Edit(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            return View(practice.company);
        }

        [HttpPost]
        public ActionResult Edit(Company company)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
                Company existingCompany = db.Practices.Find(practiceId).company;

                existingCompany.Name = company.Name;
                existingCompany.Address = company.Address;
                existingCompany.Postcode = company.Postcode;
                existingCompany.Tel = company.Tel;
                existingCompany.Fax = company.Fax;
                existingCompany.Email = company.Email;
                existingCompany.MultiSite = company.MultiSite;
                existingCompany.CompanyRef = company.CompanyRef;
                existingCompany.ExternalId = company.ExternalId;

                db.SaveChanges();
                TempData["Message"] = "Company settings saved"; 
                return RedirectToAction("Index", "Setup");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Add()
        {
            if (Session["user"] != null && ((VisionDB.Models.ApplicationUser)Session["user"]).UserName == "clark")
            {
                Company company = new Company();
                company.Id = Guid.NewGuid();
                company.ExternalId = company.Id;
                company.CompanyRef = company.Id.ToString().Substring(0, 4).ToUpper();

                return View();
            }
            else
            {
                TempData["Warning"] = "Access denied to admin section. Please use the Setup section or contact Click Software.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Add(Company company)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

                db.Companies.Add(company);

                db.SaveChanges();
                TempData["Message"] = "Company settings saved";
                return RedirectToAction("Index", "Setup");
            }
            else
            {
                return View();
            }
        }
	}
}