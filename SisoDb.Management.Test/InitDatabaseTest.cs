using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SisoDb.Sql2008;

namespace SisoDb.Management.Test
{
    [TestClass]
    public class InitDatabaseTest : TestBase
    {
        [TestMethod]
        public void WithoutStructures()
        {
            var handler = new SisoDbManagementHandler();

            Setup();

            var adapter = new FakeContextAdapter()
            .WithPath("/initdb")
            .WithEntityType<IPerson>();

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(200, adapter.StatusCode);

            var db = "data source=.\\;initial catalog=SisoDb.Management.Tests;integrated security=SSPI;".CreateSql2008Db();
            Assert.IsTrue(db.Exists());
            
        }

        [TestMethod]
        public void WithStructures()
        {
            var handler = new SisoDbManagementHandler();

            Setup();

            var adapter = new FakeContextAdapter()
            .WithPath("/initdbandstructures")
            .WithEntityType<IPerson>();

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(200, adapter.StatusCode);

            var db = "data source=.\\;initial catalog=SisoDb.Management.Tests;integrated security=SSPI;".CreateSql2008Db();
            db.Configure().ForProduction();
            Assert.IsTrue(db.Exists());
            //Force exception if tables aren't already created
            var persons = db.UseOnceTo().Query<IPerson>().ToList();

        }

        [TestMethod]
        public void WithExistingDb_ShouldNotDestoryDb()
        {
            var handler = new SisoDbManagementHandler();

            var persons = new List<Person>
            {
                new Person{FirstName = "P1"},
                new Person{FirstName = "P2"},
                new Person{FirstName = "P3"}
            };
            SetConfig(session => session.InsertMany<IPerson>(persons));

            var adapter = new FakeContextAdapter()
            .WithPath("/initdb")
            .WithEntityType<IPerson>();

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(200, adapter.StatusCode);

            var updatedPersons = Configuration.DB.UseOnceTo().Query<IPerson>().ToListOf<Person>();

            Assert.AreEqual(3, updatedPersons.Count);

        }

        private static void Setup()
        {
            var db = "data source=.\\;initial catalog=SisoDb.Management.Tests;integrated security=SSPI;".CreateSql2008Db();

            db.DeleteIfExists();

            var db2 = "data source=.\\;initial catalog=SisoDb.Management.Tests;integrated security=SSPI;".CreateSql2008Db();


            Configuration.Clear();
            Configuration.AddTypeMapping<IPerson, Person>();
            Configuration.DB = db2;

            Configuration.DB.Settings.AllowUpsertsOfSchemas = true;
            Configuration.DB.Settings.SynchronizeSchemaChanges = true;

            Configuration.Authorize = str => true;

            Configuration.Init();
        }
    }
}
