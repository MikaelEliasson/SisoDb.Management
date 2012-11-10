using System.Collections.Generic;

namespace SisoDb.Management
{
    public class QuerySpec
    {

        public string Predicate;
        public List<string> Setup = new List<string>();
        public string OrderBy;
        public SortOrder SortOrder = SortOrder.Asc;
        public int Page = 0;
        public int? PageSize;
    }

    public enum SortOrder
    {
        Asc,
        Desc
    }
}
