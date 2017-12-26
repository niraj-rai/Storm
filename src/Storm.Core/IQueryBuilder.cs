using System;
using System.Linq;

namespace Storm.Core
{
    public interface IQueryBuilder
    {
        string BuildSelect(string table, string schema = "dbo", params SelectColumnDefinition[] columns);

        string BuildInsert(string table, string schema = "dbo", params InsertColumnDefinition[] columnsWithValue);

        string BuildUpdate(string table, string schema = "dbo", params InsertColumnDefinition[] columnsWithValue);

        string BuildDelete(string table, string schema = "dbo", params InsertColumnDefinition[] columnsWithValue);
    }

    public class QueryBuilder : IQueryBuilder
    {
        public string BuildDelete(string table, string schema = "dbo", params InsertColumnDefinition[] columnsWithValue)
        {
            throw new NotImplementedException();
        }

        public string BuildInsert(string table, string schema = "dbo", params InsertColumnDefinition[] columnsWithValue)
        {
            throw new System.NotImplementedException();
        }

        public string BuildSelect(string table, string schema = "dbo", params SelectColumnDefinition[] columns)
        {
            if (string.IsNullOrWhiteSpace(table)) throw new ArgumentNullException(nameof(table));
            string strColumns = "*";
            if (columns != null && columns.Any())
                strColumns = string.Join(",", columns.Select(c => $"[{c.Name}]"));

            return $"select {strColumns} from [{schema}].[{table}]";
        }

        public string BuildUpdate(string table, string schema = "dbo", params InsertColumnDefinition[] columnsWithValue)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ColumnDefinition
    {
        public string Name { get; set; }

        public bool IsKey { get; set; }
    }

    public class SelectColumnDefinition : ColumnDefinition
    {
        public string Alias { get; set; }

        public string Expression { get; }
    }

    public class InsertColumnDefinition : ColumnDefinition
    {
        public string Value { get; set; }
    }

    public class UpdateColumnDefinition : InsertColumnDefinition
    {
        public string NewValue { get; set; }
    }

    public class Clause
    {
        public SelectColumnDefinition Column { get; set; }

        public string Operator { get; set; }

        public string Value { get; set; }

    }

    public class WhereClause : Clause
    {
        public string ClauseToken { get; set; }
    }

}
