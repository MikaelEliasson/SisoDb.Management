using System.Collections.Generic;

namespace SisoDb.Management
{
    public class QueryResult
    {
        public IEnumerable<string> Entities { get; set; }
        public int TotalMatches { get; set; }
    }
}
