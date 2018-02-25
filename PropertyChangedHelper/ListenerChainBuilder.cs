using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PropertyChangedHelper
{
    internal class ListenerChainBuilder
    {
        public PropertyChainEntry BuildListenerChain(INotifyPropertyChanged root, IEnumerable<PropertyInfo> propertyChain, Action callback)
        {
            return BuildChainEntry(root, null, propertyChain.First(), propertyChain.Skip(1), callback);
        }

        public PropertyChainEntry BuildListenerChain<T>(INotifyPropertyChanged root, IEnumerable<PropertyInfo> propertyChain, Action<T, T> callback)
        {
            return BuildChainEntry(root, null, propertyChain.First(), propertyChain.Skip(1), callback);
        }

        private PropertyChainEntry BuildChainEntry(INotifyPropertyChanged observedInstance, PropertyChainEntry parent, PropertyInfo observedProperty, IEnumerable<PropertyInfo> remainingChain, Action callback)
        {
            if (remainingChain == null || remainingChain.None())
                return BuildLastChainEntry(observedInstance, parent, observedProperty, callback);

            var entry = BuildMiddleEntry(observedInstance, parent, observedProperty);
            var nextInstance = GetNextInstance(observedInstance, observedProperty);
            entry.NextEntry = BuildChainEntry(nextInstance, entry, remainingChain.First(), remainingChain.Skip(1).ToArray(), callback);

            return entry;
        }
        
        private PropertyChainEntry BuildChainEntry<T>(INotifyPropertyChanged observedInstance, PropertyChainEntry parent, PropertyInfo observedProperty, IEnumerable<PropertyInfo> remainingChain, Action<T, T> callback)
        {
            if (remainingChain == null || remainingChain.None())
                return BuildLastChainEntry(observedInstance, parent, observedProperty, callback);

            var entry = new PropertyChainEntry(observedInstance, observedProperty, parent);
            var nextInstance = GetNextInstance(observedInstance, observedProperty);
            entry.NextEntry = BuildChainEntry(nextInstance, entry, remainingChain.First(), remainingChain.Skip(1).ToArray(), callback);
            
            return entry;
        }

        private PropertyChainEntry BuildMiddleEntry(INotifyPropertyChanged observedInstance, PropertyChainEntry parent, PropertyInfo observedProperty)
        {
            return new PropertyChainEntry(observedInstance, observedProperty, parent);
        }

        private PropertyChainEntry BuildLastChainEntry(INotifyPropertyChanged observedInstance, PropertyChainEntry parent, PropertyInfo observedProperty, Action callback)
        {
            return new LastPropertyChainEntry(observedInstance, observedProperty, parent) {
                Callback = callback
            };
        }

        private PropertyChainEntry BuildLastChainEntry<T>(INotifyPropertyChanged observedInstance, PropertyChainEntry parent, PropertyInfo observedProperty, Action<T, T> callback)
        {
            return new LastPropertyChainEntry<T>(observedInstance, observedProperty, parent) {
                Callback = callback
            };
        }

        private INotifyPropertyChanged GetNextInstance(INotifyPropertyChanged observedInstance, PropertyInfo observedProperty)
        {
            if (observedInstance != null)
                return (INotifyPropertyChanged)observedProperty.GetValue(observedInstance);
            else
                return null;
        }

    }
}
