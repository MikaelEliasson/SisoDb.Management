using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace SisoDb.Management
{
    public static class Configuration
    {
        static Configuration()
        {
            TypeMappings = new Dictionary<Type, TypeMapping>();
        }

        internal static IDictionary<Type, TypeMapping> TypeMappings { get; set; }
        public static ISisoDatabase DB;
        /// <summary>
        /// Set a callback that is used to authorize if the user can access the management pages. Should return true if the user has access.
        /// </summary>
        public static Func<string, bool> Authorize;
        /// <summary>
        /// Set at which url the management should be hosted. Should end with a trailing slash. Default value =  "siso-db-management/"
        /// </summary>
        public static string Prefix = "siso-db-management/";

        public static void AddTypeMapping<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class
        {
            TypeMappings.Add(typeof(TInterface), new TypeMapping<TInterface, TImplementation>());
        }

        public static void AddTypeMapping<TImplementation>()
            where TImplementation : class
        {
            TypeMappings.Add(typeof(TImplementation), new TypeMapping<TImplementation, TImplementation>());
        }

        public static void Init()
        {

            //Copied from: https://github.com/SamSaffron/MiniProfiler/blob/master/StackExchange.Profiling/UI/MiniProfilerHandler.cs#L69

            var routes = RouteTable.Routes;
            var handler = new SisoDbManagementHandler();

            using (routes.GetWriteLock())
            {
                var route = new Route(Prefix + "{filename}", handler)
                {
                    // we have to specify these, so no MVC route helpers will match, e.g. @Html.ActionLink("Home", "Index", "Home")
                    Defaults = new RouteValueDictionary(new { controller = "SisoDbManagementHandler", action = "ProcessRequest" }),
                    Constraints = new RouteValueDictionary(new { controller = "SisoDbManagementHandler", action = "ProcessRequest" })
                };

                routes.Insert(0, route);
            }
        }

        public static void Clear()
        {
            DB = null;
            TypeMappings.Clear();

            var routes = RouteTable.Routes;

            using (routes.GetWriteLock())
            {
                if (routes.Any())
                {
                    routes.RemoveAt(0);
                }
            }
        }
    }
}
