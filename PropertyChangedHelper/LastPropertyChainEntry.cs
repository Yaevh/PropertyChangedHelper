using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace PropertyChangedHelper
{
    internal class LastPropertyChainEntry : PropertyChainEntry
    {
        private readonly bool _isInitialized = false;

        public LastPropertyChainEntry(INotifyPropertyChanged observedInstance, PropertyInfo observedProperty, PropertyChainEntry parent) :
            base(observedInstance, observedProperty, parent)
        {
            _isInitialized = true;
        }

        public Action Callback { get; set; }


        protected override void OnObservedInstanceChanging(INotifyPropertyChanged oldValue, INotifyPropertyChanged newValue)
        {
            if (oldValue != null)
                oldValue.PropertyChanged -= InvokeCallback;
            if (newValue != null)
                newValue.PropertyChanged += InvokeCallback;

            if (!_isInitialized)
                return;
        }

        protected override void Reattach(INotifyPropertyChanged observedInstance)
        {
            base.Reattach(observedInstance);
            Callback?.Invoke();
        }

        private void InvokeCallback(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ObservedProperty.Name)
                Callback?.Invoke();
        }

        public override void Dispose()
        {
            if (ObservedInstance == null)
                return;
            ObservedInstance.PropertyChanged -= InvokeCallback;
            base.Dispose();
        }
    }

    internal class LastPropertyChainEntry<T> : PropertyChainEntry
    {
        private readonly bool _isInitialized = false;

        private T _oldValue = default(T);


        public LastPropertyChainEntry(INotifyPropertyChanged observedInstance, PropertyInfo observedProperty, PropertyChainEntry parent) :
            base(observedInstance, observedProperty, parent)
        {
            _isInitialized = true;
        }


        public Action<T, T> Callback { get; set; }
        
        protected override void OnObservedInstanceChanging(INotifyPropertyChanged oldInstance, INotifyPropertyChanged newInstance)
        {
            if (oldInstance != null)
                oldInstance.PropertyChanged -= InvokeCallback;
            if (newInstance != null)
                newInstance.PropertyChanged += InvokeCallback;
            
            if (_isInitialized)
                PerformInvokeCallback();

            if (newInstance == null)
                _oldValue = default(T);
            else
                _oldValue = (T)ObservedProperty.GetValue(newInstance);
        }

        protected override void Reattach(INotifyPropertyChanged observedInstance)
        {
            base.Reattach(observedInstance);
            PerformInvokeCallback();
        }

        private void InvokeCallback(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ObservedProperty.Name)
                PerformInvokeCallback();
        }

        private void PerformInvokeCallback()
        {
            var newValue = ObservedInstance != null ? (T)ObservedProperty.GetValue(ObservedInstance) : default(T);

            if (_oldValue == null && newValue == null)
                return;
            if (_oldValue != null && _oldValue.Equals(newValue))
                return;

            Callback?.Invoke(_oldValue, newValue);

            _oldValue = newValue;
        }

        public override void Dispose()
        {
            if (ObservedInstance == null)
                return;
            ObservedInstance.PropertyChanged -= InvokeCallback;
            base.Dispose();
        }
    }
}
