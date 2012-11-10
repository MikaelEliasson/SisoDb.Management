using System;

namespace SisoDb.Management.Test.Entities
{
    public class Person : SisoDb.Management.Test.Entities.IPerson
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Comment { get; set; }
        public DateTime Birthdate { get; set; }
    }
}
