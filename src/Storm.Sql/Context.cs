using Storm.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Collections;

namespace Storm.Sql
{
    public class Context : IContext
    {
        #region Properties
        public IDbConnection Connection { get; protected set; } 
        #endregion

        #region Constructors
        public Context(string conString)
        {
            // ToDo: Create connection via conneciton factory for underlying data provider.
        }

        public Context(IDbConnection con)
        {
            Connection = con;
        } 
        #endregion

        #region Execute Methods
        public DataTable ExecuteTable(ISqlQuery sqlQuery)
        {
            var strSql = sqlQuery.ToString();
            return null;
        }

        public DataTable ExecuteTableFunction(ISqlQuery sqlQuery)
        {
            var strSql = sqlQuery.ToString();
            return null;
        }

        public DataSet ExecuteDatSet(ISqlQuery sqlQuery)
        {
            var strSql = sqlQuery.ToString();
            return null;
        }

        public long ExecuteNonQuery(ISqlQuery sqlQuery)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(ISqlQuery sqlQuery)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Select Methods
        public IEnumerable<T> Select<T>() where T : class
        {
            var sql = new SqlSelectQuery<T>();
            sql = sql.Select((o) => o);
            var strSql = sql.ToString();
            return null;
        }

        public T Select<T>(Expression<Func<T, object>> columns, object Id) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, object>> columns, Expression<Func<T, bool>> where = null) where T : class
        {
            var sql = new SqlSelectQuery<T>();
            sql = sql.Select(columns)
                .Where(where);
            var strSql = (sql as ISqlQuery).ToString();
            return null;
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, object>> columns, Expression<Func<T, bool>> where = null, Expression<Func<T, object>> groupBy = null, Expression<Func<T, object>> having = null, Expression<Func<T, object>> orderBy = null, int pageIndex = 1, int pageSize = 40) where T : class
        {
            int rowsOffet = (pageIndex - 1) * pageSize;
            var sql = new SqlSelectQuery<T>();
            sql = sql.Select(columns)
                .Where(where)
                .GroupBy(groupBy)
                .Having(having)
                .OrderBy(orderBy)
                .Skip(rowsOffet)
                .Take(pageSize);
            var strSql = (sql as ISqlQuery).ToString();
            return null;
        }

        public DataTable Select(string columns, string from, string where = null, string groupBy = null, string having = null, string orderBy = null, int pageIndex = 1, int pageSize = 40)
        {
            int rowsOffet = (pageIndex - 1) * pageSize;
            var sql = new SqlSelectQuery<string>();
            sql = sql.Select(columns, from)
                .Where(where)
                .GroupBy(groupBy)
                .Having(having)
                .OrderBy(orderBy)
                .Skip(rowsOffet)
                .Take(pageSize);
            var strSql = (sql as ISqlQuery).ToString();
            return null;
        }

        public IEnumerable<T> Select<T>(string columns, string from, string where = null, string groupBy = null, string having = null, string orderBy = null, int pageIndex = 1, int pageSize = 40) where T : class
        {
            int rowsOffet = (pageIndex - 1) * pageSize;
            var sql = new SqlSelectQuery<string>();
            sql = sql.Select(columns, from)
                .Where(where)
                .GroupBy(groupBy)
                .Having(having)
                .OrderBy(orderBy)
                .Skip(rowsOffet)
                .Take(pageSize);
            var strSql = (sql as ISqlQuery).ToString();
            return null;
        }

        public void SelectInto<TSource, TTarget>(Expression<Func<TSource, object>> columns) where TSource : class where TTarget : class
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Insert Methods
        public T Insert<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public T Insert<T>(Expression<Func<T, object>> columns, T obj)
        {
            throw new NotImplementedException();
        }

        public T Insert<T>(params (string column, object value)[] columnsWithValue)
        {
            string table = typeof(T).Name;
            var sql = new SqlInsertQuery<T>();
            sql = sql.Insert(columnsWithValue);
            var strSql = sql.ToString();
            T retVal = Activator.CreateInstance<T>();
            return retVal;
        } 
        #endregion

        #region Update Methods
        public T Update<T>(T obj, object Id)
        {
            string table = typeof(T).Name;
            var sql = new SqlUpdateQuery<T>();
            sql = sql.Update(("col1", "newVal1"))
                .Where(("col1", "val1"));
            var strSql = sql.ToString();

            var sql1 = new SqlUpdateQuery<Core.ColumnDefinition>();
            sql1 = sql1.Update(("col1", "newVal1"))
                    .Where((o) => o.Name == "Niraj" || (o.IsKey == false && o.Name == "Sanjeev"));
            var strSql1 = sql1.ToString();

            T retVal = Activator.CreateInstance<T>();
            return retVal;
        }

        public T Update<T>(Expression<Func<T, object>> columns, object Id)
        {
            throw new NotImplementedException();
        }

        public void Update<T>(T obj, Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public void Update<T>(Expression<Func<T, object>> columns, Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        } 
        #endregion

        #region Delete Methods

        public void Delete<T>()
        {
            throw new NotImplementedException();
        }

        public void Delete<T>(object Id)
        {
            throw new NotImplementedException();
        }

        public void Truncate<T>()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
