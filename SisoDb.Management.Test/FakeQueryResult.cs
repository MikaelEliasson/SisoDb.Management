using System.Collections.Generic;
using SisoDb.Management.Test.Entities;

namespace SisoDb.Management.Test
{
    public class FakeQueryResult
    {
        public IEnumerable<Person> Entities { get; set; }
        public int TotalMatches { get; set; }
    }
}
