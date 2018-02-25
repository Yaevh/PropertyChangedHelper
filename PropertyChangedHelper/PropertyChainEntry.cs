using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace PropertyChangedHelper
{
    internal class PropertyChainEntry : IDisposable
    {
        public PropertyInfo ObservedProperty { get; protected set; }
        
        public INotifyPropertyChanged ObservedInstance {
            get { return _observedInstance; }
            protected set {
                var oldInstance = _observedInstance;
                _observedInstance = value;
                OnObservedInstanceChanging(oldInstance, value);
            }
        }
        private INotifyPropertyChanged _observedInstance;

        public PropertyChainEntry NextEntry { get; set; }

        public PropertyChainEntry Parent { get; protected set; }


        public PropertyChainEntry(INotifyPropertyChanged observedInstance, PropertyInfo observedProperty, PropertyChainEntry parent)
        {
            ObservedProperty = observedProperty;
            ObservedInstance = observedInstance;
            Parent = parent;
        }

        protected virtual void OnObservedInstanceChanging(INotifyPropertyChanged oldValue, INotifyPropertyChanged newValue)
        {
            if (oldValue != null)
                oldValue.PropertyChanged -= HandleNextPropertyChanged;
            if (newValue != null)
                newValue.PropertyChanged += HandleNextPropertyChanged;
        }
        
        private void HandleNextPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != ObservedProperty.Name)
                return;
            var newValue = ObservedProperty.GetValue(ObservedInstance);
            if (newValue != null && !(newValue is INotifyPropertyChanged))
                throw new InvalidOperationException();
            NextEntry.Reattach(newValue as INotifyPropertyChanged);
        }
        
        protected virtual void Reattach(INotifyPropertyChanged observedInstance)
        {
            ObservedInstance = observedInstance;

            if (observedInstance == null || NextEntry == null)
                return;

            var nextInstance = ObservedProperty.GetValue(ObservedInstance);
            if (nextInstance != null && !(nextInstance is INotifyPropertyChanged))
                throw new InvalidOperationException();
            NextEntry.Reattach(nextInstance as INotifyPropertyChanged);
        }


        public virtual void Dispose()
        {
            if (ObservedInstance != null)
                ObservedInstance.PropertyChanged -= HandleNextPropertyChanged;
            if (NextEntry != null)
                NextEntry.Dispose();
        }
    }
}
