using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Helper;
using VisionDB.Models;

namespace VisionDB.Controllers
{
    public class ExpensesController : VisionDBController
    {
        public ActionResult Expenses(DateTime? Start, DateTime? End)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Session["customer"] = null;
            if (Start == null)
            {
                Start = DateHelper.GetFirstDateForWeek();
                End = Start.Value.AddDays(6);
            }

            End = End.Value.AddHours(23).AddMinutes(59);

            ViewBag.Start = Start.Value;
            ViewBag.End = End.Value;

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            List<Expense> expenses = db.Expenses.Where(i =>
                i.ExpenseDate >= Start
                && i.ExpenseDate <= End
                && i.Deleted == null
                && i.practice.Id == practice.Id
                ).ToList();

            List<ExpenseViewModel> expenseViewModels = new List<ExpenseViewModel>();
            foreach (Expense expense in expenses)
            {
                expenseViewModels.Add(new ExpenseViewModel
                {
                    Id = expense.Id,
                    ExpenseDate = expense.ExpenseDate,
                    Payee = expense.Payee,
                    Cost = expense.Cost,
                    Category = expense.Category,
                    StatusEnum = expense.StatusEnum
                });
            }

            ViewBag.Expenses = expenseViewModels;

            return View();
        }

        [HttpGet]
        public ActionResult Add()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Categories = GetCategories();

            return View();
        }


        [HttpPost]
        public ActionResult Add(Expense expense)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();

                if (expense.Id == Guid.Empty)
                {
                    expense.Id = Guid.NewGuid();
                }

                expense.Added = DateTime.Now;
                expense.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                expense.practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);

                db.Expenses.Add(expense);
                db.SaveChanges();

                TempData["Message"] = "Expense saved";
                return RedirectToAction("Expenses");
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
            Expense expense = db.Expenses.Find(Id);

            ViewBag.Categories = GetCategories();

            return View(expense);
        }


        [HttpPost]
        public ActionResult Edit(Expense expense)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Expense existingExpense = db.Expenses.Find(expense.Id);

                existingExpense.Payee = expense.Payee;
                existingExpense.ExpenseDate = expense.ExpenseDate;
                existingExpense.Description = expense.Description;
                existingExpense.Cost = expense.Cost;
                existingExpense.Category = expense.Category;
                existingExpense.StatusEnum = expense.StatusEnum;
                existingExpense.VATRate = expense.VATRate;

                db.SaveChanges();

                TempData["Message"] = "Expense saved";
                return RedirectToAction("Expenses");
            }

            return Add();
        }

        public ActionResult Delete(Expense expense)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            ModelState.Clear();

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Expense existingExpense = db.Expenses.Find(expense.Id);

                existingExpense.Deleted = DateTime.Now;

                db.SaveChanges();

                TempData["Message"] = "Expense deleted";
                return RedirectToAction("Expenses");
            }

            return Add();
        }

        private List<string> GetCategories()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            List<string> categories = new List<string>();
            IEnumerable<Expense> existingExpenses = db.Expenses.Where(c => c.practice.company.Id == companyId && c.Deleted == null && c.Category != null && c.Category.Trim().Length > 0);

            foreach (Expense existingExpense in existingExpenses)
            {
                if (!categories.Contains(existingExpense.Category))
                {
                    categories.Add(existingExpense.Category);
                }
            }
            return categories.OrderBy(m => m.ToString()).ToList();
        }
    }
}