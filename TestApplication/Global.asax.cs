using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using SisoDb.Management;
using SisoDb;
using SisoDb.Sql2008;

namespace TestApplication
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            var db = "data source=.\\;initial catalog=SisoManagemenTest;integrated security=SSPI;".CreateSql2008Db();

            db.EnsureNewDatabase();

            using (var session = db.BeginSession())
            {
                var items = Enumerable.Range(0, 50).Select(i => new FeedbackItem { 
                    Id = Guid.NewGuid(), 
                    Closed = i % 2 == 0, 
                    Created = DateTime.Today.AddDays(i - 50), 
                    Title = "Title" + i, 
                    Text = "Text" + i, 
                    LatestAction = i % 2 == 0 ? new TakenAction{Date = DateTime.Today, Name = "Name"} : null,
                    CloseDate = i % 2 == 1 ? DateTime.Today : (DateTime?)null,
                    VoteCount = i }).ToList();

                session.InsertMany<IFeedbackItem>(items);

                var comments = items.SelectMany(item => Enumerable.Range(0, 10).Select(i => new Comment{
                    Author = "Author" + i,
                    CommentDate = DateTime.Now.AddHours(i - 10),
                    Deleted = i % 2 == 0,
                    FeedbackItemTitle = item.Title,
                    FeedbackItemId = item.Id,
                    Text = "CommentText" + i}))
                    .ToList();

                session.InsertMany<IComment>(comments);

            }
            db.Configure().ForProduction();
            Configuration.DB = db;

            Configuration.AddTypeMapping<IFeedbackItem, FeedbackItem>();
            Configuration.AddTypeMapping<IComment, Comment>();
            Configuration.AddTypeMapping<EntityWithNoSchema, EntityWithNoSchema>();

            //Never use this in production. You need to provide tight secuity for this
            Configuration.Authorize = action => true;

            Configuration.Init();
        }
    }
}