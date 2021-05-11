using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace VisionDB.Models
{
    public class CustomersDataContext : DbContext
    {
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Practice> Practices { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<TestFrequency> TestFrequencies { get; set; }
        public DbSet<EyeExam> EyeExams { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<NHSVoucherCode> NHSVoucherCodes { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<SMSTransaction> SMSInventory { get; set; }
        public DbSet<AuditLogEntry> AuditLog { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<CustomerTag> CustomerTags { get; set; }
        public DbSet<IOP> IOPs { get; set; }
        public DbSet<AppointmentType> AppointmentTypes { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<RecallTemplate> RecallTemplates { get; set; }
        public DbSet<RecallDocument> RecallDocuments { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<PostcodeLookup> PostcodeLookups { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        static CustomersDataContext()
        {
            Database.SetInitializer<CustomersDataContext>(null);
        }


        //map model to specific table
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });

            modelBuilder.Entity<Inventory>().ToTable("Inventory"); //without this Inventory model is automatically mapped to Inventories
            modelBuilder.Entity<SMSTransaction>().ToTable("SMSInventory");
            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<AuditLogEntry>().ToTable("AuditLog");
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }
    }
}