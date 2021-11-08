using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HTML_Editor
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            HTML_Editor.Controllers.RunController.URL_ID_Session = HTML_Editor.Controllers.RunController.GenerateRandomFileID();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "root",
                url: "{URLGen}",
                defaults: new { controller = "Run", action = "Index", URLGen = HTML_Editor.Controllers.RunController.URL_ID_Session }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{URLGen}",
                defaults: new { controller = "Run", action = "Index", URLGen = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Default2",
                url: "{URLGen}",
                defaults: new { controller = "Run", action = "Index" }
            );

        }
    }
}
