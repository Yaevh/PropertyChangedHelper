using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PropertyChangedHelper
{
    internal class MemberChainBuilder
    {
        public PropertyInfo[] BuildMemberChain<T, TViewModel>(Expression<Func<TViewModel, T>> propertyExpression)
        {
            return Enumerate(propertyExpression ?? throw new ArgumentNullException(nameof(propertyExpression)))
                .Reverse().ToArray();
        }

        public PropertyInfo[] BuildMemberChain(Expression propertyExpression)
        {
            return Enumerate(propertyExpression ?? throw new ArgumentNullException(nameof(propertyExpression)))
                .Reverse().ToArray();
        }

        private IEnumerable<PropertyInfo> Enumerate(Expression expression, bool notifyingMembersOnly = false)
        {
            Expression currentExpression = expression;
            Expression rootMemberExpression = (expression as LambdaExpression)?.Body;
            while (currentExpression != null)
            {
                switch (currentExpression.NodeType)
                {
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        currentExpression = (currentExpression as UnaryExpression).Operand;
                        foreach (var pi in Enumerate(currentExpression))
                            yield return pi;
                        // we've reached the root, end enumeration
                        yield break;
                        
                    case ExpressionType.Lambda:
                        currentExpression = (currentExpression as LambdaExpression).Body;
                        break;

                    case ExpressionType.MemberAccess:
                        var memberExpression = currentExpression as MemberExpression;
                        if (!(memberExpression.Member is PropertyInfo))
                            throw BuildInvalidExpressionTypeExceptionFor(currentExpression);
                        var propertyInfo = memberExpression.Member as PropertyInfo;
                        // every member except first must implement INotifyPropertyChanged
                        if (currentExpression != rootMemberExpression && !propertyInfo.PropertyType.DerivesFrom(typeof(INotifyPropertyChanged)))
                            throw BuildNotNotifyingExceptionFor(propertyInfo);
                        yield return propertyInfo;
                        currentExpression = memberExpression.Expression;
                        break;

                    case ExpressionType.Parameter:
                        // we've reached the root, end enumeration
                        yield break;

                    case ExpressionType.TypeAs:
                        currentExpression = (currentExpression as UnaryExpression).Operand;
                        foreach (var pi in Enumerate(currentExpression))
                            yield return pi;
                        // we've reached the root, end enumeration
                        yield break;

                    default:
                        throw BuildInvalidExpressionTypeExceptionFor(currentExpression);
                }
            }
        }
        
        private Exception BuildNotNotifyingExceptionFor(PropertyInfo pi)
        {
            return new NotSupportedException($"Type {pi.PropertyType} does not implement {nameof(INotifyPropertyChanged)} interface");
        }

        private Exception BuildInvalidExpressionTypeExceptionFor(Expression expression)
        {
            return new NotSupportedException($"Expression {expression} is not supported. Currently supported expression types are: {ExpressionType.Convert}, {ExpressionType.ConvertChecked} and {ExpressionType.MemberAccess} for property access");
        }
    }
}
