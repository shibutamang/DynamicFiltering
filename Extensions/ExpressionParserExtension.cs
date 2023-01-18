using DistributedCache.Models.Dto;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace DistributedCache.Extensions
{
    public static class ExpressionParserExtension
    {

        public static IQueryable<T> WithDynamicFilter<T>(this IQueryable<T> source, IList<FilterObject> _params) where T : class
        {
            var _expr = Parse<T>(_params);
            return source.Where(_expr).AsQueryable<T>();
        }
        public static Expression<Func<T, bool>> Parse<T>(IList<FilterObject> _params) where T : class
        {

            if (_params == null || !(_params.Count > 0))
            {
                throw new ArgumentNullException("Filter parameter is required.");
            }

            //var _dynamicParams = JsonConvert.DeserializeObject<IList<DynamicParams>>(_params);

             
            try
            {
                IList<Expression<Func<T, bool>>> identifierExpressions = new List<Expression<Func<T, bool>>>();
                 
                // t
                var parameter = Expression.Parameter(typeof(T), "t");

                Expression<Func<T, bool>> _expression = null;

                foreach (var param in _params)
                {

                    //_expression = GetSearchExpression<T>(parameter, param.Property, param.Values.First());

                    
                    //t => list.Contains(t.Country)

                    // t.Country
                    var propertyExpression = Expression.PropertyOrField(parameter, param.Property);

                    int.TryParse(param.Values[0], out int _intvalue);
                    DateTime.TryParse(param.Values[0], out DateTime _dtvalue);

                    Type _type = _intvalue > 0 ? typeof(int?) : (_dtvalue.Year > 0001 ? _dtvalue.GetType() : typeof(string));

                    // Canada
                    var constant = Expression.Constant(_intvalue > 0 ? _intvalue : (_dtvalue.Year > 0001 ? _dtvalue : param.Values[0]), _type);

                    // t.Country == "Canada"
                    (BinaryExpression, Expression <Func<T, bool>>) expressionEval = param.Operator switch
                    {
                        "==" => (Expression.Equal(propertyExpression, constant), null),
                        "<" => (Expression.LessThan(propertyExpression, constant), null),
                        ">" => (Expression.GreaterThan(propertyExpression, constant), null),
                        "<=" => (Expression.LessThanOrEqual(propertyExpression, constant), null),
                        ">=" => (Expression.GreaterThanOrEqual(propertyExpression, constant), null),
                        "!=" => (Expression.NotEqual(propertyExpression, constant), null),
                        "like" => (null, GetSearchExpression<T>(parameter, param.Property, param.Values)),
                        _ => throw new InvalidOperationException("Invalid operator")
                    };

                    if (param.Operator != "like")
                    {

                        // t => t.Country == "Canada"
                        _expression = (Expression<Func<T, bool>>?)Expression.Lambda(expressionEval.Item1, parameter);
                    }


                    identifierExpressions.Add(_expression == null ? expressionEval.Item2 : _expression);
                }

                Expression<Func<T, bool>> allExpresions = null;

                foreach (Expression<Func<T, bool>> identifierExpression in identifierExpressions)
                { 

                    Expression<Func<T, bool>> equalExpression2 = Expression.Lambda<Func<T, bool>>(identifierExpression.Body, identifierExpression.Parameters);
             
                    if (allExpresions == null)
                    {
                        allExpresions = equalExpression2;
                    }
                    else
                    {
                        var visitor = new ExpressionSubstitute(allExpresions.Parameters[0], identifierExpression.Parameters[0]);
                        var modifiedAll = (Expression<Func<T, bool>>)visitor.Visit(allExpresions);
                        BinaryExpression bin = Expression.And(modifiedAll.Body, equalExpression2.Body);
                        allExpresions = Expression.Lambda<Func<T, bool>>(bin, identifierExpression.Parameters);
                    }
                } 

                return allExpresions;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            } 
        }

        static Expression<Func<T, bool>> GetSearchExpression<T>(ParameterExpression parameterExp, string propertyName, IList<string> propertyValue)
        {
            var propertyExp = Expression.Property(parameterExp, propertyName);
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var someValue = Expression.Constant(propertyValue.First(), typeof(string));
            var containsMethodExp = Expression.Call(propertyExp, method, someValue);

            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
        }

        static Expression<Func<T, bool>> GetSearchInListExpression<T>(ParameterExpression parameterExp, string propertyName, IList<string> propertyValue)
        {
            //var parent = Expression.Parameter(typeof(T));
            //var property = Expression.Property(parent, propertyName);
            //var anyMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            //  .Single(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2)
            //  .MakeGenericMethod(new[] { typeof(string) });
            //var t = Expression.Parameter(typeof(string), "t");
            //var containsCall = Expression.Call(
            //  property,
            //  typeof(string).GetMethod("Contains", new[] { typeof(string) }),
            //  t
            //);
            //var anyCall = Expression.Call(
            //  anyMethod,
            //  Expression.Constant(propertyValue),
            //  Expression.Lambda(containsCall, t)
            //);

            ////Additionally, I have to ensure ProductClass is not null
            //var notNull = Expression.NotEqual(property, Expression.Constant(null));
            //var f = Expression.Lambda<Func<T, bool>>(anyCall, parent);
            //return Expression.Lambda<Func<T, bool>>(anyCall, parent);

            var p = Expression.Parameter(typeof(T), "t");
            var contains = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(string));
            var property = Expression.PropertyOrField(p, propertyName);
            var body = Expression.Call(contains, Expression.Constant(propertyValue), property);
            var d = Expression.Lambda<Func<T, bool>>(body, p);
            return Expression.Lambda<Func<T, bool>>(body, p);

            //var parameterEx = Expression.Parameter(typeof(T), "t");
            //var propertyExp = Expression.Property(parameterEx, propertyName);
            //MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            //var someValue = Expression.Constant(propertyValue.First(), typeof(string));
            //var containsMethodExp = Expression.Call(propertyExp, method, someValue);
            //var d = Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);

            //return d;
        }

        public class ExpressionSubstitute : ExpressionVisitor
        {
            public readonly Expression from, to;
            public ExpressionSubstitute(Expression from, Expression to)
            {
                this.from = from;
                this.to = to;
            }
            public override Expression Visit(Expression node)
            {
                if (node == from) return to;
                return base.Visit(node);
            }
        }

        public static class ExpressionSubstituteExtentions
        {
            public static Expression<Func<TEntity, TReturnType>> RewireLambdaExpression<TEntity, TReturnType>(Expression<Func<TEntity, TReturnType>> expression, ParameterExpression newLambdaParameter)
            {
                var newExp = new ExpressionSubstitute(expression.Parameters.Single(), newLambdaParameter).Visit(expression);
                return (Expression<Func<TEntity, TReturnType>>)newExp;
            }
        }

       
    }
}
