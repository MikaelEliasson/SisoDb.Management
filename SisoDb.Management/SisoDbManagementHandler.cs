using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using ServiceStack.Text;

namespace SisoDb.Management
{
    public class SisoDbManagementHandler : IRouteHandler, IHttpHandler
    {

        private const string Success = "{ \"Success\" : true}";

        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Should mainly be changed for testing
        /// </summary>
        public static IContextAdapter Context = new HttpContextAdapter(); 

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                //For security we force Authorize to be set
                if (Configuration.Authorize == null)
                {
                    Context.SetStatusCode(500);
                    Context.Write("For security reasons you must explixitly set Configuration.Authorize before you can use SisoDb.Management.");
                    return;
                }

                string output = "";
                string path = Context.GetAppRelativeCurrentExecutionFilePath();
                var actionName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();

                if(!Configuration.Authorize(actionName)){
                    Context.SetStatusCode(401);
                    Context.Write("You do not have the required permissions.");
                    return;
                }

                Context.SetStatusCode(200); //Set this before so the methods can override it
                switch (actionName)
                {
                    case "style":
                        Context.SetContentType("text/css");
                        output = GetResource("style.css");
                        break;
                    case "scripts":
                        Context.SetContentType("text/javascript");
                        output = GetResource("jquery-1.8.2.min.js") + GetResource("knockout-2.2.0.js") + GetResource("sisoManagement.js") + GetResource("tab.js");
                        break;
                    case "data":
                        output = GetData();
                        break;
                    case "page":
                        Context.SetContentType("text/html");;
                        output = GetResource("page.html");
                        break;
                    case "query":
                        output = Query();
                        break;
                    case "deletebyquery":
                        output = DeleteByQuery();
                        break;
                    case "delete":
                        output = Delete();
                        break;
                    case "update":
                        output = Update();
                        break;
                    case "insert":
                        output = Insert();
                        break;
                    case "insertschema":
                        output = InsertSchema();
                        break;
                    case "entity":
                        output = Entity();
                        break;
                    case "regenerateindexes":
                        output = RegenerateIndexes();
                        break;
                    default:
                        Context.SetStatusCode(404);
                        output = "Method [" + actionName + "] not found";
                        break;
                }
                
