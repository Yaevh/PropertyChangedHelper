using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyChangedHelper
{
    internal class PropertyChainEntry : IDisposable
    {
        public PropertyInfo ObservedProperty { get; set; }
        public PropertyChainEntry NextEntry { get; set; }
        public INotifyPropertyChanged ObservedInstance { get; protected set; }

        public virtual void Attach(INotifyPropertyChanged observedInstance)
        {
            if (ObservedInstance != null)
                ObservedInstance.PropertyChanged -= HandleNextPropertyChanged;
            ObservedInstance = observedInstance;
            if (ObservedInstance != null)
                ObservedInstance.PropertyChanged += HandleNextPropertyChanged;
        }

        private void Reattach(INotifyPropertyChanged observedInstance)
        {
            Attach(observedInstance);
            INotifyPropertyChanged nextInstance = null;
            if (observedInstance != null && NextEntry != null)
            {
                nextInstance = ObservedProperty.GetValue(observedInstance) as INotifyPropertyChanged;
                NextEntry.Reattach(nextInstance);
            }
        }

        private void HandleNextPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != ObservedProperty.Name)
                return;
            var newValue = ObservedProperty.GetValue(ObservedInstance) as INotifyPropertyChanged;
            NextEntry.Reattach(newValue);
        }

        public virtual void Dispose()
        {
            if (ObservedInstance == null)
                return;
            ObservedInstance.PropertyChanged -= HandleNextPropertyChanged;
            if (NextEntry != null)
                NextEntry.Dispose();
        }
    }
}
