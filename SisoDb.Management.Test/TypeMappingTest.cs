using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SisoDb.Management.Test.Entities;

namespace SisoDb.Management.Test
{
    [TestClass]
    public class TypeMappingTest
    {
        [TestMethod]
        public void Predicate_ForInterface_CanBuildPredicate()
        {
            var mapping = new TypeMapping<IPerson, Person>();
            var expression = mapping.Predicate("p => p.LastName == \"LName\"").Compile();

            var persons = new List<IPerson>
            {
                new Person{ Id = 1, LastName = "LName"},
                new Person{ Id = 2, LastName = "LName2"},
            };

            var filteredPersons = persons.Where(expression).ToList();

            Assert.AreEqual(1, filteredPersons.Count);
            Assert.AreEqual(1, filteredPersons[0].Id);
        }

        [TestMethod]
        public void Predicate_WithSetup_ForInterface_CanBuildPredicate()
        {
            var mapping = new TypeMapping<IPerson, Person>();
            var usingStatement = "using System;";
            var date = "var date = DateTime.Parse(\"1987-03-01\");";
            var expression = mapping.Predicate("p => p.Birthdate == date", usingStatement, date).Compile();

            var persons = new List<IPerson>
            {
                new Person{ Id = 1, Birthdate = DateTime.Parse("1987-03-01") },
                new Person{ Id = 2, Birthdate = DateTime.Parse("1987-03-02") },
            };

            var filteredPersons = persons.Where(expression).ToList();

            Assert.AreEqual(1, filteredPersons.Count);
            Assert.AreEqual(1, filteredPersons[0].Id);

        }


        [TestMethod]
        public void Predicate_WithMultiStatementSetup_CRLF_ForInterface_CanBuildPredicate()
        {
            var mapping = new TypeMapping<IPerson, Person>();
            var setupStatement = "using System;\r\n var date = DateTime.Parse(\"1987-03-01\");";
            var expression = mapping.Predicate("p => p.Birthdate == date", setupStatement).Compile();

            var persons = new List<IPerson>
            {
                new Person{ Id = 1, Birthdate = DateTime.Parse("1987-03-01") },
                new Person{ Id = 2, Birthdate = DateTime.Parse("1987-03-02") },
            };

            var filteredPersons = persons.Where(expression).ToList();

            Assert.AreEqual(1, filteredPersons.Count);
            Assert.AreEqual(1, filteredPersons[0].Id);
        }

        [TestMethod]
        public void Predicate_WithMultiStatementSetup_LF_ForInterface_CanBuildPredicate()
        {
            var mapping = new TypeMapping<IPerson, Person>();
            var setupStatement = "using System;\n var date = DateTime.Parse(\"1987-03-01\");";
            var expression = mapping.Predicate("p => p.Birthdate == date", setupStatement).Compile();

            var persons = new List<IPerson>
            {
                new Person{ Id = 1, Birthdate = DateTime.Parse("1987-03-01") },
                new Person{ Id = 2, Birthdate = DateTime.Parse("1987-03-02") },
            };

            var filteredPersons = persons.Where(expression).ToList();

            Assert.AreEqual(1, filteredPersons.Count);
            Assert.AreEqual(1, filteredPersons[0].Id);
        }

        [TestMethod]
        public void OrderBy_CanBuildExpression()
        {
            var mapping = new TypeMapping<IPerson, Person>();
            var expression = mapping.OrderBy("p => p.Birthdate").Compile();

            var persons = new List<IPerson>
            {
                new Person{ Id = 1, Birthdate = DateTime.Parse("1987-03-02") },
                new Person{ Id = 2, Birthdate = DateTime.Parse("1987-03-01") },
            };

            var orderedPersons = persons.OrderBy(expression).ToList();

            Assert.AreEqual(2, orderedPersons[0].Id);
        }
    }
}
