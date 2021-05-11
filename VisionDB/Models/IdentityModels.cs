using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisionDB.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class ApplicationUser : IdentityUser
    {
        public Guid practiceId { get; set; }
        public string Title { get; set; }
        public string Firstnames { get; set; }
        public string Surname { get; set; }
        [DisplayName("List number")]
        public string ListNumber { get; set; }
        public string SupportCode { get; set; }
        public Enums.DefaultHomePage DefaultHomePageEnum { get; set; }

        [DisplayName("GOC number")]
        public string GOCNumber { get; set; }

        public bool Hidden { get; set; }

        [DisplayName("Prevent dragging appointments")]
        public bool PreventDraggingAppointments { get; set; }

        [DisplayName("Automatically resize calendar")]
        public bool AutomaticallyResizeCalendar { get; set; }

        public string UserToString 
        {
            get
            {
                return string.Format("{0} {1} {2}", Title, Firstnames, Surname);
            }
        }

        public string RolesToString 
        { 
            get
            {
                string roles = "";
                foreach (Enums.UserRoles role in VDBRoles)
                {
                    roles += role.ToString().Replace('_', ' ') + ", ";
                }

                return roles.Trim().Trim(',');
            }
        }

        public List<Enums.UserRoles> VDBRoles { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
    }
}