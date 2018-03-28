using System;

namespace Storm.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyAttribute : Attribute
    {
        public bool Explicit { get; set; }
    }
}