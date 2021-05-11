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
    public class KnowledgeBaseController : VisionDBController
    {
        public ActionResult Products()
        {
            return View();
        }
    }
}