                Context.Write(output);
            }
            catch (Exception e)
            {
                Context.SetStatusCode(500);
                Context.Write(e.Message);
            }
        }

        private string Entity()
        {
            Context.SetContentType("text/html");
            var typeMatch = GetTypeMapping();
            object id = Context.GetFormValue("entityId");

            if (typeMatch.GetIdType() == typeof(Guid))
            {
                id = Guid.Parse((string)id);
            }
            
            return Configuration.DB.UseOnceTo().GetByIdAsJson(typeMatch.Contract, id);
        }

        private string Delete()
        {
            var typeMatch = GetTypeMapping();
            object id = Context.GetFormValue("entityId");

            if (typeMatch.GetIdType() == typeof(Guid))
            {
                id = Guid.Parse((string)id);
            }

            Configuration.DB.UseOnceTo().DeleteById(typeMatch.Contract, id);

            return "";
        }

        private string Query()
        {
            var typeMatch = GetTypeMapping();
            var spec = GetQuerySpec(typeMatch);
            Context.SetContentType("application/json");
            var queryResult = typeMatch.GetJsonList(spec);
            return "{ \"TotalMatches\":" + queryResult.TotalMatches + ", \"Entities\":[" + string.Join(",", queryResult.Entities) + "]}";
        }

        private string DeleteByQuery()
        {
            var typeMatch = GetTypeMapping();
            var spec = GetQuerySpec(typeMatch);

            Context.SetContentType("text/html");
            return typeMatch.DeleteByQuery(spec).ToString();
        }

        private string Update()
        {
            Context.SetContentType("application/json");

            var typeMatch = GetTypeMapping();
            var id = Context.GetFormValue("entityId");
            var modifiedEntity = Context.GetFormValue("modifiedEntity");
            if (string.IsNullOrWhiteSpace(modifiedEntity))
            {
                return "{ \"Error\" : \"The modified json was empty. Aborting update!\"}";
            }

            //Should probably verify that noone touched the ID in the json here
            var entity = JsonSerializer.DeserializeFromString(modifiedEntity, typeMatch.Implementation);
            Configuration.DB.UseOnceTo().Update(typeMatch.Contract, entity);

            return Success;
        }

        private string Insert()
        {
            Context.SetContentType("application/json");

            var typeMatch = GetTypeMapping();

            var json = Context.GetFormValue("json");
            if (string.IsNullOrWhiteSpace(json))
            {
                return "{ \"Error\" : \"The json was empty. Aborting insert!\"}";
            }


            json = json.Trim();
            if (json.StartsWith("["))
            {
                var entities = JsonSerializer.DeserializeFromString(json, typeMatch.GetCollectionType()) as IEnumerable<object>;
                Configuration.DB.UseOnceTo().InsertMany(typeMatch.Contract, entities);
            }
            else
            {
                var entity = JsonSerializer.DeserializeFromString(json, typeMatch.Implementation);
                Configuration.DB.UseOnceTo().Insert(typeMatch.Contract, entity);
            }

            return Success;
        }

        private string RegenerateIndexes()
        {
            var typeMatch = GetTypeMapping();

            var method = Configuration.DB.Maintenance.GetType().GetMethods().First(m => m.Name == "RegenerateQueryIndexesFor" && m.GetGenericArguments().Count() == 2);

            var genericMethod = method.MakeGenericMethod(typeMatch.Contract, typeMatch.Implementation);

            genericMethod.Invoke(Configuration.DB.Maintenance, null);

            return Success;
        }

        private string InsertSchema()
        {
            var typeMatch = GetTypeMapping();

            //This whole part is a hack and it only works if no query has been run on the entity earlier as siso cache that.
            //BEtter support for this will have to be added to Siso first
            var settings = Configuration.DB.Settings;
            var oldAllowUpsertsOfSchemas = settings.AllowUpsertsOfSchemas;
            var oldSynchronizeSchemaChanges = settings.SynchronizeSchemaChanges;
            settings.AllowUpsertsOfSchemas = true;
            settings.SynchronizeSchemaChanges = true;
            Configuration.DB.UpsertStructureSet(typeMatch.Contract);
            settings.AllowUpsertsOfSchemas = oldAllowUpsertsOfSchemas;
            settings.SynchronizeSchemaChanges = oldSynchronizeSchemaChanges;
            return Success;
        }

        private string GetData()
        {
            var assembly = typeof(SisoDbManagementHandler).Assembly;
            var v = AssemblyName.GetAssemblyName(assembly.Location).Version;
            var version = string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);

            Context.SetContentType("application/json");
            return "var data = { \"Version\": \"" + version  + "\", \"Entities\":" + JsonSerializer.SerializeToString(Configuration.TypeMappings.Select(m => m.Value)) + "};";
        }

        private QuerySpec GetQuerySpec(TypeMapping typeMapping)
        {
            var spec = new QuerySpec();

            var setup = Context.GetFormValue("setup");
            if (!string.IsNullOrWhiteSpace(setup))
            {
                spec.Setup.Add(setup);
            }
            spec.Predicate = Context.GetFormValue("predicate");

            var orderBy = Context.GetFormValue("orderby");
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                spec.OrderBy = orderBy;
            }

            spec.SortOrder = Context.GetFormValue("sortorder") == "desc" ? SortOrder.Desc : SortOrder.Asc;

            var page = Context.GetFormValue("page");
            if (!string.IsNullOrWhiteSpace(page))
            {
                spec.Page = int.Parse(page);
            }

            var pageSize = Context.GetFormValue("pagesize");
            if (!string.IsNullOrWhiteSpace(pageSize))
            {
                spec.PageSize = int.Parse(pageSize);
            }

            return spec;
        }

        private static TypeMapping GetTypeMapping()
        {
            var type = Type.GetType(Context.GetFormValue("entityType"));
            var typeMatch = Configuration.TypeMappings[type];
            return typeMatch;
        }

        //private string UpdateByQuery()
        //{
        //    var type = Type.GetType(HttpContext.Current.Request.Form["entityType"]);
        //    var setup = HttpContext.Current.Request.Form["setup"];
        //    var predicate = HttpContext.Current.Request.Form["predicate"];
        //    var modifier = HttpContext.Current.Request.Form["modifier"];
        //    var typeMatch = Configuration.TypeMappings[type];

        //    Context.SetContentType("text/html";
        //    return typeMatch.UpdateByQuery(predicate, setup, modifier).ToString();
        //}

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        private static string GetResource(string filename)
        {
            using (var stream = typeof(SisoDbManagementHandler).Assembly.GetManifestResourceStream("SisoDb.Management." 
                + (filename.EndsWith(".js") ? "Scripts" : "Templates") +  "." + filename))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
            
        }


    }
}
