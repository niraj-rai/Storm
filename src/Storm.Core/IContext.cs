using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Storm.Core
{
    public interface IContext
    {
        #region Properties
        IDbConnection Connection { get; } 
        #endregion

        #region Execute Methods
        DataSet ExecuteDatSet(ISqlQuery sqlQuery);

        DataTable ExecuteTable(ISqlQuery sqlQuery);

        DataTable ExecuteTableFunction(ISqlQuery sqlQuery);

        object ExecuteScalar(ISqlQuery sqlQuery);

        long ExecuteNonQuery(ISqlQuery sqlQuery);
        #endregion

        #region Select Methods
        DataTable Select(string columns, string from, string where = null, string groupBy = null, string having =null, string orderBy = null, int pageIndex = 1, int pageSize = 40);

        IEnumerable<T> Select<T>(string columns, string from, string where = null, string groupBy = null, string having = null, string orderBy = null, int pageIndex = 1, int pageSize = 40) where T : class;

        IEnumerable<T> Select<T>() where T : class;

        IEnumerable<T> Select<T>(Expression<Func<T, object>> columns, Expression<Func<T, bool>> where = null) where T : class;

        IEnumerable<T> Select<T>(Expression<Func<T, object>> columns, Expression<Func<T, bool>> where=null, Expression<Func<T, object>> groupBy=null, Expression<Func<T, object>> having= null, Expression<Func<T, object>> orderBy = null, int pageIndex=1, int pageSize=40) where T : class;

        T Select<T>(Expression<Func<T, object>> columns, object Id) where T : class;

        void SelectInto<TSource, TTarget>(Expression<Func<TSource, object>> columns) where TSource : class where TTarget : class;
        #endregion

        #region Insert Methods
        T Insert<T>(T obj);

        T Insert<T>(Expression<Func<T, object>> columns, T obj);

        T Insert<T>(params (string column, object value)[] columnsWithValue);
        #endregion

        #region Update Methods
        T Update<T>(T obj, object Id);

        T Update<T>(Expression<Func<T, object>> columns, object Id);

        void Update<T>(T obj, Expression<Func<T, bool>> where);

        void Update<T>(Expression<Func<T, object>> columns, Expression<Func<T, bool>> where);
        #endregion

        #region Delete Methods
        void Delete<T>(object Id);
        void Delete<T>();
        void Truncate<T>();
        #endregion
    }
}
