using System;

namespace Storm.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}