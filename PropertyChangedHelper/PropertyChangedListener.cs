using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

namespace PropertyChangedHelper
{
    /// <summary>
    /// Observes <see cref="INotifyPropertyChanged.PropertyChanged"/> events and invokes callback
    /// </summary>
    /// <typeparam name="TRoot"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public class PropertyChangedListener<TRoot, TProperty> : IDisposable
        where TRoot : INotifyPropertyChanged
    {
        private readonly PropertyChainEntry _listenerChain;

        /// <summary>
        /// Creates a listener that observes a property chain given by <paramref name="selectorExpression"/>
        /// and rooted in <paramref name="rootObject"/>, invoking <paramref name="callback"/> on change
        /// </summary>
        /// <param name="rootObject">root of the property chain</param>
        /// <param name="selectorExpression">expression pointing on a property to observe</param>
        /// <param name="callback">callback to invoke when property changes</param>
        public PropertyChangedListener(TRoot rootObject, Expression<Func<TRoot, TProperty>> selectorExpression, Action callback)
        {
            var memberChain = new MemberChainBuilder().BuildMemberChain(selectorExpression);
            _listenerChain = new ListenerChainBuilder().BuildListenerChain<TProperty>(rootObject, memberChain, callback);
        }

        /// <summary>
        /// Creates a listener that observes a property chain given by <paramref name="selectorExpression"/>
        /// and rooted in <paramref name="rootObject"/>, invoking <paramref name="callback"/> on change
        /// </summary>
        /// <param name="rootObject">root of the property chain</param>
        /// <param name="selectorExpression">expression pointing on a property to observe</param>
        /// <param name="callback">callback to invoke when property changes; first argument to the callback is the old value of the observed property, second argument is the new value</param>
        public PropertyChangedListener(TRoot rootObject, Expression<Func<TRoot, TProperty>> selectorExpression, Action<TProperty, TProperty> callback)
        {
            var memberChain = new MemberChainBuilder().BuildMemberChain(selectorExpression);
            _listenerChain = new ListenerChainBuilder().BuildListenerChain(rootObject, memberChain, callback);
        }

        /// <summary>
        /// Disposes of current instance, unsubscribing from events
        /// </summary>
        public void Dispose()
        {
            _listenerChain?.Dispose();
        }
    }
}
