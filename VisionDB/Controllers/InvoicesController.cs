using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Models;
using Kendo.Mvc.Extensions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using VisionDB.Helper;
using System.Web.Configuration;
using MvcReportViewer;
using System.IO;
using System.Globalization;
using System.Net.Http;

namespace VisionDB.Controllers
{
    [Authorize]
    public class InvoicesController : VisionDBController
    {
        public ActionResult Invoice(Guid Id)
        {
            CustomersDataContext db = new CustomersDataContext();
            Invoice invoice = db.Invoices.Find(Id);
            Session["customer"] = invoice.customer;
            Session["invoice"] = invoice;
            ViewBag.Customer = invoice.customer;
            bool IsRefundableInvoice = false;
            List<InvoiceDetailViewModel> invoiceDetails = new List<InvoiceDetailViewModel>();
            foreach (InvoiceDetail invoiceDetail in new InventoryController().GetInvoiceDetails(invoice, db).ToList())
            {
                // DRC INFOTECH
                // CHECK IF INVOICE CONTAINS REFUNCDABLE PRODUCT
                if ((int)invoiceDetail.product.ProductTypeEnum == 5)
                {
                    IsRefundableInvoice = true;
                }

                invoiceDetails.Add(new InvoiceDetailViewModel
                {
                    Id = invoiceDetail.Id,
                    product = invoiceDetail.product,
                    UnitPrice = invoiceDetail.UnitPrice,
                    VATRate = invoiceDetail.VATRate,
                    DiscountPercentage = invoiceDetail.DiscountPercentage,
                    Quantity = invoiceDetail.Quantity,
                    SpectacleNumber = invoiceDetail.SpectacleNumber,
                    Description = invoiceDetail.Description,
                    Added = invoiceDetail.Added,
                    Dispensed = invoiceDetail.Dispensed,
                    Code = invoiceDetail.Code,
                    Status = invoiceDetail.product.NegativeValue == false && invoiceDetail.product.GoodsInProduct ? invoiceDetail.StatusEnum.ToString().Replace('_', ' ') : ""
                });
            }
            ViewBag.IsRefundableInvoice = IsRefundableInvoice;
            ViewBag.InvoiceDetails = invoiceDetails;

            if (invoice.customer != null)
            {
                EyeExam LatestEyeExam = db.EyeExams.Where(ee => ee.customer.Id == invoice.customer.Id && ee.Deleted == null).OrderByDescending(ee => ee.TestDateAndTime).Take(1).FirstOrDefault();
                if (LatestEyeExam != null)
                {
                    ViewBag.PDRight = LatestEyeExam.PDRight;
                    ViewBag.PDRightNear = LatestEyeExam.PDRightNear;
                    ViewBag.RHeight = LatestEyeExam.RHeight;
                    ViewBag.PDLeft = LatestEyeExam.PDLeft;
                    ViewBag.PDLeftNear = LatestEyeExam.PDLeftNear;
                    ViewBag.LHeight = LatestEyeExam.LHeight;
                }
            }

            return View(invoice);
        }

        public List<Invoice> GetInvoices(Customer customer)
        {
            CustomersDataContext db = new CustomersDataContext();

            var results = from row in db.Invoices
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

        [HttpGet]
        public ActionResult Add()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Session["invoiceDetails"] = new List<InvoiceDetail>();
            CustomersDataContext db = new CustomersDataContext();
            Customer customer = ((Customer)HttpContext.Session["customer"]);
            ViewBag.Customer = customer;
            if (customer != null)
            {
                var validationResult = db.Entry(customer).GetValidationResult();

                if (!validationResult.IsValid)
                {
                    TempData["Error"] = validationResult.ValidationErrors.ToList()[0].ErrorMessage;
                    return RedirectToAction("Edit", "Customers", new { customer.Id });
                }
            }
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            string NextInvoiceNumber = practice.PracticeNumber.ToString() + (practice.LastInvoiceNumber + 1).ToString();
            ViewBag.NextInvoiceNumber = NextInvoiceNumber;
            Session["NextInvoiceNumber"] = NextInvoiceNumber;
            practice.LastInvoiceNumber += 1;
            db.SaveChanges();

            ViewBag.Products = new CustomersDataContext().Products.Where(p => p.Deleted == null
                && (p.company.Id == practice.company.Id || p.company == null)
                && p.ProductTypeEnum != Product.ProductTypeList.NHS).ToList();

            if (customer != null)
            {
                EyeExam LatestEyeExam = db.EyeExams.Where(ee => ee.customer.Id == customer.Id && ee.Deleted == null).OrderByDescending(ee => ee.TestDateAndTime).Take(1).FirstOrDefault();
                if (LatestEyeExam != null)
                {
                    ViewBag.HasLastEyeExam = true;
                    ViewBag.PDRight = LatestEyeExam.PDRight;
                    ViewBag.PDRightNear = LatestEyeExam.PDRightNear;
                    ViewBag.RHeight = LatestEyeExam.RHeight;
                    ViewBag.PDLeft = LatestEyeExam.PDLeft;
                    ViewBag.PDLeftNear = LatestEyeExam.PDLeftNear;
                    ViewBag.LHeight = LatestEyeExam.LHeight;
                }
                else
                {
                    ViewBag.HasLastEyeExam = false;
                }
            }
            ViewBag.ShowDomiciliaryFields = practice.ShowDomiciliaryFields;

            EyeExamsController eyeExamsController = new EyeExamsController();
            List<EyeExam> eyeExams = eyeExamsController.GetEyeExams(customer).Where(e => e.Deleted == null).OrderBy(e => e.TestDateAndTime).ToList();
            List<EyeExamViewModel> eyeExamViewModels = new List<EyeExamViewModel>();
            eyeExamViewModels.Add(new EyeExamViewModel { Id = Guid.Empty, ToStringEyeExam = "None" });

            foreach (EyeExam eyeExam in eyeExams)
            {
                eyeExamViewModels.Add(new EyeExamViewModel { Id = eyeExam.Id, ToStringEyeExam = eyeExam.ToStringEyeExam });
            }

            ViewBag.EyeExams = eyeExamViewModels;

            return View();
        }

