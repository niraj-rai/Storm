using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Storm.Core
{
    public class ExpressionEvaluator
    {
        public virtual object Visit(Expression exp)
        {
            //visitedExpressionIsTableColumn = false;

            if (exp == null)
                return string.Empty;

            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda(exp as LambdaExpression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess(exp as MemberExpression);
                case ExpressionType.Constant:
                    return VisitConstant(exp as ConstantExpression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                //    //return "(" + VisitBinary(exp as BinaryExpression) + ")";
                    return VisitBinary(exp as BinaryExpression);
                //case ExpressionType.Negate:
                //case ExpressionType.NegateChecked:
                //case ExpressionType.Not:
                //case ExpressionType.Convert:
                //case ExpressionType.ConvertChecked:
                //case ExpressionType.ArrayLength:
                //case ExpressionType.Quote:
                //case ExpressionType.TypeAs:
                //    return VisitUnary(exp as UnaryExpression);
                case ExpressionType.Parameter:
                    return VisitParameter(exp as ParameterExpression);
                //case ExpressionType.Call:
                //    return VisitMethodCall(exp as MethodCallExpression);
                case ExpressionType.New:
                    return VisitNew(exp as NewExpression);
                //case ExpressionType.NewArrayInit:
                //case ExpressionType.NewArrayBounds:
                //    return VisitNewArray(exp as NewArrayExpression);
                //case ExpressionType.MemberInit:
                //    return VisitMemberInit(exp as MemberInitExpression);
                //case ExpressionType.Index:
                //    return VisitIndexExpression(exp as IndexExpression);
                //case ExpressionType.Conditional:
                //    return VisitConditional(exp as ConditionalExpression);
                default:
                    return exp.ToString();
            }
        }

        protected virtual object VisitParameter(ParameterExpression p)
        {
            var props = p.Type.GetProperties();
            return new SelectList(props.ToList()
                .Select(o => new SelectItemExpression(o.Name, o.Name)));
        }

        protected virtual object VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
                return new PartialSqlString("null");

            return c.Value;
        }

        protected virtual object VisitLambda(LambdaExpression lambda)
        {
            var sep = " ";
            if (lambda.Body.NodeType == ExpressionType.MemberAccess && sep == " ")
            {
                MemberExpression m = lambda.Body as MemberExpression;

                if (m.Expression != null)
                {
                    var r = VisitMemberAccess(m);
                    if (!(r is PartialSqlString))
                        return r;

                    if (m.Expression.Type.IsNullableType())
                        return r.ToString();

                    return $"{r}={'1'}";
                }

            }
            return Visit(lambda.Body);
        }

        protected virtual object VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null)
            {
                if (m.Member.DeclaringType.IsNullableType())
                {
                    if (m.Member.Name == "Value") //Can't use C# 6 yet: nameof(Nullable<bool>.Value)
                        return Visit(m.Expression);
                    if (m.Member.Name == "HasValue") //nameof(Nullable<bool>.HasValue)
                    {
                        var doesNotEqualNull = Expression.MakeBinary(ExpressionType.NotEqual, m.Expression, Expression.Constant(null));
                        return Visit(doesNotEqualNull); // Nullable<T>.HasValue is equivalent to "!= null"
                    }

                    throw new ArgumentException($"Expression '{m}' accesses unsupported property '{m.Member}' of Nullable<T>");
                }

                if (IsParameterOrConvertAccess(m))
                    return GetMemberExpression(m);
            }
            return Visit(m?.Expression);

            //return CachedExpressionCompiler.Evaluate(m);
        }

        protected virtual object GetMemberExpression(MemberExpression m)
        {
            var propertyInfo = m.Member as PropertyInfo;

            var modelType = m.Expression.Type;
            if (m.Expression.NodeType == ExpressionType.Convert)
            {
                if (m.Expression is UnaryExpression unaryExpr)
                {
                    modelType = unaryExpr.Operand.Type;
                }
            }

            //OnVisitMemberType(modelType);

            //var tableDef = modelType.GetModelDefinition();

            //if (propertyInfo != null && propertyInfo.PropertyType.IsEnum)
            //    return new EnumMemberAccess(
            //        GetQuotedColumnName(tableDef, m.Member.Name), propertyInfo.PropertyType);

            return new PartialSqlString(m.Member.Name);
        }

        bool skipParameterizationForThisExpression = true;
        public virtual object GetValue(object value, Type type)
        {
            if (skipParameterizationForThisExpression)
                return GetQuotedValue(value, type);

            var paramValue = GetParamValue(value, type);
            return paramValue ?? "null";
        }

        protected virtual object VisitBinary(BinaryExpression b)
        {
            object originalLeft = null, originalRight = null, left, right;
            var operand = BindOperant(b.NodeType);   //sep= " " ??
            if (operand == "AND" || operand == "OR")
            {
                if (IsBooleanComparison(b.Left))
                {
                    left = VisitMemberAccess((MemberExpression) b.Left);
                    if (left is PartialSqlString)
                        left = new PartialSqlString($"{left}={GetQuotedTrueValue()}");
                }
                else left = Visit(b.Left);

                if (IsBooleanComparison(b.Right))
                {
                    right = VisitMemberAccess((MemberExpression)b.Right);
                    if (right is PartialSqlString)
                        right = new PartialSqlString($"{right}={GetQuotedTrueValue()}");
                }
                else right = Visit(b.Right);

                if (!(left is PartialSqlString) && !(right is PartialSqlString))
                {
                    var result = PreEvaluateBinary(b, left, right);
                    return result;
                }

                if (!(left is PartialSqlString))
                    left = ((bool)left) ? GetTrueExpression() : GetFalseExpression();
                if (!(right is PartialSqlString))
                    right = ((bool)right) ? GetTrueExpression() : GetFalseExpression();
            }
            else if ((operand == "=" || operand == "<>") && b.Left is MethodCallExpression && ((MethodCallExpression)b.Left).Method.Name == "CompareString")
            {
                //Handle VB.NET converting (x => x.Name == "Foo") into (x => CompareString(x.Name, "Foo", False)
                var methodExpr = (MethodCallExpression)b.Left;
                var args = this.VisitExpressionList(methodExpr.Arguments);
                right = GetValue(args[1], typeof(string));
                ConvertToPlaceholderAndParameter(ref right);
                return new PartialSqlString($"({args[0]} {operand} {right})");
            }
            else
            {
                originalLeft = left = Visit(b.Left);
                originalRight = right = Visit(b.Right);

                // Handle "expr = true/false", including with the constant on the left

                if (operand == "=" || operand == "<>")
                {
                    if (left is bool)
                    {
                        Swap(ref left, ref right); // Should be safe to swap for equality/inequality checks
                    }

                    if (right is bool && !IsFieldName(left)) // Don't change anything when "expr" is a column name - then we really want "ColName = 1"
                    {
                        if (operand == "=")
                            return (bool)right ? left : GetNotValue(left); // "expr == true" becomes "expr", "expr == false" becomes "not (expr)"
                        if (operand == "<>")
                            return (bool)right ? GetNotValue(left) : left; // "expr != true" becomes "not (expr)", "expr != false" becomes "expr"
                    }
                }

                var leftEnum = left as EnumMemberAccess;
                var rightEnum = right as EnumMemberAccess;

                var rightNeedsCoercing = leftEnum != null && rightEnum == null;
                var leftNeedsCoercing = rightEnum != null && leftEnum == null;

                if (rightNeedsCoercing)
                {
                    var rightPartialSql = right as PartialSqlString;
                    if (rightPartialSql == null)
                    {
                        right = GetValue(right, leftEnum.EnumType);
                    }
                }
                else if (leftNeedsCoercing)
                {
                    var leftPartialSql = left as PartialSqlString;
                    if (leftPartialSql == null)
                    {
                        left = GetQuotedValue(left, rightEnum.EnumType);
                    }
                }
                else if (!(left is PartialSqlString) && !(right is PartialSqlString))
                {
                    var evaluatedValue = PreEvaluateBinary(b, left, right);
                    var result = VisitConstant(Expression.Constant(evaluatedValue));
                    return result;
                }
                else if (!(left is PartialSqlString))
                {
                    left = GetQuotedValue(left, left?.GetType());
                }
                else if (!(right is PartialSqlString))
                {
                    right = GetValue(right, right?.GetType());
                }
            }

            if (left.ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                Swap(ref left, ref right); // "null is x" will not work, so swap the operands
            }

            var separator = " "; //sep;
            if (right.ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                if (operand == "=")
                    operand = "is";
                else if (operand == "<>")
                    operand = "is not";

                separator = " ";
            }

            if (operand == "+" && b.Left.Type == typeof(string) && b.Right.Type == typeof(string))
                return BuildConcatExpression(new List<object> {left, right});

            VisitFilter(operand, originalLeft, originalRight, ref left, ref right);

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return new PartialSqlString($"{operand}({left},{right})");
                default:
                    return new PartialSqlString("(" + left + separator + operand + separator + right + ")");
            }
        }

        bool visitedExpressionIsTableColumn = false;

        protected virtual void VisitFilter(string operand, object originalLeft, object originalRight, ref object left, ref object right)
        {
            if (skipParameterizationForThisExpression || visitedExpressionIsTableColumn)
                return;

            if (originalLeft is EnumMemberAccess && originalRight is EnumMemberAccess)
                return;

            if (operand == "AND" || operand == "OR" || operand == "is" || operand == "is not")
                return;

            if (!(right is PartialSqlString))
            {
                ConvertToPlaceholderAndParameter(ref right);
            }
        }

        private PartialSqlString BuildConcatExpression(List<object> args)
        {
            for (int i = 0; i < args.Count; i++)
            {
                if (!(args[i] is PartialSqlString))
                    args[i] = ConvertToParam(args[i]);
            }
            return ToConcatPartialString(args);
        }

        public string ConvertToParam(object value)
        {
            var p = AddParam(value);
            return p.ParameterName;
        }

        protected PartialSqlString ToConcatPartialString(List<object> args)
        {
            return new PartialSqlString(SqlConcat(args));
        }

        public virtual string SqlConcat(IEnumerable<object> args) => $"CONCAT({string.Join(", ", args)})";

        protected virtual bool IsFieldName(object quotedExp)
        {
            var fieldExpr = quotedExp.ToString().StripTablePrefixes();
            var unquotedExpr = fieldExpr.StripQuotes();
            return true;
            //var isTableField = modelDef.FieldDefinitionsArray
            //    .Any(x => GetColumnName(x.FieldName) == unquotedExpr);
            //if (isTableField)
            //    return true;

            //var isJoinedField = tableDefs.Any(t => t.FieldDefinitionsArray
            //    .Any(x => GetColumnName(x.FieldName) == unquotedExpr));

            //return isJoinedField;
        }

        private static void Swap(ref object left, ref object right)
        {
            var temp = right;
            right = left;
            left = temp;
        }

        protected virtual void ConvertToPlaceholderAndParameter(ref object right)
        {
            var parameter = AddParam(right);

            right = parameter.ParameterName;
        }

        public List<IDbDataParameter> Params { get; set; }
        public virtual IDbDataParameter AddParam(object value)
        {
            var paramName = Params.Count.ToString();
            var paramValue = value;

            var parameter = CreateParam(paramName, paramValue);
            Params.Add(parameter);
            return parameter;
        }

        public IDbDataParameter CreateParam(string name,
            object value = null,
            ParameterDirection direction = ParameterDirection.Input,
            DbType? dbType = null,
            DataRowVersion sourceVersion = DataRowVersion.Default)
        {
            var p = new SqlParameter();
            p.ParameterName = name;
            p.Direction = direction;

            //if (!IsMySqlConnector()) //throws NotSupportedException
            //{
            //    p.SourceVersion = sourceVersion;
            //}

            //if (p.DbType == DbType.String)
            //{
            //    p.Size = DialectProvider.GetStringConverter().StringLength;
            //    if (value is string strValue && strValue.Length > p.Size)
            //        p.Size = strValue.Length;
            //}

            if (value != null)
            {
                p.Value = GetParamValue(value, value.GetType());
            }
            else
            {
                p.Value = DBNull.Value;
            }

            if (dbType != null)
                p.DbType = dbType.Value;

            return p;
        }


        protected object GetTrueExpression()
        {
            return new PartialSqlString($"({GetQuotedTrueValue()}={GetQuotedTrueValue()})");
        }

        protected object GetFalseExpression()
        {
            return new PartialSqlString($"({GetQuotedTrueValue()}={GetQuotedFalseValue()})");
        }

        protected object GetQuotedTrueValue()
        {
            return new PartialSqlString("1");
        }

        protected object GetQuotedFalseValue()
        {
            return new PartialSqlString("0");
        }

        private object GetNotValue(object o)
        {
            if (!(o is PartialSqlString))
                return !(bool)o;

            if (IsFieldName(o))
                return new PartialSqlString(o + "=" + GetQuotedFalseValue());

            return new PartialSqlString("NOT (" + o + ")");
        }

        public virtual string GetQuotedValue(string paramValue)
        {
            return "'" + paramValue.Replace("\\", "\\\\").Replace("'", @"\'") + "'";
        }

        public virtual string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType == typeof(byte[]))
                return "0x" + BitConverter.ToString((byte[])value).Replace("-", "");

            return $"'{value.ToString()}'";
        }

        public virtual object GetParamValue(object value, Type fieldType)
        {
            return $"'{value.ToString()}'";
        }

        private BinaryExpression PreEvaluateBinary(BinaryExpression b, object left, object right)
        {
            var visitedBinaryExp = b;

            if (IsParameterAccess(b.Left) || IsParameterAccess(b.Right))
            {
                var eLeft = !IsParameterAccess(b.Left) ? b.Left : Expression.Constant(left, b.Left.Type);
                var eRight = !IsParameterAccess(b.Right) ? b.Right : Expression.Constant(right, b.Right.Type);
                if (b.NodeType == ExpressionType.Coalesce)
                    visitedBinaryExp = Expression.Coalesce(eLeft, eRight, b.Conversion);
                else
                    visitedBinaryExp = Expression.MakeBinary(b.NodeType, eLeft, eRight, b.IsLiftedToNull, b.Method);
            }

            return visitedBinaryExp;
        }

        /// <summary>
        /// Determines whether the expression is the parameter inside MemberExpression which should be compared with TrueExpression.
        /// </summary>
        /// <returns>Returns true if the specified expression is the parameter inside MemberExpression which should be compared with TrueExpression;
        /// otherwise, false.</returns>
        protected virtual bool IsBooleanComparison(Expression e)
        {
            if (!(e is MemberExpression)) return false;

            var m = (MemberExpression)e;

            if (m.Member.DeclaringType.IsNullableType() &&
                m.Member.Name == "HasValue") //nameof(Nullable<bool>.HasValue)
                return false;

            return IsParameterAccess(m);
        }

        /// <summary>
        /// Determines whether the expression is the parameter.
        /// </summary>
        /// <returns>Returns true if the specified expression is parameter;
        /// otherwise, false.</returns>
        protected virtual bool IsParameterAccess(Expression e)
        {
            return CheckExpressionForTypes(e, new[] { ExpressionType.Parameter });
        }

        /// <summary>
        /// Determines whether the expression is a Parameter or Convert Expression.
        /// </summary>
        /// <returns>Returns true if the specified expression is parameter or convert;
        /// otherwise, false.</returns>

        protected virtual bool IsParameterOrConvertAccess(Expression e)
        {
            return CheckExpressionForTypes(e, new[] { ExpressionType.Parameter, ExpressionType.Convert });
        }

        protected bool CheckExpressionForTypes(Expression e, ExpressionType[] types)
        {
            while (e != null)
            {
                if (types.Contains(e.NodeType))
                {
                    var subUnaryExpr = e as UnaryExpression;
                    var isSubExprAccess = subUnaryExpr?.Operand is IndexExpression;
                    if (!isSubExprAccess)
                        return true;
                }

                if (e is BinaryExpression binaryExpr)
                {
                    if (CheckExpressionForTypes(binaryExpr.Left, types))
                        return true;

                    if (CheckExpressionForTypes(binaryExpr.Right, types))
                        return true;
                }

                if (e is MethodCallExpression methodCallExpr)
                {
                    for (var i = 0; i < methodCallExpr.Arguments.Count; i++)
                    {
                        if (CheckExpressionForTypes(methodCallExpr.Arguments[i], types))
                            return true;
                    }

                    if (CheckExpressionForTypes(methodCallExpr.Object, types))
                        return true;
                }

                if (e is UnaryExpression unaryExpr)
                {
                    if (CheckExpressionForTypes(unaryExpr.Operand, types))
                        return true;
                }

                if (e is ConditionalExpression condExpr)
                {
                    if (CheckExpressionForTypes(condExpr.Test, types))
                        return true;

                    if (CheckExpressionForTypes(condExpr.IfTrue, types))
                        return true;

                    if (CheckExpressionForTypes(condExpr.IfFalse, types))
                        return true;
                }

                var memberExpr = e as MemberExpression;
                e = memberExpr?.Expression;
            }

            return false;
        }

        protected virtual object VisitNew(NewExpression nex)
        {
            var isAnonType = nex.Type.Name.StartsWith("<>");
            if (isAnonType)
            {
                var exprs = VisitExpressionList(nex.Arguments);

                for (var i = 0; i < exprs.Count; ++i)
                {
                    exprs[i] = SetAnonTypePropertyNamesForSelectExpression(exprs[i], nex.Arguments[i], nex.Members[i]);
                }

                return new SelectList(exprs);
            }
            return null;
            //return CachedExpressionCompiler.Evaluate(nex);
        }

        protected virtual List<object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            var list = new List<object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var e = original[i];
                if (e.NodeType == ExpressionType.NewArrayInit ||
                    e.NodeType == ExpressionType.NewArrayBounds)
                {
                    list.AddRange(VisitNewArrayFromExpressionList(e as NewArrayExpression));
                }
                else
                {
                    list.Add(Visit(e));
                }
            }
            return list;
        }

        protected virtual List<object> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {
            var exprs = VisitExpressionList(na.Expressions);
            return exprs;
        }

        protected virtual string BindOperant(ExpressionType e)
        {
            switch (e)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "MOD";
                case ExpressionType.Coalesce:
                    return "COALESCE";
                default:
                    return e.ToString();
            }
        }

        private object SetAnonTypePropertyNamesForSelectExpression(object expr, Expression arg, MemberInfo member)
        {
            // When selecting a column use the anon type property name, rather than the table property name, as the returned column name

            MemberExpression propertyExpr;
            if ((propertyExpr = arg as MemberExpression) != null && propertyExpr.Member.Name != member.Name)
                return new StringBuilder($"{expr.ToString()} AS [{member.Name}]");

            // When selecting an entire table use the anon type property name as a prefix for the returned column name
            // to allow the caller to distinguish properties with the same names from different tables

            var selectList = arg is ParameterExpression paramExpr && paramExpr.Name != member.Name
                ? expr as SelectList
                : null;
            if (selectList != null)
            {
                foreach (var item in selectList.Items)
                {
                    if (item is SelectItem selectItem)
                    {
                        if (!string.IsNullOrEmpty(selectItem.Alias))
                        {
                            selectItem.Alias = member.Name + selectItem.Alias;
                        }
                        else
                        {
                            if (item is SelectItemColumn columnItem)
                            {
                                columnItem.Alias = member.Name + columnItem.ColumnName;
                            }
                        }
                    }
                }
            }

            var methodCallExpr = arg as MethodCallExpression;
            var mi = methodCallExpr?.Method;
            var declareType = mi?.DeclaringType;
            if (declareType != null && declareType.Name == "Sql" && mi.Name != "Desc" && mi.Name != "Asc" && mi.Name != "As" && mi.Name != "AllFields")
                return new PartialSqlString(expr + " AS " + member.Name); // new { Count = Sql.Count("*") }

            return expr;
        }

        public class PartialSqlString
        {
            public PartialSqlString(string text)
            {
                Text = text;
            }
            public string Text { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        public class EnumMemberAccess : PartialSqlString
        {
            public EnumMemberAccess(string text, Type enumType)
                : base(text)
            {
                if (!enumType.IsEnum) throw new ArgumentException("Type not valid", nameof(enumType));

                EnumType = enumType;
            }

            public Type EnumType { get; private set; }
        }

        private class SelectList
        {
            public readonly IEnumerable<object> Items;

            public SelectList(IEnumerable<object> items)
            {
                this.Items = items;
            }

            public override string ToString()
            {
                return Items.ToSelectString();
            }
        }
    }

    public abstract class SelectItem
    {
        protected SelectItem(string alias)
        {
            Alias = alias;
        }

        /// <summary>
        /// Unquoted alias for the column or expression being selected.
        /// </summary>
        public string Alias { get; set; }

        //protected IOrmLiteDialectProvider DialectProvider { get; set; }

        public abstract override string ToString();
    }

    public class SelectItemExpression : SelectItem
    {
        public SelectItemExpression(string selectExpression, string alias)
            : base(alias)
        {
            if (string.IsNullOrEmpty(selectExpression))
                throw new ArgumentNullException(nameof(selectExpression));
            if (string.IsNullOrEmpty(alias))
                throw new ArgumentNullException(nameof(alias));

            SelectExpression = selectExpression;
            Alias = alias;
        }

        /// <summary>
        /// The SQL expression being selected, including any necessary quoting.
        /// </summary>
        public string SelectExpression { get; set; }

        public override string ToString()
        {
            var text = $"[{SelectExpression}]";
            if (!string.IsNullOrEmpty(Alias)) // Note that even though Alias must be non-empty in the constructor it may be set to null/empty later
                text += " AS " + $"[{Alias}]";
            return text;
        }
    }

    public class SelectItemColumn : SelectItem
    {
        public SelectItemColumn(string columnName, string columnAlias = null, string quotedTableAlias = null)
            : base(columnAlias)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName));

            ColumnName = columnName;
            QuotedTableAlias = quotedTableAlias;
        }

        /// <summary>
        /// Unquoted column name being selected.
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Table name or alias used to prefix the column name, if any. Already quoted.
        /// </summary>
        public string QuotedTableAlias { get; set; }

        public override string ToString()
        {
            var text = $"[{ColumnName}]";

            if (!string.IsNullOrEmpty(QuotedTableAlias))
                text = QuotedTableAlias + "." + text;
            if (!string.IsNullOrEmpty(Alias))
                text += " AS " + $"[{Alias}]";

            return text;
        }
    }

    public static class Extension
    {
        public static object Evaluate(this Expression exp)
        {
            if (exp != null)
            {
                var eval = new ExpressionEvaluator();
                var val = eval.Visit(exp);
                return val;
            }
            return exp;
        }
        public static string ToSelectString<TItem>(this IEnumerable<TItem> items)
        {
            var sb = new StringBuilder();

            foreach (var item in items)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(item);
            }

            return sb.ToString();
        }

        public static bool IsNullableType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return true;
            return false;
        }

        public static string StripTablePrefixes(this string selectExpression)
        {
            if (selectExpression.IndexOf('.') < 0)
                return selectExpression;

            var sb = new StringBuilder();
            var tokens = selectExpression.Split(' ');
            foreach (var token in tokens)
            {
                var parts = token.Split('.');
                if (parts.Length > 1)
                {
                    sb.Append(" " + parts[parts.Length - 1]);
                }
                else
                {
                    sb.Append(" " + token);
                }
            }

            return sb.ToString().Trim();
        }

        public static char[] QuotedChars = new[] { '"', '`', '[', ']' };

        public static string StripQuotes(this string quotedExpr)
        {
            return quotedExpr.Trim(QuotedChars);
        }
    }
}
