using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;

namespace SisoDb.Management.Test
{
    [TestClass]
    public class DeleteTest : TestBase
    {
        [TestMethod]
        public void DeleteSingle()
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
            .WithPath("/delete")
            .WithEntityType<IPerson>()
            .WithId(persons[1].Id);

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(200, adapter.StatusCode);

            var updatedPersons = Configuration.DB.UseOnceTo().Query<IPerson>().ToListOf<Person>();

            Assert.AreEqual(2, updatedPersons.Count);
            Assert.IsTrue(!updatedPersons.Any(p => p.FirstName == "P2"));
        }
    }
}
