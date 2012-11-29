using System;
using SisoDb.Management.Test.Entities;
using SisoDb.Sql2008;

namespace SisoDb.Management.Test
{
    public class TestBase
    {
        protected static void SetConfig(Action<ISession> setup = null)
        {
            var db = "data source=.\\;initial catalog=SisoDb.Management.Tests;integrated security=SSPI;".CreateSql2008Db();

            db.EnsureNewDatabase();


            Configuration.Clear();
            Configuration.AddTypeMapping<IPerson, Person>();
            Configuration.DB = db;

            Configuration.DB.Settings.AllowDynamicSchemaCreation = true;
            Configuration.DB.Settings.AllowDynamicSchemaUpdates = true;

            Configuration.Authorize = str => true;

            if (setup != null)
            {
                using (var session = db.BeginSession())
                {
                    setup(session);
                }
            }
            
            Configuration.Init();


        }

    }
}
