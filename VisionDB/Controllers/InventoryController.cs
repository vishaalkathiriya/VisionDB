using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisionDB.Helper;
using VisionDB.Models;
using Kendo.Mvc.Extensions;

namespace VisionDB.Controllers
{
    [Authorize]
    public class InventoryController : VisionDBController
    {
        const int MaxSearchProductsCount = 20;

        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Search", "Inventory");
        }

        public ActionResult Search(string Search)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;

            IEnumerable<Product> results = null;

            if (!string.IsNullOrWhiteSpace(Search))
            {
                results = (from p in db.Products
                           where p.company.Id == companyId
                             && p.Deleted == null
                             && (p.Name.ToLower().Contains(Search.ToLower())
                             || p.Code.Replace(" ", "").ToLower().Contains(Search.Replace(" ", "").ToLower())
                             || p.Description.Contains(Search)
                             || p.ReferenceNumber.Contains(Search)
                             || p.ProductTypeEnum.ToString().ToLower().Contains(Search.ToLower()))
                           select p).Take(MaxSearchProductsCount + 1);
            }
            else
            {
                results = from p in db.Products
                          where p.Id == null
                          select p;
            }

            var recentProducts = (from p in db.Products
                             where p.company.Id == companyId
                             && p.Deleted == null
                             orderby p.LastUpdated descending
                             select p).Take(10);
            ViewBag.RecentProducts = recentProducts;
            if (results.Count() > MaxSearchProductsCount)
            {
                ViewBag.MoreThanMaxSearchProductsCount = true;
            }
            ViewBag.SearchText = Search;
            return View(results.Take(20));
        }

        public ActionResult Product(Guid id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }


        [HttpGet]
        public ActionResult Add()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Manufacturers = GetManufacturers();
            ViewBag.LensTypes = GetLensTypes();
            ViewBag.Company = new CustomersDataContext().Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product product, string StockLevel)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int stockLevel = 0;
            if (StockLevel.Length > 0)
            {
                if (int.TryParse(StockLevel, out stockLevel) == false)
                {
                    ModelState.AddModelError("StockLevel", "Invalid stock level");
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                Practice practice = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId);
                ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
   
                Product newProduct = new Product();
                newProduct.Id = Guid.NewGuid();
                newProduct.Name = product.Name;
                newProduct.Code = product.Code;
                newProduct.ProductTypeEnum = product.ProductTypeEnum;
                newProduct.Price = product.Price;
                newProduct.VATRate = product.VATRate;
                newProduct.CreatedByUser = user;
                newProduct.LastUpdated = DateTime.Now;
                newProduct.company = practice.company;
                newProduct.FrequentlyUsed = product.FrequentlyUsed;
                newProduct.CostPrice = product.CostPrice;
                newProduct.Manufacturer = product.Manufacturer;
                newProduct.LensType = product.LensType;

                db.Products.Add(newProduct);

                if (stockLevel > 0)
                {
                    Inventory inventory = new Inventory();
                    inventory.Id = Guid.NewGuid();
                    inventory.practice = practice;
                    inventory.CreatedByUser = user;
                    inventory.product = newProduct;
                    inventory.Price = product.CostPrice.HasValue ? (decimal)product.CostPrice : 0m; 
                    inventory.VATRateValue = newProduct.VATRate;
                    inventory.Quantity = stockLevel;
                    inventory.Added = DateTime.Now;
                    db.Inventory.Add(inventory);
                }
                
                db.SaveChanges();
                return RedirectToAction("Search");
            }

            return Add();
        }


        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Manufacturers = GetManufacturers();
            ViewBag.LensTypes = GetLensTypes();
            CustomersDataContext db = new CustomersDataContext();

            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();

                db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                product.LastUpdated = DateTime.Now;

                db.SaveChanges();
                return RedirectToAction("Search");
            }
            return View(product);
        }

        private List<string> GetManufacturers()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            List<string> manufacturers = new List<string>();
            IEnumerable<Product> existingProducts = db.Products.Where(p => p.company.Id == companyId && p.Deleted == null && p.Manufacturer != null && p.Manufacturer.Trim().Length > 0);

            foreach (Product existingProduct in existingProducts)
            {
                if (!manufacturers.Contains(existingProduct.Manufacturer))
                {
                    manufacturers.Add(existingProduct.Manufacturer);
                }
            }
            return manufacturers.OrderBy(m => m.ToString()).ToList();
        }

        private List<string> GetLensTypes()
        {
            CustomersDataContext db = new CustomersDataContext();
            Guid companyId = db.Practices.Find(((ApplicationUser)HttpContext.Session["user"]).practiceId).company.Id;
            List<string> lensTypes = new List<string>();
            IEnumerable<Product> existingProducts = db.Products.Where(p => p.company.Id == companyId && p.Deleted == null && p.LensType != null && p.LensType.Trim().Length > 0);

            foreach (Product existingProduct in existingProducts)
            {
                if (!lensTypes.Contains(existingProduct.LensType))
                {
                    lensTypes.Add(existingProduct.LensType);
                }
            }
            return lensTypes.OrderBy(m => m.ToString()).ToList();
        }

        public ActionResult Delete(Product product)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            Product existingProduct = db.Products.Find(product.Id);
            existingProduct.LastUpdated = DateTime.Now;
            existingProduct.Deleted = DateTime.Now;
            existingProduct.DeletedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);

            db.SaveChanges();

            return RedirectToAction("Search", "Inventory");
        }

        protected override void Dispose(bool disposing)
        {
            //CustomersDataContext db = ((CustomersDataContext)HttpContext.Session["customersDataContext"]);
            CustomersDataContext db = new CustomersDataContext();
            db.Dispose();
            base.Dispose(disposing);
        }

        internal int GetPracticeStockLevel(Guid productId, Guid practiceId)
        {
            //CustomersDataContext db = ((CustomersDataContext)HttpContext.Session["customersDataContext"]);
            CustomersDataContext db = new CustomersDataContext();
            int stockLevel = 0;

            var results = from row in db.Inventory
                          select row;

            results = results.Where(i => i.product.Id == productId
                && (i.practice.Id == practiceId
                || (i.customer.practice.Id == practiceId)));

            foreach (Inventory inv in results)
            {
                stockLevel += inv.Quantity;
            }

            return stockLevel;
        }

        internal int GetCompanyStockLevel(Guid productId)
        {
            int stockLevel = 0;
            //CustomersDataContext db = ((CustomersDataContext)HttpContext.Session["customersDataContext"]);
            CustomersDataContext db = new CustomersDataContext();
            var results = from row in db.Inventory
                          select row;

            results = results.Where(i => i.product.Id == productId);

            foreach (Inventory inv in results)
            {
                stockLevel += inv.Quantity;
            }

            return stockLevel;
        }


        public IList<InvoiceDetail> GetInvoiceDetails(Invoice invoice, CustomersDataContext db)
        {
            var results = from row in db.InvoiceDetails.Where(c => c.invoice.Id == invoice.Id && c.UnitPrice >= 0 && c.Deleted == null).OrderBy(c => c.Added)
                          select row;

            var payments = from row in db.InvoiceDetails.Where(c => c.invoice.Id == invoice.Id && c.UnitPrice < 0 && c.Deleted == null).OrderBy(c => c.Added)
                           select row;

            IList<InvoiceDetail> invoiceDetails = results.Union(payments).ToList();

            return invoiceDetails;
        }

        public JsonResult GetProducts(string Search)
        {
            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            Invoice invoice = null;
            if (HttpContext.Session["invoice"] != null)
            {
                invoice = (Invoice)HttpContext.Session["invoice"];
            }
            Customer customer = null;
            if (HttpContext.Session["customer"] != null)
            {
                customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);
            }

            List<Product> products = null; 
            if (Search.Length == 0)
            {
                products = db.Products.Where(p => p.Deleted == null
                && (p.company.Id == practice.company.Id || p.company == null)
                && p.FrequentlyUsed == true).ToList().Where(p => customer != null || (customer == null && p.CanBeAddedToAdhocOrder)).OrderBy(p => p.Name).Take(30).ToList();

                if (invoice != null)
                {
                    List<InvoiceDetail> invoiceDetails = db.InvoiceDetails.Where(id => id.invoice.Id == invoice.Id && id.Deleted == null).ToList();

                    foreach (InvoiceDetail invoiceDetail in invoiceDetails)
                    {
                        if (!products.Contains(invoiceDetail.product))
                        {
                            products.Add(invoiceDetail.product);
                        }
                    }

                    products = products.OrderBy(p => p.Name).ToList();
                }
            }
            else
            {
                products = db.Products.Where(p => p.Deleted == null
                && (p.company.Id == practice.company.Id || p.company == null)
                && (p.Name.Contains(Search)
                || p.Description.Contains(Search)
                || p.ReferenceNumber.Contains(Search)
                || p.Code.Contains(Search))).ToList().Where(p => customer != null || (customer == null && p.CanBeAddedToAdhocOrder)).OrderBy(p => p.Name).Take(30).ToList();
            }

            return Json(products, JsonRequestBehavior.AllowGet);
        }


        #region GoodsIn

        public ActionResult GoodsIn(bool? IncludeNotOrdered, bool? IncludeOrdered, bool? IncludeReceived, bool? IncludeNotified)
        {
            IncludeNotOrdered = IncludeNotOrdered == null ? true : IncludeNotOrdered;
            IncludeOrdered = IncludeOrdered == null ? true : IncludeOrdered;
            IncludeReceived = IncludeReceived == null ? false : IncludeReceived;
            IncludeNotified = IncludeNotified == null ? false : IncludeNotified;

            ViewBag.IncludeNotOrdered = IncludeNotOrdered;
            ViewBag.IncludeOrdered = IncludeOrdered;
            ViewBag.IncludeReceived = IncludeReceived;
            ViewBag.IncludeNotified = IncludeNotified;
            return View();
        }

        public ActionResult CustomerOrders(Guid Id)
        {
            CustomersDataContext db = new CustomersDataContext();
            Customer customer = db.Customers.Find(Id);
            return View(customer);
        }


        public ActionResult _Read([DataSourceRequest] DataSourceRequest request,
            bool? IncludeNotOrdered, bool? IncludeOrdered, bool? IncludeReceived, bool? IncludeNotified, bool? IncludeAdhoc)
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
                && (((bool)IncludeNotOrdered && id.StatusEnum == Enums.InvoiceDetailStatus.Not_Ordered)
                || ((bool)IncludeOrdered && id.StatusEnum == Enums.InvoiceDetailStatus.Ordered)
                || ((bool)IncludeReceived && id.StatusEnum == Enums.InvoiceDetailStatus.Received)
                || ((bool)IncludeNotified && id.StatusEnum == Enums.InvoiceDetailStatus.Notified))
                ).ToList().Where(id => id.product.GoodsInProduct).OrderByDescending(id => id.Added).ToList();


            List<InvoiceDetailViewModel> invoiceDetailViewModels = new List<InvoiceDetailViewModel>();
            foreach (InvoiceDetail invoiceDetail in invoiceDetails)
            {
                if (invoiceDetail.product.NegativeValue == false) //todo: add this where clause to linq above. 
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
                        Status = invoiceDetail.product.NegativeValue == false && invoiceDetail.product.ProductTypeEnum != VisionDB.Models.Product.ProductTypeList.Service ? invoiceDetail.StatusEnum.ToString().Replace('_', ' ') : ""
                    });
                }
            }

            return Json(invoiceDetailViewModels.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<InvoiceDetailViewModel> invoiceDetailViewModels, Enums.InvoiceDetailStatus InvoiceDetailStatus)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<ModelError> errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
            ModelState.Clear();
            CustomersDataContext db = new CustomersDataContext();
            ApplicationUser user = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            foreach (InvoiceDetailViewModel invoiceDetailViewModel in invoiceDetailViewModels)
            {
                InvoiceDetail invoiceDetail = db.InvoiceDetails.Where(p => p.Id == invoiceDetailViewModel.Id).First();
                invoiceDetail.product = db.Products.Find(invoiceDetailViewModel.product.Id);
                if (InvoiceDetailStatus == Enums.InvoiceDetailStatus.Received && invoiceDetail.StatusEnum != Enums.InvoiceDetailStatus.Received)
                {
                    //book goods into inventory
                    Inventory inventory = new Inventory();
                    inventory.Id = Guid.NewGuid();
                    inventory.practice = invoiceDetail.invoice.practice;
                    inventory.CreatedByUser = user;
                    inventory.product = invoiceDetail.product;
                    inventory.Price = invoiceDetail.product.CostPrice.HasValue ? (decimal)invoiceDetail.product.CostPrice : 0m;
                    inventory.VATRateValue = invoiceDetail.product.VATRate;
                    inventory.Quantity = invoiceDetail.Quantity;
                    inventory.Added = DateTime.Now;
                    db.Inventory.Add(inventory);
                }
                invoiceDetail.StatusEnum = InvoiceDetailStatus;
                invoiceDetail.ReconciledStatusUpdated = DateTime.Now;
                
                //invoiceDetail.ReconciledStatusUpdatedByUser = user; //todo: fix error when saving after assigning user
                invoiceDetail.ReconciliationNotes = string.Concat("Updated by ", user.UserToString); //todo: get notes field, this is currently empty
            }
            errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
            db.SaveChanges();

            return Json(ModelState.ToDataSourceResult());
        }
        
        #endregion

        public ActionResult _GetCustomerOrders([DataSourceRequest] DataSourceRequest request, Guid Id)
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
                && id.invoice.customer.Id == Id).ToList().Where(id => id.product.GoodsInProduct && id.product.NegativeValue == false).ToList();

            List<InvoiceDetailViewModel> invoiceDetailViewModels = new List<InvoiceDetailViewModel>();
            foreach (InvoiceDetail invoiceDetail in invoiceDetails)
            {
                InvoiceDetail id = db.InvoiceDetails.Find(invoiceDetail.Id);
                invoiceDetailViewModels.Add(new InvoiceDetailViewModel
                {
                    Id = invoiceDetail.Id,
                    product = invoiceDetail.product,
                    CustomerName = invoiceDetail.invoice.customer.CustomerToString,
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
                    Status = invoiceDetail.product.NegativeValue == false && invoiceDetail.product.ProductTypeEnum != VisionDB.Models.Product.ProductTypeList.Service ? invoiceDetail.StatusEnum.ToString().Replace('_', ' ') : ""
                });
            }

            return Json(invoiceDetailViewModels.ToDataSourceResult(request));
        }
    }
}