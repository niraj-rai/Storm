using System;
using System.Collections.Generic;
using System.Linq;
using Storm.Core;
using Storm.Sql;
using Storm.Test.Models;

namespace Storm.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Storm Console");

            //var context = new Context("");
            //var result1 = context.Select("selectColumnDefinition");
            //var nameValue = "A";
            //var result2 = context.Select<Core.SelectColumnDefinition>() as IQueryable<Core.SelectColumnDefinition>;
            //var result3 = context.Select<Core.SelectColumnDefinition>((o)=> new {a = o.Alias, o.Name, o.IsKey, value = 10}, (o)=> o.Name == nameValue && o.IsKey == true);

            //context.Insert<Core.SelectColumnDefinition>
            //    (
            //        ("col1", "val1"),
            //        ("col2", "val2"),
            //        ("col3", "val4"),
            //        ("col4", "val4"),
            //        ("col5", "val5"),
            //        ("col6", "val6")
            //    );

            //context.Update<Core.SelectColumnDefinition>(new Core.SelectColumnDefinition(), 1);
            //Test.Select_Test_1();
            Test.Select_Test_2();
            Test.Select_Test_3();
            Test.Select_Test_4();
            Test.Select_Test_5();
        }
    }

    public static class Test
    {
        static readonly IContext _context;

        static Test()
        {
                _context = new Context("");
        }
        public static void Select_Test_1()
        {
            var result = _context.Select("*", from:"selectColumnDefinition");
        }

        public static void Select_Test_2()
        {
            var result = _context.Select<Employee>();
        }

        public static void Select_Test_3()
        {
            var result = _context.Select<Employee>((o)=> new {o.FirstName, o.LastName, BirthDate = o.DateOrBirth, o.Salary, Value = 10}, (o)=> o.FirstName == "Niraj" && o.IsActive == true);
        }

        public static void Select_Test_4()
        {
            var result = _context.Select<Employee>(
                columns:(o) => new { o.FirstName, o.LastName, BirthDate = o.DateOrBirth, o.Salary, Value = 10 },
                where: (o) => o.FirstName == "Niraj" && o.IsActive == true, 
                orderBy: (o)=> o.Salary, 
                pageIndex:2, 
                pageSize:20);
        }

        public static void Select_Test_5()
        {
            var result = _context.Select("firstName, lastName", "Employee", "firtName like '%Niraj'");
        }
    }
}
