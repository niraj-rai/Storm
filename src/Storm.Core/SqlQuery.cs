using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Storm.Core
{
    #region SqlQueries
    public abstract class SqlQuery<T> : ISqlQuery
    {
        internal List<SqlClause> Clauses { get; set; }
        public List<IDbDataParameter> Parameters { get; protected set; }

        public SqlQuery()
        {
            Clauses = new List<SqlClause>();
        }
    }

    public class SqlRawQuery : ISqlQuery
    {
        internal List<SqlClause> Clauses { get; set; }
        public List<IDbDataParameter> Parameters { get; protected set; }
        public SqlRawQuery RawSql(string sql)
        {
            Clauses.Add(new SqlRawClause(sql));
            return this;
        }

        public override string ToString()
        {
            var sql = Clauses.FirstOrDefault(c => c is SqlRawClause);
            return sql.ToString();
        }
    }

    public class SqlSelectQuery<T> : SqlQuery<T>
    {
        protected string SelectClauseColumns { get; set; }
        private string _selectClauseTable;
        public SqlSelectQuery<T> Select(Expression<Func<T, object>> columns)
        {
            _selectClauseTable = typeof(T).Name;
            Clauses.Add(new SqlSelectClause<T>(columns, _selectClauseTable));
            return this;
        }

        public SqlSelectQuery<T> Select(string columns)
        {
            _selectClauseTable = typeof(T).Name;
            return Select(columns, _selectClauseTable);
        }

        public SqlSelectQuery<T> Select(string columns, string from)
        {
            //_selectClauseTable = from;
            if (string.IsNullOrWhiteSpace(columns)) columns = "*";
            var exp = Expression.Constant(columns);
            Clauses.Add(new SqlSelectClause<T>(exp, from));
            return this;
        }

        public SqlSelectQuery<T> Where(string predicate)
        {
            if (predicate == null) return this;
            var exp = Expression.Constant(predicate);
            var clause = new SqlWhereClause(exp as Expression);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) return this;
            var clause = new SqlWhereClause(predicate);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> GroupBy(string columns)
        {
            if (columns == null) return this;
            var exp = Expression.Constant(columns);
            var clause = new SqlGroupByClause(exp);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> GroupBy(Expression<Func<T, object>> columns)
        {
            if (columns == null) return this;
            var clause = new SqlGroupByClause(columns);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> Having(Expression<Func<T, object>> predicate)
        {
            if (predicate == null) return this;
            var clause = new SqlHavingClause(predicate);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> Having(string predicate)
        {
            if (predicate == null) return this;
            var exp = Expression.Constant(predicate);
            var clause = new SqlHavingClause(exp);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> OrderBy(Expression<Func<T, object>> columns)
        {
            if (columns == null) return this;
            var clause = new SqlOrderByClause(columns);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> OrderBy(string columns)
        {
            if (columns == null) return this;
            var exp = Expression.Constant(columns);
            var clause = new SqlOrderByClause(exp);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> Skip(int rows)
        {
            var clause = new SqlRowOffsetClause(rows);
            Clauses.Add(clause);
            return this;
        }

        public SqlSelectQuery<T> Take(int rows)
        {
            var clause = new SqlRowFetchNextClause(rows);
            Clauses.Add(clause);
            return this;
        }

        public override string ToString()
        {
            var strSql = new StringBuilder();

            var select = Clauses.FirstOrDefault(c => c is SqlSelectClause<T>);
            var where = Clauses.FirstOrDefault(c => c is SqlWhereClause);
            var groupBy = Clauses.FirstOrDefault(c => c is SqlGroupByClause);
            var having = Clauses.FirstOrDefault(c => c is SqlHavingClause);
            var orderBy = Clauses.FirstOrDefault(c => c is SqlOrderByClause);
            var offset = Clauses.FirstOrDefault(c => c is SqlRowOffsetClause);
            var fetch = Clauses.FirstOrDefault(c => c is SqlRowFetchNextClause);

            if (select != null)
                strSql.Append($"{select}");
            if (where != null)
                strSql.Append($" {where}");

            if (groupBy != null)
            {
                strSql.Append($" {groupBy}");
                if (having != null)
                    strSql.Append($" {having}");
            }

            if (orderBy == null)
                orderBy = new SqlOrderByClause(Expression.Constant(1));

            strSql.Append($" {orderBy}");

            if (offset != null)
            {
                strSql.Append($" {offset}");
                if (fetch != null)
                    strSql.Append($" {fetch}");
            }

            return strSql.ToString();
        }
    }

    public class SqlInsertQuery<T> : SqlQuery<T>
    {
        private List<(string column, object value)> InsertColumnsWithValue { get; set; }
        private string _insertClauseTable;

        public SqlInsertQuery<T> Insert(T obj)
        {
            _insertClauseTable = typeof(T).Name;
            var clause = new SqlInsertClause<T>(obj as Expression);
            Clauses.Add(clause);
            return this;
        }

        public SqlInsertQuery<T> Insert(params (string column, object value)[] columnsWithValue)
        {
            _insertClauseTable = typeof(T).Name;
            InsertColumnsWithValue = columnsWithValue.ToList();
            var insertCluase = $"({ string.Join(", ", InsertColumnsWithValue.Select(c => $"[{c.column}]"))}) VALUES({ string.Join(", ", InsertColumnsWithValue.Select(c => $"'{c.value}'"))})";
            var exp = Expression.Constant(insertCluase);
            var clause = new SqlInsertClause<T>(exp as Expression);
            Clauses.Add(clause);
            return this;
        }

        public override string ToString()
        {
            var insert = Clauses.FirstOrDefault(c => c is SqlInsertClause<T>);
            return insert.ToString();
        }
    }

    public class SqlUpdateQuery<T> : SqlQuery<T>
    {
        private List<(string column, object value)> _updateColumnsWithValue { get; set; }
        private List<(string column, object value)> _whereColumnsWithValue { get; set; }

        private string _updateClauseTable;
        private string _updateWhereClause;

        public SqlUpdateQuery<T> Update(T obj)
        {
            _updateClauseTable = typeof(T).Name;
            var clause = new SqlInsertClause<T>(obj as Expression);
            Clauses.Add(clause);
            return this;
        }

        public SqlUpdateQuery<T> Update(params (string column, object value)[] columnsWithValue)
        {
            _updateClauseTable = typeof(T).Name;
            _updateColumnsWithValue = columnsWithValue.ToList();
            return this;
        }

        public SqlUpdateQuery<T> Update(object Id, params (string column, object value)[] columnsWithValue)
        {
            _updateClauseTable = typeof(T).Name;
            _updateColumnsWithValue = columnsWithValue.ToList();
            return this;
        }

        public SqlUpdateQuery<T> Where(Expression<Func<T, bool>> whereClause)
        {
            _updateClauseTable = typeof(T).Name;
            _updateWhereClause = whereClause.Evaluate()?.ToString();
            return this;
        }

        public SqlUpdateQuery<T> Where(params (string column, object value)[] whereColumnsWithValue)
        {
            _updateClauseTable = typeof(T).Name;
            _whereColumnsWithValue = whereColumnsWithValue.ToList();
            _updateWhereClause = string.Join("AND ", _whereColumnsWithValue.Select(c => $"[{c.column}] = '{c.value}'"));
            return this;
        }

        public SqlUpdateQuery<T> Where(string whereCluase)
        {
            _updateClauseTable = typeof(T).Name;
            _updateWhereClause = whereCluase;
            return this;
        }

        public override string ToString()
        {
            string updateQuery = $"UPDATE [{_updateClauseTable}] SET {string.Join(", ", _updateColumnsWithValue.Select(c => $"[{ c.column}] = '{c.value}'"))}";
            if (!string.IsNullOrWhiteSpace(_updateWhereClause))
                updateQuery = $"{updateQuery} WHERE {_updateWhereClause}";
            return updateQuery;
        }
    }
    #endregion

    #region SqlClauses
    internal abstract class SqlClause
    {
        protected abstract string ClauseToken { get; }
    }

    internal class SqlRawClause : SqlClause
    {
        private string _sql;

        protected override string ClauseToken => "";

        public SqlRawClause(string sql)
        {
            _sql = sql;
        }

        public override string ToString()
        {
            return _sql;
        }
    }

    internal class SqlSelectClause<T> : SqlClause
    {
        private List<string> _selectColumns;
        private string _selectClause;
        private string _tableName;
        protected override string ClauseToken => "SELECT";
        public SqlSelectClause(Expression exp, string from = null)
        {
            _tableName = from ?? typeof(T).GetType().Name;
            _selectClause = exp.Evaluate()?.ToString();
            _selectColumns = _selectClause?.Split(',')?.Select(c => c.Trim()).ToList();
        }

        public override string ToString()
        {
            return $"{ClauseToken} {_selectClause} FROM [{_tableName}]";
        }
    }

    internal class SqlInsertClause<T> : SqlClause
    {
        private List<(string column, object value)> _insertColumnsWithValue;
        private string _insertClause;
        private string _tableName;
        protected override string ClauseToken => "INSERT INTO [{0}]";

        public SqlInsertClause(Expression exp)
        {
            _tableName = typeof(T).GetType().Name;
            _insertClause = exp.Evaluate()?.ToString();
            //_insertColumnsWithValue = _insertClause?.Split(',')?.Select(c => c.Trim()).ToList();
        }

        public override string ToString()
        {
            return $"{string.Format(ClauseToken, _tableName)} {_insertClause}";
        }
    }

    internal class SqlUpdateClause<T> : SqlClause
    {
        private List<(string column, object value)> _insertColumnsWithValue;
        private string _updateClause;
        private string _tableName;
        protected override string ClauseToken => "UPDATE [{0}] SET";

        public SqlUpdateClause(Expression exp)
        {
            _tableName = typeof(T).GetType().Name;
            _updateClause = exp.Evaluate()?.ToString();
            //_insertColumnsWithValue = _insertClause?.Split(',')?.Select(c => c.Trim()).ToList();
        }

        public override string ToString()
        {
            return $"{string.Format(ClauseToken, _tableName)} {_updateClause}";
        }
    }

    internal class SqlWhereClause : SqlClause
    {
        private string _whereClause;
        protected override string ClauseToken => "WHERE";

        public SqlWhereClause(Expression exp)
        {
            _whereClause = exp.Evaluate()?.ToString();
            //_whereClause = _selectClause?.Split(',')?.Select(c=> c.Trim()).ToList();
        }

        public override string ToString()
        {
            return $"{ClauseToken} {_whereClause}";
        }
    }

    internal class SqlGroupByClause : SqlClause
    {
        private string _groupByClause;
        protected override string ClauseToken => "GROUP BY";

        public SqlGroupByClause(Expression exp)
        {
            _groupByClause = exp.Evaluate()?.ToString();
        }

        public override string ToString()
        {
            return $"{ClauseToken} {_groupByClause}";
        }
    }

    internal class SqlHavingClause : SqlClause
    {
        private string _havingClause;
        protected override string ClauseToken => "HAVING";

        public SqlHavingClause(Expression exp)
        {
            _havingClause = exp.Evaluate()?.ToString();
        }

        public override string ToString()
        {
            return $"{ClauseToken} {_havingClause}";
        }
    }

    internal class SqlOrderByClause : SqlClause
    {
        private string _orderbyClause;
        protected override string ClauseToken => "ORDER BY";

        public SqlOrderByClause(Expression exp)
        {
            _orderbyClause = exp.Evaluate()?.ToString();
        }

        public override string ToString()
        {
            return $"{ClauseToken} {_orderbyClause}";
        }
    }

    internal class SqlRowOffsetClause : SqlClause
    {
        private int _offsetRows;
        protected override string ClauseToken => "OFFSET {0} ROWS";

        public SqlRowOffsetClause(int rows)
        {
            _offsetRows = rows > 0 ? rows : 0;
        }

        public override string ToString()
        {
            return string.Format(ClauseToken, _offsetRows);
        }
    }

    internal class SqlRowFetchNextClause : SqlClause
    {
        private int _nextRows;
        protected override string ClauseToken => "FETCH NEXT {0} ROWS ONLY";

        public SqlRowFetchNextClause(int rows)
        {
            _nextRows = rows > 0 ? rows : 1;
        }

        public override string ToString()
        {
            return string.Format(ClauseToken, _nextRows);
        }
    }
    #endregion
}