        [HttpPost]
        public ActionResult Add(Invoice invoice, Enums.NHSVoucherGrade? NHSVoucher, float? PDRight, float? PDRightNear, float? RHeight, float? PDLeft, float? PDLeftNear, float? LHeight, string SelectedEyeExam)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (((List<InvoiceDetail>)Session["invoiceDetails"]).Count == 0)
            {
                ViewBag.Error = "No products added";
                TempData["Error"] = "No templates added";
                return Add();
            }

            CustomersDataContext db = new CustomersDataContext();

            invoice.Id = Guid.NewGuid();
            invoice.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            invoice.CreatedTimestamp = DateTime.Now;
            invoice.InvoiceDate = invoice.InvoiceDate;
            invoice.practice = practice;
            if (SelectedEyeExam != null && Guid.Empty != new Guid(SelectedEyeExam))
            {
                invoice.DispenseEyeExam = db.EyeExams.Find(new Guid(SelectedEyeExam));
            }
            else
            {
                invoice.DispenseEyeExam = null;
            }
            invoice.InvoiceNumber = Session["NextInvoiceNumber"].ToString();
            decimal invoiceTotal = 0;

            foreach (InvoiceDetail invoiceDetail in ((List<InvoiceDetail>)Session["invoiceDetails"]))
            {
                InvoiceDetail newInvoiceDetail = new InvoiceDetail();
                newInvoiceDetail.Id = Guid.NewGuid();
                newInvoiceDetail.Added = invoiceDetail.Added;
                newInvoiceDetail.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                Product product = db.Products.Find(invoiceDetail.product.Id);
                newInvoiceDetail.product = product;
                newInvoiceDetail.UnitPrice = GetNegativePriceIfIncome(invoiceDetail.UnitPrice, product.ProductTypeEnum);
                newInvoiceDetail.VATRate = product.VATRate;
                newInvoiceDetail.CostPrice = product.Price;
                newInvoiceDetail.DiscountPercentage = invoiceDetail.DiscountPercentage;
                newInvoiceDetail.Quantity = invoiceDetail.Quantity;
                newInvoiceDetail.SpectacleNumber = invoiceDetail.SpectacleNumber;
                newInvoiceDetail.Description = invoiceDetail.Description;
                if (newInvoiceDetail.product.ProductTypeEnum == Product.ProductTypeList.Payment
                    || newInvoiceDetail.product.ProductTypeEnum == Product.ProductTypeList.NHS
                    || newInvoiceDetail.product.ProductTypeEnum == Product.ProductTypeList.Discount)
                {
                    if (newInvoiceDetail.product.Id != new Guid(WebConfigurationManager.AppSettings["CashPaymentProductId"].ToString()))
                    {
                        newInvoiceDetail.ReconciliationStatusEnum = Enums.ReconciliationStatus.Pending;
                    }
                    else
                    {
                        newInvoiceDetail.ReconciliationStatusEnum = Enums.ReconciliationStatus.Reconciled;
                    }
                }
                newInvoiceDetail.CostPrice = invoiceDetail.CostPrice;
                newInvoiceDetail.Dispensed = invoiceDetail.Dispensed;
                newInvoiceDetail.Code = invoiceDetail.Code;
                newInvoiceDetail.invoice = invoice;
                db.InvoiceDetails.Add(newInvoiceDetail);
                invoiceTotal += newInvoiceDetail.TotalIncVAT;
            }

            if (NHSVoucher != null && NHSVoucher != Enums.NHSVoucherGrade.None)
            {
                InvoiceDetail newInvoiceDetail = new InvoiceDetail();
                newInvoiceDetail.Id = Guid.NewGuid();
                newInvoiceDetail.Added = DateTime.Now;
                newInvoiceDetail.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                NHSVoucherCode nhsVoucherCode = NHSVoucherCode.NHSVoucherCodes.Where(n => n.Grade == NHSVoucher.ToString()).First();
                Product nhsVoucherProduct = db.Products.Find(nhsVoucherCode.Id);
                newInvoiceDetail.product = nhsVoucherProduct;
                newInvoiceDetail.UnitPrice = invoiceTotal > Math.Abs(nhsVoucherProduct.Price) ? nhsVoucherProduct.Price : -invoiceTotal;
                newInvoiceDetail.Quantity = 1;
                newInvoiceDetail.ReconciliationStatusEnum = Enums.ReconciliationStatus.Pending;
                newInvoiceDetail.invoice = invoice;
                db.InvoiceDetails.Add(newInvoiceDetail);
            }

            if (HttpContext.Session["customer"] != null)
            {
                invoice.customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);
                invoice.customer.LastUpdated = DateTime.Now;
                EyeExam LatestEyeExam = db.EyeExams.Where(ee => ee.customer.Id == invoice.customer.Id && ee.Deleted == null).OrderByDescending(ee => ee.TestDateAndTime).Take(1).FirstOrDefault();
                if (LatestEyeExam != null)
                {
                    LatestEyeExam.PDRight = PDRight;
                    LatestEyeExam.PDRightNear = PDRightNear;
                    LatestEyeExam.RHeight = RHeight;
                    LatestEyeExam.PDLeft = PDLeft;
                    LatestEyeExam.PDLeftNear = PDLeftNear;
                    LatestEyeExam.LHeight = LHeight;
                }
            }

