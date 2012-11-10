using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;

namespace SisoDb.Management.Test
{
    [TestClass]
    public class GetEntityTest : TestBase
    {
        [TestMethod]
        public void GetSingle()
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
            .WithPath("/entity")
            .WithEntityType<IPerson>()
            .WithId(persons[1].Id);

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual("text/html", adapter.ContentType);
            Assert.AreEqual(200, adapter.StatusCode);

            Assert.IsTrue(adapter.Response.Contains("P2"));
        }
    }
}
