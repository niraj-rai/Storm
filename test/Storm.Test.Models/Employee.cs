using Storm.Core.Attributes;
using System;

namespace Storm.Test.Models
{
    /// <summary>
    /// Employee Model
    /// </summary>
    public class Employee
    {
        [Key]
        public long EmployeeId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DateOrBirth { get; set; }

        public decimal? Salary { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
    }
}
