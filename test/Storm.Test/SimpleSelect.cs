using System;
using Storm.Core;
using Storm.Sql;
using Storm.Test.Models;
using Xunit;

namespace Storm.Test
{
    public class SimpleSelect
    {
        static readonly IContext _context;

        [Fact]
        public void Test1()
        {
            Assert.Equal(2,1+1);
        }

        static SimpleSelect()
        {
            _context = new Context("");
        }

        [Fact]
        public void Select_Test_1()
        {
            var result = _context.Select("*", from: "employee");
        }

        [Fact]
        public void Select_Test_2()
        {
            var result = _context.Select<Employee>();
        }

        [Fact]
        public void Select_Test_3()
        {
            var result = _context.Select<Employee>((o) => new { o.FirstName, o.LastName, BirthDate = o.DateOrBirth, o.Salary, Value = 10 }, (o) => o.FirstName == "Niraj" && o.IsActive == true);
        }

        [Fact]
        public void Select_Test_4()
        {
            var result = _context.Select<Employee>(
                columns: (o) => new { o.FirstName, o.LastName, BirthDate = o.DateOrBirth, o.Salary, Value = 10 },
                where: (o) => o.FirstName == "Niraj" && o.IsActive == true,
                orderBy: (o) => new { o.Salary },
                pageIndex: 2,
                pageSize: 20);
        }

        [Fact]
        public void Select_Test_5()
        {
            var result = _context.Select(columns:"firstName, lastName", from:"Employee", where:"firstName like '%Niraj'");
        }

        [Fact]
        public void Select_Test_6()
        {
            var result = _context.Select<Employee>(columns: (o) => new { o.FirstName, o.LastName, BirthDate = o.DateOrBirth, o.Salary, Value = 10 }, id: 10); 
        }
    }
}