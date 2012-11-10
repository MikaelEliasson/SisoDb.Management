using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;

namespace SisoDb.Management.Test
{
    [TestClass]
    public class DeleteByQueryTest : TestBase
    {
        [TestMethod]
        public void DeleteMatchingCriteria()
        {
            var handler = new SisoDbManagementHandler();
            var persons = new List<Person>
            {
                new Person{FirstName = "P1", Birthdate = DateTime.Parse("2012-01-02")},
                new Person{FirstName = "P2", Birthdate = DateTime.Parse("2011-01-02")},
                new Person{FirstName = "P3", Birthdate = DateTime.Parse("2010-01-02")},
            };
            SetConfig(session => session.InsertMany<IPerson>(persons));

            var adapter = new FakeContextAdapter()
            .WithPath("/deletebyquery")
            .WithEntityType<IPerson>()
            .WithPredicate("p => p.Birthdate < DateTime.Parse(\"2012-01-01\")");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual("2", adapter.Response);
            Assert.AreEqual(200, adapter.StatusCode);

            var updatedPersons = Configuration.DB.UseOnceTo().Query<IPerson>().ToListOf<Person>();

            Assert.AreEqual(1, updatedPersons.Count);
            Assert.IsTrue(updatedPersons[0].FirstName == "P1");
        }
    }
}
