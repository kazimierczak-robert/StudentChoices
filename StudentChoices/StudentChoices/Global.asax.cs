using StudentChoices.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace StudentChoices
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DateTime date = DateTime.Now;
            Application["RecActive"] = false;
            Application["RecStop"] = date;
            Application["RecStopString"] = date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            Application["AfterRec"] = false;
            Application["ShareResults"] = false;
        }
    }
}
