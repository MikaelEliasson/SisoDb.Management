using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SisoDb.Management.Test
{
    [TestClass]
    public class InsertSchemaTest : TestBase
    {
        [TestMethod]
        public void WithNoSchema_InsertsSchema()
        {
            var handler = new SisoDbManagementHandler();
            SetConfig();

            var adapter = new FakeContextAdapter()
            .WithPath("/insertschema")
            .WithEntityType<IPerson>();

            SisoDbManagementHandler.Context = adapter;

            Configuration.DB.Configure().ForProduction();
            handler.ProcessRequest(null);

            Assert.AreEqual(200, adapter.StatusCode);

            //Query persons to trigger exception if the schema wasn't already inserted
            var updatedPersons = Configuration.DB.UseOnceTo().Query<IPerson>().ToListOf<Person>();
        }

        [TestMethod]
        public void WithSchema_DoesNotDeleteData()
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
            .WithPath("/insertschema")
            .WithEntityType<IPerson>();

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(200, adapter.StatusCode);

            var updatedPersons = Configuration.DB.UseOnceTo().Query<IPerson>().ToListOf<Person>();

            Assert.AreEqual(3, updatedPersons.Count);
        }

        [TestMethod]
        public void DoesNotAlterEnabledUpsert()
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
            .WithPath("/insertschema")
            .WithEntityType<IPerson>();

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(200, adapter.StatusCode);

            Assert.IsTrue(Configuration.DB.Settings.AllowDynamicSchemaCreation);
            Assert.IsTrue(Configuration.DB.Settings.AllowDynamicSchemaUpdates);
        }

        [TestMethod]
        public void DoesNotAlterDisabledUpsert()
        {
            var handler = new SisoDbManagementHandler();
            var persons = new List<Person>
            {
                new Person{FirstName = "P1"},
                new Person{FirstName = "P2"},
                new Person{FirstName = "P3"}
            };
            SetConfig(session => session.InsertMany<IPerson>(persons));
            Configuration.DB.Configure().ForProduction();

            var adapter = new FakeContextAdapter()
            .WithPath("/insertschema")
            .WithEntityType<IPerson>();

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(200, adapter.StatusCode);

            Assert.IsFalse(Configuration.DB.Settings.AllowDynamicSchemaCreation);
            Assert.IsFalse(Configuration.DB.Settings.AllowDynamicSchemaUpdates);
        }
    }
}
