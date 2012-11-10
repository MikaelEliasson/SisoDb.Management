using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;

namespace SisoDb.Management.Test
{
    [TestClass]
    public class AuthorizeTest : TestBase
    {
        [TestMethod]
        public void NoAuthorizeSet_Returns500()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/page");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(500, adapter.StatusCode);

            Assert.IsTrue(!adapter.Response.Contains("<html"));
        }

        [TestMethod]
        public void AuthorizeReturnsFalse_Returns401()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/page");
            Configuration.Authorize = str => false;
            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            Assert.AreEqual(401, adapter.StatusCode);

            Assert.IsTrue(!adapter.Response.Contains("<html"));
        }
    }
}
