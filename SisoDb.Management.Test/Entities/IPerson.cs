using System;
namespace SisoDb.Management.Test.Entities
{
    public interface IPerson
    {
        DateTime Birthdate { get; set; }
        string FirstName { get; set; }
        int Id { get; set; }
        string LastName { get; set; }
    }
}