            db.SaveChanges(); //save invoice details so that calculating totals works
            CalculateInvoiceTotals(invoice, db);

            db.SaveChanges();

            return RedirectToAction("Invoice", new { invoice.Id });
        }

        [HttpGet]
        public ActionResult Edit(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Customer customer = ((Customer)HttpContext.Session["customer"]);
            Invoice invoice = db.Invoices.Find(Id);
            List<InvoiceDetail> invoiceDetailsSaved = new InventoryController().GetInvoiceDetails(invoice, db).ToList();
            List<InvoiceDetailViewModel> invoiceDetailViewModels = new List<InvoiceDetailViewModel>();
            foreach (InvoiceDetail invoiceDetail in invoiceDetailsSaved)
            {
                invoiceDetailViewModels.Add(new InvoiceDetailViewModel
                {
                    Id = invoiceDetail.Id,
                    product = invoiceDetail.product,
                    UnitPrice = Math.Abs(invoiceDetail.UnitPrice),
                    VATRate = invoiceDetail.VATRate,
                    DiscountPercentage = invoiceDetail.DiscountPercentage,
                    Quantity = invoiceDetail.Quantity,
                    SpectacleNumber = invoiceDetail.SpectacleNumber,
                    Description = invoiceDetail.Description,
                    Dispensed = invoiceDetail.Dispensed,
                    Code = invoiceDetail.Code,
                    Added = invoiceDetail.Added
                });
            }
            Session["invoiceDetails"] = invoiceDetailsSaved;
            ViewBag.InvoiceDetails = invoiceDetailViewModels;
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            ViewBag.Products = new CustomersDataContext().Products.Where(p => p.Deleted == null
                && (p.company.Id == practice.company.Id || p.company == null)
                && p.ProductTypeEnum != Product.ProductTypeList.NHS).ToList();
            ViewBag.Customer = invoice.customer;

            if (invoice.customer != null)
            {
                EyeExam LatestEyeExam = db.EyeExams.Where(ee => ee.customer.Id == invoice.customer.Id && ee.Deleted == null).OrderByDescending(ee => ee.TestDateAndTime).Take(1).FirstOrDefault();
                if (LatestEyeExam != null)
                {
                    ViewBag.PDRight = LatestEyeExam.PDRight;
                    ViewBag.PDRightNear = LatestEyeExam.PDRightNear;
                    ViewBag.RHeight = LatestEyeExam.RHeight;
                    ViewBag.PDLeft = LatestEyeExam.PDLeft;
                    ViewBag.PDLeftNear = LatestEyeExam.PDLeftNear;
                    ViewBag.LHeight = LatestEyeExam.LHeight;
                }
            }
            ViewBag.ShowDomiciliaryFields = invoice.practice.ShowDomiciliaryFields;

            EyeExamsController eyeExamsController = new EyeExamsController();
            List<EyeExam> eyeExams = eyeExamsController.GetEyeExams(customer).Where(e => e.Deleted == null).OrderBy(e => e.TestDateAndTime).ToList();
            List<EyeExamViewModel> eyeExamViewModels = new List<EyeExamViewModel>();
            eyeExamViewModels.Add(new EyeExamViewModel { Id = Guid.Empty, ToStringEyeExam = "None" });

            foreach (EyeExam eyeExam in eyeExams)
            {
                eyeExamViewModels.Add(new EyeExamViewModel { Id = eyeExam.Id, ToStringEyeExam = eyeExam.ToStringEyeExam });
            }

            ViewBag.EyeExams = eyeExamViewModels;

            return View(invoice);
        }

        [HttpPost]
        public ActionResult Edit(Invoice invoice, float? PDRight, float? PDRightNear, float? RHeight, float? PDLeft, float? PDLeftNear, float? LHeight, string SelectedEyeExam) //todo: get NHS voucher code
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (((List<InvoiceDetail>)Session["invoiceDetails"]).Count == 0)
            {
                ViewBag.Error = "No products added";
                return Add();
            }

            CustomersDataContext db = new CustomersDataContext();
            Invoice existingInvoice = db.Invoices.Find(invoice.Id);

            decimal invoiceTotal = 0;

            InvoiceDetail nhsVoucher = null;

            foreach (InvoiceDetail invoiceDetail in ((List<InvoiceDetail>)Session["invoiceDetails"]))
            {
                InvoiceDetail existingInvoiceDetail = db.InvoiceDetails.Find(invoiceDetail.Id);
                if (existingInvoiceDetail != null)
                {
                    Product product = db.Products.Find(invoiceDetail.product.Id);
                    existingInvoiceDetail.product = product;
                    existingInvoiceDetail.UnitPrice = GetNegativePriceIfIncome(invoiceDetail.UnitPrice, product.ProductTypeEnum);
                    existingInvoiceDetail.VATRate = product.VATRate;
                    existingInvoiceDetail.DiscountPercentage = invoiceDetail.DiscountPercentage;
                    existingInvoiceDetail.Quantity = invoiceDetail.Quantity;
                    existingInvoiceDetail.ReconciliationStatusEnum = invoiceDetail.ReconciliationStatusEnum;
                    existingInvoiceDetail.invoice = existingInvoice;
                    existingInvoiceDetail.Deleted = invoiceDetail.Deleted;
                    existingInvoiceDetail.DeletedByUser = invoiceDetail.DeletedByUser != null ? db.ApplicationUsers.Find(invoiceDetail.DeletedByUser.Id) : null;
                    existingInvoiceDetail.SpectacleNumber = invoiceDetail.SpectacleNumber;
                    existingInvoiceDetail.Description = invoiceDetail.Description;
                    existingInvoiceDetail.CostPrice = invoiceDetail.CostPrice;
                    existingInvoiceDetail.Dispensed = invoiceDetail.Dispensed;
                    existingInvoiceDetail.Code = invoiceDetail.Code;
                    if (invoiceDetail.Added > DateTime.MinValue)
                    {
                        existingInvoiceDetail.Added = invoiceDetail.Added;
                    }
                    if (existingInvoiceDetail.product.ProductTypeEnum == Product.ProductTypeList.NHS)
                    {
                        nhsVoucher = existingInvoiceDetail; //todo: amend if necessary
                    }
                }
                else
                {
                    InvoiceDetail newInvoiceDetail = new InvoiceDetail();
                    newInvoiceDetail.Id = invoiceDetail.Id;
                    newInvoiceDetail.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                    Product product = db.Products.Find(invoiceDetail.product.Id);
                    newInvoiceDetail.product = product;
                    newInvoiceDetail.UnitPrice = GetNegativePriceIfIncome(invoiceDetail.UnitPrice, product.ProductTypeEnum);
                    newInvoiceDetail.DiscountPercentage = invoiceDetail.DiscountPercentage;
                    newInvoiceDetail.VATRate = product.VATRate;
                    newInvoiceDetail.CostPrice = product.Price;
                    newInvoiceDetail.Quantity = invoiceDetail.Quantity;
                    newInvoiceDetail.SpectacleNumber = invoiceDetail.SpectacleNumber;
                    newInvoiceDetail.Description = invoiceDetail.Description;
                    if (newInvoiceDetail.product.ProductTypeEnum == Product.ProductTypeList.Payment
                        || newInvoiceDetail.product.ProductTypeEnum == Product.ProductTypeList.NHS
                        || newInvoiceDetail.product.ProductTypeEnum == Product.ProductTypeList.Discount)
                    {
                        if (newInvoiceDetail.product.Id != new Guid(WebConfigurationManager.AppSettings["CashPaymentProductId"].ToString()))
                        {
                            newInvoiceDetail.ReconciliationStatusEnum = Enums.ReconciliationStatus.Pending;
                        }
                        else
                        {
                            newInvoiceDetail.ReconciliationStatusEnum = Enums.ReconciliationStatus.Reconciled;
                        }
                    }

                    newInvoiceDetail.CostPrice = invoiceDetail.CostPrice;
                    newInvoiceDetail.Dispensed = invoiceDetail.Dispensed;
                    newInvoiceDetail.Code = invoiceDetail.Code;
                    newInvoiceDetail.Added = invoiceDetail.Added;
                    newInvoiceDetail.invoice = existingInvoice;
                    db.InvoiceDetails.Add(newInvoiceDetail);
                }

                invoiceTotal += invoiceDetail.TotalIncVAT;
            }
            existingInvoice.Notes = invoice.Notes;
            existingInvoice.DiscountPercentage = invoice.DiscountPercentage;
            existingInvoice.MethodSentByEnum = invoice.MethodSentByEnum;
            existingInvoice.InvoiceDate = invoice.InvoiceDate;
            if (existingInvoice.customer != null)
            {
                existingInvoice.customer.LastUpdated = DateTime.Now;
            }

            if (existingInvoice.customer != null)
            {
                EyeExam LatestEyeExam = db.EyeExams.Where(ee => ee.customer.Id == existingInvoice.customer.Id && ee.Deleted == null).OrderByDescending(ee => ee.TestDateAndTime).Take(1).FirstOrDefault();
                if (LatestEyeExam != null)
                {
                    LatestEyeExam.PDRight = PDRight;
                    LatestEyeExam.PDRightNear = PDRightNear;
                    LatestEyeExam.RHeight = RHeight;
                    LatestEyeExam.PDLeft = PDLeft;
                    LatestEyeExam.PDLeftNear = PDLeftNear;
                    LatestEyeExam.LHeight = LHeight;
                }
            }

            if (SelectedEyeExam != null && Guid.Empty != new Guid(SelectedEyeExam))
            {
                existingInvoice.DispenseEyeExam = db.EyeExams.Find(new Guid(SelectedEyeExam));
            }
            else
            {
                existingInvoice.DispenseEyeExam = null;
            }

            db.SaveChanges(); //save invoice details so that calculating totals works
            CalculateInvoiceTotals(existingInvoice, db);

            db.SaveChanges();

            return RedirectToAction("Invoice", new { invoice.Id });
        }

        decimal GetNegativePriceIfIncome(decimal Price, Product.ProductTypeList? ProductType)
        {
            if (ProductType != null && (ProductType == Product.ProductTypeList.Discount || ProductType == Product.ProductTypeList.NHS || ProductType == Product.ProductTypeList.Payment))
            {
                return -Math.Abs(Price);
            }
            else
            {
                return Math.Abs(Price);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult AddInvoiceDetail([DataSourceRequest] DataSourceRequest request, InvoiceDetailViewModel invoiceDetailViewModel, string Mode, string Added)
        {
            CustomersDataContext db = new CustomersDataContext();

            InvoiceDetail newInvoiceDetail = new InvoiceDetail();
            List<InvoiceDetail> invoiceDetails = ((List<InvoiceDetail>)Session["invoiceDetails"]);

            if (Mode != "Create")
            {
                newInvoiceDetail.Id = Guid.NewGuid();
                newInvoiceDetail.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                Product product = db.Products.Find(invoiceDetailViewModel.product.Id);
                newInvoiceDetail.product = product;
                newInvoiceDetail.UnitPrice = invoiceDetailViewModel.UnitPrice;

                if (newInvoiceDetail.product.NegativeValue)
                {
                    newInvoiceDetail.DiscountPercentage = 0m;
                    newInvoiceDetail.Quantity = 1;
                    newInvoiceDetail.SpectacleNumber = null;
                }
                else
                {
                    newInvoiceDetail.DiscountPercentage = invoiceDetailViewModel.DiscountPercentage;
                    newInvoiceDetail.Quantity = invoiceDetailViewModel.Quantity;
                    newInvoiceDetail.SpectacleNumber = invoiceDetailViewModel.SpectacleNumber;
                }

                newInvoiceDetail.Description = invoiceDetailViewModel.Description;
                newInvoiceDetail.ReconciliationStatusEnum = Enums.ReconciliationStatus.Pending;
                newInvoiceDetail.CostPrice = product.CostPrice;
                newInvoiceDetail.Dispensed = invoiceDetailViewModel.Dispensed;
                newInvoiceDetail.Code = invoiceDetailViewModel.Code;
                newInvoiceDetail.Added = DateTime.ParseExact(Added, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                invoiceDetailViewModel.Added = newInvoiceDetail.Added;
                invoiceDetails.Add(newInvoiceDetail);
            }
            else
            {
                throw new ApplicationException("Error adding invoice line. Contact support.");
            }

            return Json(new[] { invoiceDetailViewModel }.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdateInvoiceDetail([DataSourceRequest] DataSourceRequest request, InvoiceDetailViewModel invoiceDetailViewModel)
        {
            if (invoiceDetailViewModel != null)
            {
                foreach (InvoiceDetail existingInvoiceDetail in ((List<InvoiceDetail>)Session["invoiceDetails"]))
                {
                    if (existingInvoiceDetail.Id == invoiceDetailViewModel.Id)
                    {
                        existingInvoiceDetail.product = invoiceDetailViewModel.product;
                        existingInvoiceDetail.UnitPrice = invoiceDetailViewModel.UnitPrice;
                        existingInvoiceDetail.Quantity = invoiceDetailViewModel.Quantity;
                        existingInvoiceDetail.DiscountPercentage = invoiceDetailViewModel.DiscountPercentage;
                        existingInvoiceDetail.SpectacleNumber = invoiceDetailViewModel.SpectacleNumber;
                        existingInvoiceDetail.Description = invoiceDetailViewModel.Description;
                        existingInvoiceDetail.Dispensed = invoiceDetailViewModel.Dispensed;
                        existingInvoiceDetail.Code = invoiceDetailViewModel.Code;
                        existingInvoiceDetail.Added = invoiceDetailViewModel.Added;
                    }
                }
            }

            return Json(new[] { invoiceDetailViewModel }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DeleteInvoiceDetail([DataSourceRequest] DataSourceRequest request, InvoiceDetailViewModel invoiceDetailViewModel)
        {
            if (invoiceDetailViewModel != null)
            {
                foreach (InvoiceDetail existingInvoiceDetail in ((List<InvoiceDetail>)Session["invoiceDetails"]))
                {
                    if (existingInvoiceDetail.Id == invoiceDetailViewModel.Id)
                    {
                        existingInvoiceDetail.Deleted = DateTime.Now;
                        existingInvoiceDetail.DeletedByUser = new CustomersDataContext().ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                    }
                }
            }

            return Json(new[] { invoiceDetailViewModel }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Invoices(DateTime? Start, DateTime? End)
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

            List<Invoice> invoices = db.Invoices.Where(i =>
                i.InvoiceDate > Start
                && i.InvoiceDate < End
                && i.Deleted == null
                && (i.customer.practice.Id == practice.Id
                || i.practice.Id == practice.Id)
                && ((i.customer != null && i.customer.Deleted == null) || i.customer == null)).ToList();

            List<InvoiceViewModel> invoiceViewModels = new List<InvoiceViewModel>();
            foreach (Invoice invoice in invoices)
            {
                invoiceViewModels.Add(new InvoiceViewModel
                {
                    Id = invoice.Id,
                    InvoiceDate = invoice.InvoiceDate,
                    CustomerName = invoice.customer != null ? invoice.customer.ToString() : "Counter sale",
                    Summary = invoice.ToString(),
                    InvoiceNumber = invoice.InvoiceNumber,
                    DiscountPercentage = invoice.DiscountPercentage,
                    DispenseStatus = invoice.DispenseStatus
                });
            }

            ViewBag.Invoices = invoiceViewModels;

            return View();
        }

        public ActionResult Delete(Invoice invoice)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Invoice existingInvoice = db.Invoices.Find(invoice.Id);
                existingInvoice.Deleted = DateTime.Now;
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                existingInvoice.DeletedByUser = user;
                db.SaveChanges();
                VisionDBController.AddAuditLogEntry(user, Enums.AuditLogEntryType.Invoice_Deleted, "Invoice Deleted", existingInvoice.Id, true);

                if (invoice.customer != null)
                {
                    return RedirectToAction("Customer", "Customers", new { existingInvoice.customer.Id });
                }
                else
                {
                    return RedirectToAction("Invoices", "Invoices");
                }
            }
            else
            {
                //todo: standardise error messages
                ViewBag.Error = "Unable to delete invoice.";
                return View();
            }
        }

        public ActionResult Reconcile(DateTime? Start, DateTime? End, bool? IncludePending, bool? IncludeFlagged, bool? IncludeReconciled)
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

            IncludePending = IncludePending == null ? true : IncludePending;
            IncludeFlagged = IncludeFlagged == null ? true : IncludeFlagged;
            IncludeReconciled = IncludeReconciled == null ? false : IncludeReconciled;

            ViewBag.IncludePending = IncludePending;
            ViewBag.IncludeFlagged = IncludeFlagged;
            ViewBag.IncludeReconciled = IncludeReconciled;

            return View();
        }

        public ActionResult _Read([DataSourceRequest] DataSourceRequest request, DateTime? Start, DateTime? End, bool? IncludePending, bool? IncludeFlagged, bool? IncludeReconciled)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            List<InvoiceDetail> invoiceDetails = db.InvoiceDetails.Where(id =>
                id.invoice.practice.Id == practice.Id
                && id.Deleted == null
                && id.invoice.Deleted == null
                && id.Added > Start
                && id.Added < End
                && id.product.Name.ToLower().Contains("cash") == false
                && (id.product.ProductTypeEnum == Product.ProductTypeList.Payment || id.product.ProductTypeEnum == Product.ProductTypeList.NHS)
                && (((bool)IncludePending && id.ReconciliationStatusEnum == Enums.ReconciliationStatus.Pending)
                || ((bool)IncludeFlagged && id.ReconciliationStatusEnum == Enums.ReconciliationStatus.Flagged)
                || ((bool)IncludeReconciled && id.ReconciliationStatusEnum == Enums.ReconciliationStatus.Reconciled))).ToList();

            List<InvoiceDetailViewModel> invoiceDetailViewModels = new List<InvoiceDetailViewModel>();
            foreach (InvoiceDetail invoiceDetail in invoiceDetails)
            {
                InvoiceDetail id = db.InvoiceDetails.Find(invoiceDetail.Id);
                invoiceDetailViewModels.Add(new InvoiceDetailViewModel
                {
                    Id = invoiceDetail.Id,
                    product = invoiceDetail.product,
                    CustomerName = invoiceDetail.invoice.customer != null ? invoiceDetail.invoice.customer.CustomerToString : "Counter sale invoice",
                    UnitPrice = invoiceDetail.UnitPrice,
                    VATRate = invoiceDetail.VATRate,
                    Quantity = invoiceDetail.Quantity,
                    DiscountPercentage = invoiceDetail.DiscountPercentage,
                    SpectacleNumber = invoiceDetail.SpectacleNumber,
                    Description = invoiceDetail.Description,
                    InvoiceId = invoiceDetail.invoice.Id,
                    Added = invoiceDetail.Added,
                    InvoiceNumber = invoiceDetail.invoice.InvoiceNumber,
                    ReconciliationStatus = invoiceDetail.ReconciliationStatusEnum.ToString(),
                    Dispensed = invoiceDetail.Dispensed,
                    Code = invoiceDetail.Code,
                    Status = invoiceDetail.product.NegativeValue == false && invoiceDetail.product.ProductTypeEnum != Product.ProductTypeList.Service ? invoiceDetail.StatusEnum.ToString().Replace('_', ' ') : ""
                });
            }

            return Json(invoiceDetailViewModels.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<InvoiceDetailViewModel> payments, Enums.ReconciliationStatus ReconciliationStatus)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<ModelError> errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
            ModelState.Clear();
            CustomersDataContext db = new CustomersDataContext();
            foreach (InvoiceDetailViewModel paymentViewModel in payments)
            {
                InvoiceDetail invoiceDetail = db.InvoiceDetails.Where(p => p.Id == paymentViewModel.Id).First();
                invoiceDetail.product = db.Products.Find(paymentViewModel.product.Id);
                invoiceDetail.ReconciliationStatusEnum = ReconciliationStatus;
                invoiceDetail.ReconciledStatusUpdated = DateTime.Now;
                invoiceDetail.Added = paymentViewModel.Added;
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);

                //invoiceDetail.ReconciledStatusUpdatedByUser = user; //todo: fix error when saving after assigning user
                invoiceDetail.ReconciliationNotes = string.Concat("Updated by ", user.UserToString); //todo: get notes field, this is currently empty
            }
            errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
            db.SaveChanges();

            return Json(ModelState.ToDataSourceResult());
        }

        public ActionResult EmailInvoice(Guid Id)
        {
            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            Invoice invoice = db.Invoices.Find(Id);

            FileStreamResult fileResult = this.Report(
                ReportFormat.PDF,
                new VisionDB.Controllers.ReportsController().GetReportPath(practice.Id, "Invoice"),
                new { invoiceId = Id });

            Stream fileStream = fileResult.FileStream;

            EmailHelper emailHelper = new EmailHelper();
            emailHelper.SendEmail(invoice.customer.Email, "Invoice", "Your invoice", practice.Email, fileStream, "invoice.pdf", false);

            TempData["Message"] = "Email sent";
            return RedirectToAction("Invoice", new { Id });
        }

        public ActionResult Dispensing(bool? IncludeNotOrdered, bool? IncludeAwaitingGoods, bool? IncludeAwaitingDispatchOrCollection, bool? IncludeAwaitingPayment,
            bool? IncludeComplete, bool? IncludeAdhoc,
            DateTime? Start, DateTime? End)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Session["customer"] = null;

            IncludeNotOrdered = IncludeNotOrdered == null ? true : IncludeNotOrdered;
            IncludeAwaitingGoods = IncludeAwaitingGoods == null ? true : IncludeAwaitingGoods;
            IncludeAwaitingDispatchOrCollection = IncludeAwaitingDispatchOrCollection == null ? true : IncludeAwaitingDispatchOrCollection;
            IncludeAwaitingPayment = IncludeAwaitingPayment == null ? true : IncludeAwaitingPayment;
            IncludeComplete = IncludeComplete == null ? false : IncludeComplete;
            IncludeAdhoc = IncludeAdhoc == null ? true : IncludeAdhoc;

            ViewBag.IncludeNotOrdered = IncludeNotOrdered;
            ViewBag.IncludeAwaitingGoods = IncludeAwaitingGoods;
            ViewBag.IncludeAwaitingDispatchOrCollection = IncludeAwaitingDispatchOrCollection;
            ViewBag.IncludeAwaitingPayment = IncludeAwaitingPayment;
            ViewBag.IncludeComplete = IncludeComplete;
            ViewBag.IncludeAdhoc = IncludeAdhoc;

            if (Start == null)
            {
                Start = DateTime.Now.Date.AddDays(-14);
                End = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            }

            End = End.Value.AddHours(23).AddMinutes(59);

            ViewBag.Start = Start.Value;
            ViewBag.End = End.Value;

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            List<Invoice> invoices = db.Invoices.Where(i =>
                i.Deleted == null
                && i.InvoiceDate > Start
                && i.InvoiceDate < End
                && i.practice.Id == practice.Id
                ).ToList().Where(i =>
                    (((bool)IncludeNotOrdered && i.DispenseStatus == Enums.InvoiceStatus.Not_Ordered)
                    || ((bool)IncludeAwaitingGoods && i.DispenseStatus == Enums.InvoiceStatus.Awaiting_Goods)
                    || ((bool)IncludeAwaitingDispatchOrCollection && i.DispenseStatus == Enums.InvoiceStatus.Awaiting_Collection_Or_Dispatch)
                    || ((bool)IncludeAwaitingPayment && i.DispenseStatus == Enums.InvoiceStatus.Awaiting_Payment)
                    || ((bool)IncludeComplete && i.DispenseStatus == Enums.InvoiceStatus.Complete))
                    && (((bool)IncludeAdhoc == false && i.customer != null) || ((bool)IncludeAdhoc == true))
                    ).OrderByDescending(i => i.InvoiceDate).ToList();

            List<InvoiceViewModel> invoiceViewModels = new List<InvoiceViewModel>();
            foreach (Invoice invoice in invoices)
            {
                invoiceViewModels.Add(new InvoiceViewModel
                {
                    Id = invoice.Id,
                    InvoiceDate = invoice.InvoiceDate,
                    CustomerName = invoice.customer != null ? invoice.customer.ToString() : "Counter sale",
                    Summary = invoice.ToString(),
                    InvoiceNumber = invoice.InvoiceNumber,
                    DiscountPercentage = invoice.DiscountPercentage,
                    DispenseStatus = invoice.DispenseStatus
                });
            }

            ViewBag.Invoices = invoiceViewModels;

            return View();
        }

        public ActionResult MarkAsSent(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Invoice invoice = db.Invoices.Find(Id);
            Enums.InvoiceDetailStatus ToStatus = Enums.InvoiceDetailStatus.Received;
            if (invoice.MethodSentByEnum == Enums.MethodSentBy.Collection)
            {
                ToStatus = Enums.InvoiceDetailStatus.Collected;
            }
            else if (invoice.MethodSentByEnum == Enums.MethodSentBy.Dispatch)
            {
                ToStatus = Enums.InvoiceDetailStatus.Dispatched;
            }
            List<InvoiceDetail> invoiceDetailsReceived = new InventoryController().GetInvoiceDetails(invoice, db).ToList()
                .Where(model => model.StatusEnum == Enums.InvoiceDetailStatus.Received || model.StatusEnum == Enums.InvoiceDetailStatus.Awaiting_Collection_or_Dispatch).ToList();
            ModelState.Clear();
            foreach (InvoiceDetail invoiceDetail in invoiceDetailsReceived)
            {
                InvoiceDetail existingInvoiceDetail = db.InvoiceDetails.Find(invoiceDetail.Id);
                if (existingInvoiceDetail.CreatedByUser == null)
                {
                    Product product = db.Products.Find(invoiceDetail.product.Id);
                    existingInvoiceDetail.product = product;
                    existingInvoiceDetail.UnitPrice = GetNegativePriceIfIncome(invoiceDetail.UnitPrice, product.ProductTypeEnum);
                    existingInvoiceDetail.VATRate = product.VATRate;
                    existingInvoiceDetail.DiscountPercentage = invoiceDetail.DiscountPercentage;
                    existingInvoiceDetail.Quantity = invoiceDetail.Quantity;
                    existingInvoiceDetail.ReconciliationStatusEnum = invoiceDetail.ReconciliationStatusEnum;
                    existingInvoiceDetail.Deleted = invoiceDetail.Deleted;
                    existingInvoiceDetail.DeletedByUser = invoiceDetail.DeletedByUser != null ? db.ApplicationUsers.Find(invoiceDetail.DeletedByUser.Id) : null;
                    existingInvoiceDetail.SpectacleNumber = invoiceDetail.SpectacleNumber;
                    existingInvoiceDetail.Description = invoiceDetail.Description;
                }
                //update inventory
                Inventory inventory = new Inventory();
                inventory.Id = Guid.NewGuid();
                inventory.practice = invoiceDetail.invoice.practice;
                inventory.CreatedByUser = user;
                inventory.product = invoiceDetail.product;
                inventory.Price = invoiceDetail.product.CostPrice.HasValue ? (decimal)invoiceDetail.product.CostPrice : 0m;
                inventory.VATRateValue = invoiceDetail.product.VATRate;
                inventory.Quantity = -invoiceDetail.Quantity;
                inventory.Added = DateTime.Now;
                db.Inventory.Add(inventory);
                existingInvoiceDetail.StatusEnum = ToStatus;
            }

            db.SaveChanges();

            TempData["Message"] = "Marked as " + ToStatus.ToString().ToLower();

            return RedirectToAction("Invoice", "Invoices", new { Id });
        }

        public ActionResult ExportToPDF(Guid Id, string SSRSReportName)
        {
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            return this.Report(
                ReportFormat.PDF,
                new VisionDB.Controllers.ReportsController().GetReportPath(practiceId, SSRSReportName),
                new { invoiceId = Id });
        }

        public void PopulateLastEyeExamsForInvoices()
        {
            CustomersDataContext db = new CustomersDataContext();
            List<Invoice> invoices = db.Invoices.Where(i => i.customer != null && i.Deleted == null).ToList();
            foreach (Invoice invoice in invoices)
            {
                EyeExam LastEyeExam = new EyeExamsController().GetLatestEyeExam(db, invoice.customer, invoice.InvoiceDate);
                if (invoice.customer != null && invoice.DispenseEyeExam != null && LastEyeExam != null)
                {
                    invoice.DispenseEyeExam = LastEyeExam;
                }
            }

        }

        public void CalculateInvoiceTotals(Invoice invoice, CustomersDataContext db)
        {
            IList<InvoiceDetail> InvoiceDetails = new Controllers.InventoryController().GetInvoiceDetails(invoice, db);

            //BalanceExcVAT
            decimal balanceExVAT = 0;
            if (InvoiceDetails != null && InvoiceDetails.Count > 0)
            {
                foreach (InvoiceDetail invoiceDetail in InvoiceDetails)
                {
                    if (invoiceDetail.TotalExcVAT > 0)
                    {
                        balanceExVAT += invoiceDetail.TotalExcVAT * (1 - (invoice.DiscountPercentage / 100));
                    }
                    else
                    {
                        balanceExVAT += invoiceDetail.TotalExcVAT;
                    }
                }
            }
            invoice.BalanceExcVAT = balanceExVAT;

            //TotalExcVAT
            decimal totalExcVAT = 0;
            if (InvoiceDetails != null && InvoiceDetails.Count > 0)
            {
                foreach (InvoiceDetail invoiceDetail in InvoiceDetails)
                {
                    if (invoiceDetail.TotalExcVAT > 0)
                    {
                        totalExcVAT += invoiceDetail.TotalExcVAT;
                    }
                }
            }
            invoice.TotalExcVAT = totalExcVAT * (1 - (invoice.DiscountPercentage / 100));

            //TotalIncVAT
            decimal total = 0;
            if (InvoiceDetails != null && InvoiceDetails.Count > 0)
            {
                foreach (InvoiceDetail invoiceDetail in InvoiceDetails)
                {
                    if (invoiceDetail.TotalExcVAT > 0)
                    {
                        total += invoiceDetail.TotalIncVAT;
                    }
                }
            }
            invoice.TotalIncVAT = total * (1 - (invoice.DiscountPercentage / 100));

            //BalanceIncVAT
            decimal balance = 0;
            if (InvoiceDetails != null && InvoiceDetails.Count > 0)
            {
                foreach (InvoiceDetail invoiceDetail in InvoiceDetails)
                {
                    if (invoiceDetail.TotalIncVAT > 0)
                    {
                        balance += invoiceDetail.TotalIncVAT * (1 - (invoice.DiscountPercentage / 100));
                    }
                    else
                    {
                        balance += invoiceDetail.TotalIncVAT;
                    }
                }
            }
            invoice.BalanceIncVAT = balance;
        }

        internal void UpdateTotalsForInvoices()
        {
            CustomersDataContext db = new CustomersDataContext();
            List<Invoice> invoices = db.Invoices.Where(i => i.Deleted == null).ToList();
            foreach (Invoice invoice in invoices)
            {
                CalculateInvoiceTotals(invoice, db);
            }

            db.SaveChanges();
        }

        //DRC INFOTECH
        // PROCESS REFUND AND RE_CALCULATE INVOICE TOTALS
        public ActionResult ProcessRefund(string id, decimal amount)
        {
            try
            {
                if (HttpContext.Session["user"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                CustomersDataContext db = new CustomersDataContext();
                Invoice _invoice = db.Invoices.Where(z => z.Id.ToString() == id).FirstOrDefault();
                if (_invoice != null)
                {
                    Product _product = db.Products.Find("C8230C71-BD30-4331-9AA1-224361993B79");
                    InvoiceDetail _invoiceDetail = new InvoiceDetail();
                    _invoiceDetail.Id = Guid.NewGuid();
                    _invoiceDetail.product = _product;
                    _invoiceDetail.Added = DateTime.ParseExact(DateTime.Now.ToString(), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                    _invoiceDetail.Quantity = 1;
                    _invoiceDetail.invoice = _invoice;
                    _invoiceDetail.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                    db.InvoiceDetails.Add(_invoiceDetail);
                    db.SaveChanges();
                }
                return RedirectToAction("Invoice", new { _invoice.Id });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
