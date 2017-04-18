using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyChangedHelper
{
    internal class LastPropertyChainEntry : PropertyChainEntry
    {
        public Action Callback { get; set; }

        public override void Attach(INotifyPropertyChanged observedInstance)
        {
            if (ObservedInstance != null)
                ObservedInstance.PropertyChanged -= InvokeCallback;
            ObservedInstance = observedInstance;
            if (ObservedInstance != null)
                ObservedInstance.PropertyChanged += InvokeCallback;
            PerformInvokeCallback();
        }

        private void InvokeCallback(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ObservedProperty.Name)
                PerformInvokeCallback();
        }

        private void PerformInvokeCallback()
        {
            if (Callback != null)
                Callback();
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
        public PropertyChangedHandler<T> TypedCallback { get; set; }
        private T _oldValue = default(T);

        public override void Attach(INotifyPropertyChanged observedInstance)
        {
            if (ObservedInstance != null)
                ObservedInstance.PropertyChanged -= InvokeCallback;
            ObservedInstance = observedInstance;
            if (ObservedInstance != null)
                ObservedInstance.PropertyChanged += InvokeCallback;
            PerformInvokeCallback();
        }

        private void InvokeCallback(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ObservedProperty.Name)
                PerformInvokeCallback();
        }

        private void PerformInvokeCallback()
        {
            if (TypedCallback == null)
                return;
            var newValue = ObservedInstance != null ? (T)ObservedProperty.GetValue(ObservedInstance) : default(T);
            TypedCallback(_oldValue, newValue);
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
