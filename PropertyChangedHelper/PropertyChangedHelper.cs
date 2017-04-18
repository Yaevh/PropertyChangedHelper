using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyChangedHelper
{
    public class PropertyChangedHelper
    {
        public static void RaisePropertyChanged<T>(object source, PropertyChangedEventHandler handler, Expression<Func<T>> selectorExpression)
        {
            var body = selectorExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("The body must be a member expression");
            RaisePropertyChanged(source, handler, body.Member.Name);
        }

        public static void RaisePropertyChanged(object source, PropertyChangedEventHandler handler, string propertyName)
        {
            if (handler != null)
                handler.Invoke(source, new PropertyChangedEventArgs(propertyName));
        }


        private List<PropertyChainEntry> _handlers = new List<PropertyChainEntry>();


        /// <summary>
        /// Registers a callback that will be executed when property indicated by <paramref name="propertyExpression"/> changes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="rootObject"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="callback">callback to be executed; it will be passed two parameters: oldValue and newValue</param>
        /// <returns>token that can be disposed to unregister the callback</returns>
        public IDisposable AddPropertyChangedHandler<T, TViewModel>(TViewModel rootObject, Expression<Func<TViewModel, T>> propertyExpression, PropertyChangedHandler<T> callback) where TViewModel : INotifyPropertyChanged
        {
            var memberChain = BuildMemberChain(propertyExpression);
            var propertyChain = BuildChainEntry<T>(rootObject, memberChain.First(), memberChain.Skip(1).ToArray(), callback);
            _handlers.Add(propertyChain);
            return propertyChain;
        }

        /// <summary>
        /// Registers a callback that will be executed when property indicated by <paramref name="propertyExpression"/> changes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="rootObject"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="callback">action to be executed</param>
        /// <returns>token that can be disposed to unregister the callback</returns>
        public IDisposable AddPropertyChangedHandler<T, TViewModel>(TViewModel rootObject, Expression<Func<TViewModel, T>> propertyExpression, Action callback) where TViewModel : INotifyPropertyChanged
        {
            var memberChain = BuildMemberChain(propertyExpression);
            var propertyChain = BuildChainEntry(rootObject, memberChain.First(), memberChain.Skip(1).ToArray(), callback);
            _handlers.Add(propertyChain);
            return propertyChain;
        }

        /// <summary>
        /// Unregisters the callback associated with the token
        /// </summary>
        /// <param name="token"></param>
        public void RemovePropertyChangedHandler(IDisposable token)
        {
            var entry = token as PropertyChainEntry;
            if (entry != null)
            {
                entry.Dispose();
                _handlers.Remove(entry);
            }
        }

        /// <summary>
        /// Unregisters all callbacks registered on this instance
        /// </summary>
        public void ClearPropertyChangedHandlers()
        {
            foreach (var handler in _handlers)
                handler.Dispose();
            _handlers.Clear();
        }


        private PropertyInfo[] BuildMemberChain<T, TViewModel>(Expression<Func<TViewModel, T>> propertyExpression)
        {
            var members = new List<PropertyInfo>();
            for (var currentExpression = propertyExpression.Body;
                currentExpression != null && currentExpression is MemberExpression;
                currentExpression = (currentExpression as MemberExpression).Expression)
            {
                members.Add((currentExpression as MemberExpression).Member as PropertyInfo);
            }
            members.Reverse();
            return members.ToArray();
        }

        private PropertyChainEntry BuildLastChainEntry<T>(INotifyPropertyChanged observedInstance, PropertyInfo observedProperty, PropertyChangedHandler<T> callback)
        {
            var entry = new LastPropertyChainEntry<T>() {
                ObservedProperty = observedProperty,
                TypedCallback = callback
            };
            entry.Attach(observedInstance);
            return entry;
        }

        private PropertyChainEntry BuildLastChainEntry(INotifyPropertyChanged observedInstance, PropertyInfo observedProperty, Action callback)
        {
            var entry = new LastPropertyChainEntry() {
                ObservedProperty = observedProperty,
                Callback = callback
            };
            entry.Attach(observedInstance);
            return entry;
        }

        private PropertyChainEntry BuildChainEntry<T>(INotifyPropertyChanged observedInstance, PropertyInfo observedProperty, PropertyInfo[] remainingChain, PropertyChangedHandler<T> callback)
        {
            if (remainingChain == null || remainingChain.None())
                return BuildLastChainEntry(observedInstance, observedProperty, callback);

            var entry = new PropertyChainEntry() {
                ObservedProperty = observedProperty
            };
            INotifyPropertyChanged nextInstance = null;
            if (observedInstance != null)
                nextInstance = observedProperty.GetValue(observedInstance) as INotifyPropertyChanged;
            entry.NextEntry = BuildChainEntry(nextInstance, remainingChain.First(), remainingChain.Skip(1).ToArray(), callback);
            entry.Attach(observedInstance);
            return entry;
        }

        private PropertyChainEntry BuildChainEntry(INotifyPropertyChanged observedInstance, PropertyInfo observedProperty, PropertyInfo[] remainingChain, Action callback)
        {
            if (remainingChain == null || remainingChain.None())
                return BuildLastChainEntry(observedInstance, observedProperty, callback);

            var entry = new PropertyChainEntry() {
                ObservedProperty = observedProperty
            };
            INotifyPropertyChanged nextInstance = null;
            if (observedInstance != null)
                nextInstance = observedProperty.GetValue(observedInstance) as INotifyPropertyChanged;
            entry.NextEntry = BuildChainEntry(nextInstance, remainingChain.First(), remainingChain.Skip(1).ToArray(), callback);
            entry.Attach(observedInstance);
            return entry;
        }
        
    }
}
