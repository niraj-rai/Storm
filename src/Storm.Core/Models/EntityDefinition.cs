using System.Collections.Generic;
using System.Data;

namespace Storm.Core.Models
{
    public class EntityDefinition
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<EntityAttribute> Attributes { get; set; }
    }

    public class EntityAttribute
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public SqlDbType DataType { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsAutoGenerated { get; set; }
    }
}