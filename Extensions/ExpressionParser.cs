using DistributedCache.Models.Dto;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace DistributedCache.Extensions
{
    public static class ExpressionParser
    {

        public static Expression<Func<T, bool>> Parse<T>(ref T entity,string _params) where T : class
        {

            if (string.IsNullOrEmpty(_params))
            {
                throw new ArgumentNullException("Parameter is required.");
            }

            var _dynamicParams = JsonConvert.DeserializeObject<IList<DynamicParams>>(_params);

             
            try
            {
                IList<Expression<Func<T, bool>>> identifierExpressions = new List<Expression<Func<T, bool>>>();
                 
                // t
                var parameter = Expression.Parameter(typeof(T), "t");

                Expression<Func<T, bool>> _expression = null;

                foreach (var param in _dynamicParams)
                {
                    // t.Country
                    var propertyExpression = Expression.PropertyOrField(parameter, param.Property);

                    int.TryParse(param.Value, out int _intvalue);
                    DateTime.TryParse(param.Value, out DateTime _dtvalue);

                    Type _type =  _intvalue > 0 ? typeof(Nullable<Int32>) : (_dtvalue.Year > 0001 ? _dtvalue.GetType() : typeof(string));

                    // Canada
                    var constant = Expression.Constant( _intvalue > 0 ? _intvalue : (_dtvalue.Year > 0001 ? _dtvalue : param.Value), _type);
 
                    // t.Country == "Canada"
                    BinaryExpression expressionEval = param.Operator switch
                    {
                        "==" => Expression.Equal(propertyExpression, constant),
                        "<" => Expression.LessThan(propertyExpression, constant),
                        ">" => Expression.GreaterThan(propertyExpression, constant),
                        "<=" => Expression.LessThanOrEqual(propertyExpression, constant),
                        ">=" => Expression.GreaterThanOrEqual(propertyExpression, constant),
                        "!=" => Expression.NotEqual(propertyExpression, constant), 
                        "like" => Expression.Equal(propertyExpression, constant),
                        _ => throw new InvalidOperationException("Invalid operator")
                    };

                    if(param.Operator == "like")
                    {
                        _expression = GetSearchExpression<T>(parameter, param.Property, param.Value);
                    }
                    else
                    {
                        // t => t.Country == "Canada"
                        _expression = (Expression<Func<T, bool>>?)Expression.Lambda(expressionEval, parameter);
                    } 

                    identifierExpressions.Add(_expression);
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

        static Expression<Func<T, bool>> GetSearchExpression<T>(ParameterExpression parameterExp, string propertyName, string propertyValue)
        {
            var propertyExp = Expression.Property(parameterExp, propertyName);
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var someValue = Expression.Constant(propertyValue, typeof(string));
            var containsMethodExp = Expression.Call(propertyExp, method, someValue);

            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
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
