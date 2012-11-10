using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;

namespace SisoDb.Management.Test
{
    [TestClass]
    public class InsertTest : TestBase
    {
        [TestMethod]
        public void InsertSingle()
        {
            var json = "{ \"FirstName\":\"Mikael\", \"LastName\":\"Eliasson\",\"BirthDate\":\"1987-03-01\" }";

            var handler = new SisoDbManagementHandler();
            SetConfig();

            var adapter = new FakeContextAdapter()
            .WithPath("/insert")
            .WithEntityType<IPerson>()
            .WithJson(json);

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual("{ \"Success\" : true}", adapter.Response);
            Assert.AreEqual(200, adapter.StatusCode);

            var persons = Configuration.DB.UseOnceTo().Query<IPerson>().ToListOf<Person>();

            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual("Mikael", persons[0].FirstName);
            Assert.AreEqual("Eliasson", persons[0].LastName);

        }

        [TestMethod]
        public void InsertMany()
        {
            var json = "[{ \"FirstName\":\"Mikael1\", \"LastName\":\"Eliasson\",\"BirthDate\":\"1987-03-01\" },"
                + "{ \"FirstName\":\"Mikael2\", \"LastName\":\"Eliasson\",\"BirthDate\":\"1987-03-01\" }]";

            var handler = new SisoDbManagementHandler();
            SetConfig();

            var adapter = new FakeContextAdapter()
            .WithPath("/insert")
            .WithEntityType<IPerson>()
            .WithJson(json);

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual("{ \"Success\" : true}", adapter.Response);
            Assert.AreEqual(200, adapter.StatusCode);

            var persons = Configuration.DB.UseOnceTo().Query<IPerson>().ToListOf<Person>();

            Assert.AreEqual(2, persons.Count);
            Assert.AreEqual("Mikael1", persons[0].FirstName);
            Assert.AreEqual("Mikael2", persons[1].FirstName);

        }
    }
}
