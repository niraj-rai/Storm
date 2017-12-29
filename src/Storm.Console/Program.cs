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
            #region Select Tests
            // Simple select tests
            var simpleSelectTest = new Test.SimpleSelect();
            simpleSelectTest.Select_Test_1();
            simpleSelectTest.Select_Test_2();
            simpleSelectTest.Select_Test_3();
            simpleSelectTest.Select_Test_4();
            simpleSelectTest.Select_Test_5();
            #endregion
        }
    }
}
