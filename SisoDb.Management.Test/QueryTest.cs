using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;
using SisoDb.Serialization;

namespace SisoDb.Management.Test
{


    [TestClass]
    public class QueryTest : TestBase
    {
        private static List<Person> persons;
        private static DateTime lastDate;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            persons = Enumerable.Range(0, 500)
                .Select(i => new Person { FirstName = "F" + i, LastName = "L" + i, Birthdate = DateTime.Today.AddDays(i - 500), Comment = "C" + 1 })
                .ToList();
            lastDate = persons.Last().Birthdate;
            persons.Add(new Person { FirstName = "Mikael", LastName = "Eliasson", Birthdate = DateTime.Parse("1987-03-01"), Comment = "C" });

            SetConfig(session =>
            {
                session.InsertMany<IPerson>(persons);
            });
        }

        [TestMethod]
        public void NoPredicate_NoOrderBy_ReturnsAll()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>();

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
            .IsSuccess()
            .HasEntityCount(501)
            .HasTotalMatches(501);
        }

        [TestMethod]
        public void OrderByBirthdate_NoPage_NoSortorder_ReturnsAllOrderedAsc()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithOrderBy("x => x.Birthdate");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(501)
                .HasTotalMatches(501)
                .FirstEntityShould(p => Assert.AreEqual(DateTime.Parse("1987-03-01"), p.Birthdate))
                .LastEntityShould(p => Assert.AreEqual(lastDate, p.Birthdate));
        }

        [TestMethod]
        public void OrderByBirthdate_NoPage_Asc_ReturnsAllOrderedAsc()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithOrderBy("x => x.Birthdate")
            .WithSortOrder("asc");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(501)
                .HasTotalMatches(501)
                .FirstEntityShould(p => Assert.AreEqual(DateTime.Parse("1987-03-01"), p.Birthdate))
                .LastEntityShould(p => Assert.AreEqual(lastDate, p.Birthdate));
        }

        [TestMethod]
        public void OrderByBirthdate_NoPage_Desc_ReturnsAllOrderedDesc()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithOrderBy("x => x.Birthdate")
            .WithSortOrder("desc");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(501)
                .HasTotalMatches(501)
                .FirstEntityShould(p => Assert.AreEqual(lastDate, p.Birthdate))
                .LastEntityShould(p => Assert.AreEqual(DateTime.Parse("1987-03-01"), p.Birthdate));
        }

        [TestMethod]
        public void OrderByBirthdate_PageSize100_NoPage_Asc_Returns100First()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithOrderBy("x => x.Birthdate")
            .WithPageSize(100)
            .WithSortOrder("asc");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(100)
                .HasTotalMatches(501)
                .FirstEntityShould(p => Assert.AreEqual(DateTime.Parse("1987-03-01"), p.Birthdate))
                .LastEntityShould(p => Assert.AreEqual("F98", p.FirstName));
        }

        [TestMethod]
        public void OrderByBirthdate_PageSize100_Page1_Asc_Returns101To200()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithOrderBy("x => x.Birthdate")
            .WithPageSize(100)
            .WithPage(1)
            .WithSortOrder("asc");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(100)
                .HasTotalMatches(501)
                .FirstEntityShould(p => Assert.AreEqual("F99", p.FirstName))
                .LastEntityShould(p => Assert.AreEqual("F198", p.FirstName));
        }

        [TestMethod]
        public void FilterOnDateInline()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithPredicate("p => p.Birthdate == DateTime.Parse(\"1987-03-01\")");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(1)
                .HasTotalMatches(1)
                .FirstEntityShould(p => Assert.AreEqual("Mikael", p.FirstName));
        }

        [TestMethod]
        public void FilterOnDateWithSetup()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithSetup("var date = DateTime.Parse(\"1987-03-01\");")
            .WithPredicate("p => p.Birthdate == date");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(1)
                .HasTotalMatches(1)
                .FirstEntityShould(p => Assert.AreEqual("Mikael", p.FirstName));
        }

        [TestMethod]
        public void FilterUsingASisoMethods()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithPredicate("p => p.FirstName.QxStartsWith(\"Mik\")");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(1)
                .HasTotalMatches(1)
                .FirstEntityShould(p => Assert.AreEqual("Mikael", p.FirstName));
        }

        [TestMethod]
        public void FilterWithBadPredicate_ShouldReturnErrorAndMessageAboutThePredicateBeingBad()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithPredicate("p => p.SomeNameThatDoesNotExist == 2");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter).FailedWithMessage("Mono could not parse the predicate");
        }

        [TestMethod]
        public void SortAndPredicateWorksToghether()
        {
            var handler = new SisoDbManagementHandler();

            var adapter = new FakeContextAdapter()
            .WithPath("/query")
            .WithEntityType<IPerson>()
            .WithOrderBy("x => x.Birthdate")
            .WithPageSize(100)
            .WithPredicate("p => p.FirstName.QxStartsWith(\"Mik\")");

            SisoDbManagementHandler.Context = adapter;

            handler.ProcessRequest(null);

            AssertThat(adapter)
                .IsSuccess()
                .HasEntityCount(1)
                .HasTotalMatches(1)
                .FirstEntityShould(p => Assert.AreEqual("Mikael", p.FirstName));
        }



        private PersonQueryResultEvaluator AssertThat(FakeContextAdapter adapter)
        {
            return new PersonQueryResultEvaluator(adapter);
        }

        private class PersonQueryResultEvaluator
        {
            public FakeQueryResult QueryResult;
            public List<Person> Entities;
            private FakeContextAdapter adapter;

            public PersonQueryResultEvaluator(FakeContextAdapter adapter)
            {
                this.adapter = adapter;
                if (adapter.StatusCode == 200)
                {
                    QueryResult = JsonSerializer.DeserializeFromString<FakeQueryResult>(adapter.Response);
                    Entities = QueryResult.Entities.ToList();
                }
            }

            public PersonQueryResultEvaluator IsSuccess()
            {
                Assert.AreEqual(200, adapter.StatusCode);
                Assert.AreEqual("application/json", adapter.ContentType);
                return this;
            }

            public PersonQueryResultEvaluator FailedWithMessage(string message)
            {
                Assert.AreEqual(500, adapter.StatusCode);
                Assert.AreEqual(message, adapter.Response);
                return this;
            }

            public PersonQueryResultEvaluator HasTotalMatches(int value)
            {
                Assert.AreEqual(value, QueryResult.TotalMatches);
                return this;
            }

            public PersonQueryResultEvaluator HasEntityCount(int value)
            {
                Assert.AreEqual(value, Entities.Count);
                return this;
            }

            public PersonQueryResultEvaluator FirstEntityShould(Action<Person> evaluate)
            {
                evaluate(Entities.First());
                return this;
            }

            public PersonQueryResultEvaluator LastEntityShould(Action<Person> evaluate)
            {
                evaluate(Entities.Last());
                return this;
            }


        }
    }
}
