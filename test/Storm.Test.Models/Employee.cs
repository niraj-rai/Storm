using System;

namespace Storm.Test.Models
{
    /// <summary>
    /// Employee Model
    /// </summary>
    public class Employee
    {
        public long Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DateOrBirth { get; set; }

        public bool IsActive { get; set; }

        public decimal Salary { get; set; }
    }
}
