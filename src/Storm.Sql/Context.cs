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
using Storm.Core.Models;
using Storm.Core.Attributes;

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

        public T Select<T>(Expression<Func<T, object>> columns, object id) where T : class
        {
            var entityDef = GetEntityDefintion(typeof(T));
            var pkAttributeName = entityDef.Attributes.FirstOrDefault(o => o.IsPrimaryKey == true)?.Name;
            if(pkAttributeName == null)
                pkAttributeName = entityDef.Attributes.First().Name;
                
            var sql = new SqlSelectQuery<T>();
            sql = sql.Select(columns)
                .Where($"{pkAttributeName} == '{id.ToString()}'");
            var strSql = (sql as ISqlQuery).ToString();
            return null;
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
        public T Update<T>(T obj, object id)
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

        public T Update<T>(Expression<Func<T, object>> columns, object id)
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

        public void Delete<T>(object id)
        {
            throw new NotImplementedException();
        }

        public void Truncate<T>()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Helper Methods
        private static EntityDefinition GetEntityDefintion(Type type)
        {
            EntityDefinition entityDef = new EntityDefinition();
            entityDef.Name = GetEntityName(type, out var description);
            entityDef.Description = description;

            var entityAttrs = new List<EntityAttribute>();
            var propInfos =  type.GetProperties();
            bool hasPk = false;
            foreach (var propInfo in propInfos)
            {
                var keyAttr = propInfo.GetCustomAttributes(typeof(KeyAttribute), true).FirstOrDefault() as KeyAttribute;
                //var autoGenAttr = propInfo.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), true).FirstOrDefault() as DatabaseGeneratedAttribute;
                var columnAttr = propInfo.GetCustomAttributes(typeof(ColumnAttribute), true)?.FirstOrDefault();
                string columnName = columnAttr != null ? (columnAttr as ColumnAttribute)?.Name : propInfo.Name;
                //var propValue = propInfo.GetValue(obj);

                var attribute = new EntityAttribute
                {
                    Name = columnName,
                    DisplayName = propInfo.Name,
                    //IsAutoGenerated = (autoGenAttr != null && autoGenAttr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity) ? true : false,
                    IsPrimaryKey = keyAttr != null ? true : false
                };

                if(attribute.IsPrimaryKey)
                    hasPk = true;

                entityAttrs.Add(attribute);
                entityDef.Attributes = entityAttrs;
            }
            
            // If entity definition has no any primary key defined using attribute annotation 
            // use first column named 'Id' to use as primary key
            if(!hasPk)
            {
                var pkAttr = entityDef.Attributes.FirstOrDefault(o=> o.Name.ToLower() == "id");
                if(pkAttr != null)
                    pkAttr.IsPrimaryKey = true;
            }

            return entityDef;
        }    

        private static string GetEntityName(Type type, out string description)
        {
            var tableAttr = type.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault();
            string entityName = type.Name;;
            description = null;
            if(tableAttr != null)
            {
                TableAttribute attr = tableAttr as TableAttribute;
                entityName = attr?.Name;
                description = attr?.Description;
            }
            return entityName;
        }

        private static SqlDbType GetPropertyDataType(Type type, string value, out string sqlCompatibleValue)
        {
            SqlDbType dbType = SqlDbType.NVarChar;

            if (type == typeof(String))
                dbType = SqlDbType.NVarChar;
            else if (type == typeof(Int32) || type == typeof(Int32?))
                dbType = SqlDbType.Int;
            else if (type == typeof(Int64) || type == typeof(Int64?))
                dbType = SqlDbType.BigInt;
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
                dbType = SqlDbType.DateTime;
            else if (type == typeof(bool) || type == typeof(bool?))
                dbType = SqlDbType.Bit;

            sqlCompatibleValue = GetSqlCompatibleValue(value, dbType);
            return dbType;
        }      

        private static string GetSqlCompatibleValue(string value, SqlDbType type)
        {
            if (value == null) return "NULL";
            string sqlCompatibleValue = value;
            switch (type)
            {
                case SqlDbType.Int:
                case SqlDbType.BigInt:
                case SqlDbType.SmallInt:
                case SqlDbType.TinyInt:
                    sqlCompatibleValue = value.ToString();
                    break;
                // case SqlDbType.Bit:
                //     sqlCompatibleValue = value.ToSqlBit().ToString();
                //     break;
                // case SqlDbType.DateTime:
                //     sqlCompatibleValue = value.ToSqlDateTime();
                //     break;
                // case SqlDbType.Timestamp:
                //     break;
                // case SqlDbType.Date:
                //     sqlCompatibleValue = value.ToSqlDate();
                //     break;
                // case SqlDbType.DateTime2:
                //     sqlCompatibleValue = value.ToSqlDateTime();
                //     break;
                // case SqlDbType.NVarChar:
                // case SqlDbType.VarChar:
                //     sqlCompatibleValue = value.ToSqlString();
                //     break;
                // default:
                //     sqlCompatibleValue = value.ToSqlString();
                //     break;
            }
            return sqlCompatibleValue;
        }          

        #endregion
    }
}
