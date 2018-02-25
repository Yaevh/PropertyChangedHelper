using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PropertyChangedHelper
{
    public delegate void PropertyChangedHandler<T>(T oldValue, T newValue);

    /// <summary>
    /// Convenience class to omit type arguments in <see cref="PropertyChangedListener{TRoot, TProperty}"/>
    /// </summary>
    public class PropertyChangedHelper
    {
        /// <summary>
        /// Creates a listener that observes a property chain given by <paramref name="selectorExpression"/>
        /// and rooted in <paramref name="rootObject"/>, invoking <paramref name="callback"/> on change
        /// </summary>
        /// <typeparam name="TRoot"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="rootObject">root of the property chain</param>
        /// <param name="selectorExpression">expression pointing on a property to observe</param>
        /// <param name="callback">callback to invoke when property changes</param>
        /// <returns>token that can be disposed to unregister the callback</returns>
        public IDisposable BuildListener<TRoot, TProperty>(TRoot rootObject, Expression<Func<TRoot, TProperty>> selectorExpression, Action callback) where TRoot : INotifyPropertyChanged
        {
            return new PropertyChangedListener<TRoot, TProperty>(rootObject, selectorExpression, callback);
        }

        /// <summary>
        /// Creates a listener that observes a property chain given by <paramref name="selectorExpression"/>
        /// and rooted in <paramref name="rootObject"/>, invoking <paramref name="callback"/> on change
        /// </summary>
        /// <typeparam name="TRoot"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="rootObject">root of the property chain</param>
        /// <param name="selectorExpression">expression pointing on a property to observe</param>
        /// <param name="callback">callback to invoke when property changes</param>
        /// <returns>token that can be disposed to unregister the callback</returns>
        public IDisposable BuildListener<TRoot, TProperty>(TRoot rootObject, Expression<Func<TRoot, TProperty>> selectorExpression, Action<TProperty, TProperty> callback) where TRoot : INotifyPropertyChanged
        {
            return new PropertyChangedListener<TRoot, TProperty>(rootObject, selectorExpression, callback);
        }
    }
}
