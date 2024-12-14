using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SomiodAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Remove JSON formatter
            config.Formatters.JsonFormatter.SupportedMediaTypes.Clear();

            // Ensure XML formatter is enabled
            config.Formatters.XmlFormatter.UseXmlSerializer = true;

            // Define routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
