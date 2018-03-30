using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyChangedHelper.Tests.PropertyChangedListenerSpec
{
    public class NotNotifyingSampleObject
    {
        public NotNotifyingSampleObject NotNotifyingChild { get; set; }

        public NotNotifyingSampleObject Field;

        public NotNotifyingSampleObject SampleMethod()
        {
            return new NotNotifyingSampleObject();
        }
    }

    public class NotifyingSampleObject : NotNotifyingSampleObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NotifyingSampleObject Child { get { return _child; } set { _child = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child))); } }
        private NotifyingSampleObject _child;

        public string Value { get { return _value; } set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); } }
        private string _value;
    }

    public class NotifyingSampleObject2 : NotifyingSampleObject { }


    [TestClass]
    public class GivenPropertyChangedListenerBuilder
    {
        Action<object, object> _dummyCallback = (o, n) => { };

        
        [TestMethod]
        public void WhenMemberChainContainsField_ThenThrowNotSupportedExceptionOnBuilding()
        {
            var target = new NotifyingSampleObject();

            Assert.ThrowsException<NotSupportedException>(() =>
            {
                var listener = new PropertyChangedHelper().BuildListener(target, x => x.Field, _dummyCallback);
            });
        }

        [TestMethod]
        public void WhenMemberChainContainsNotNotifyingProperty_ThenThrowNotSupportedExceptionOnBuilding()
        {
            var target = new NotifyingSampleObject();

            Assert.ThrowsException<NotSupportedException>(() =>
            {
                var listener = new PropertyChangedHelper().BuildListener(target, x => x.NotNotifyingChild.NotNotifyingChild, _dummyCallback);
            });
        }
        
        [TestMethod]
        public void WhenMemberChainContainsMethod_ThenThrowNotSupportedExceptionOnBuilding()
        {
            var target = new NotifyingSampleObject();

            Assert.ThrowsException<NotSupportedException>(() =>
            {
                var listener = new PropertyChangedHelper().BuildListener(target, x => x.SampleMethod().NotNotifyingChild, _dummyCallback);
            });
        }

        [TestMethod]
        public void WhenMemberChainContainsCastingOperator_ThenBuildProperListener()
        {
            var target = new NotifyingSampleObject();

            var listener = new PropertyChangedHelper().BuildListener(target, x => (x.Child as NotifyingSampleObject2).Child, _dummyCallback);
            Assert.IsTrue(listener != null);
        }
        
    }

    [TestClass]
    public class GivenListenerWithCallback
    {
        [TestMethod]
        public void WhenObservedPropertyChanges_ThenCallbackIsInvoked()
        {
            bool wasCalled = false;
            var target = new NotifyingSampleObject() { Child = new NotifyingSampleObject() { Value = "foo" } };
            var listener = new PropertyChangedHelper().BuildListener(target, x => x.Child.Value, () => { wasCalled = true; });

            target.Child.Value = "bar";

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void WhenNoChangeToObservedProperty_ThenCallbackIsNotInvoked()
        {
            bool wasCalled = false;
            var target = new NotifyingSampleObject() { Child = new NotifyingSampleObject() { Value = "foo" } };
            var listener = new PropertyChangedHelper().BuildListener(target, x => x.Child.Value, () => { wasCalled = true; });

            target.Child.Value = "foo";

            Assert.IsFalse(wasCalled);
        }

        [TestMethod]
        public void WhenIntermediatePropertyChanges_ThenCallbackIsInvoked()
        {
            var wasCalled = false;
            var target = new NotifyingSampleObject() { Child = new NotifyingSampleObject() { Value = "foo" } };
            new PropertyChangedHelper().BuildListener(target, x => x.Child.Value, () => wasCalled = true);

            target.Child = new NotifyingSampleObject() { Value = "bar" };

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void WhenIntermediatePropertyChangesButNoChangeToObservedProperty_ThenCallbackIsNotInvoked()
        {
            var wasCalled = false;
            var target = new NotifyingSampleObject() { Child = new NotifyingSampleObject() { Value = "foo" } };
            new PropertyChangedHelper().BuildListener(target, x => x.Child.Value, () => wasCalled = true);

            target.Child = new NotifyingSampleObject() { Value = "foo" };

            Assert.IsFalse(wasCalled);
        }

        [TestMethod]
        public void WhenListenerIsDisposed_ThenCallbackIsNoLongerInvoked()
        {
            bool wasCalled = false;
            var target = new NotifyingSampleObject() { Child = new NotifyingSampleObject() { Value = "foo" } };

            var listener = new PropertyChangedHelper().BuildListener(target, x => x.Child.Value, () => wasCalled = true);
            listener.Dispose();

            target.Child.Value = "bar";

            Assert.IsFalse(wasCalled);
        }

    }

    [TestClass]
    public class GivenListenerWithTypedCallback
    {
        private NotifyingSampleObject _target;
        private IDisposable _listener;
        private Action<string, string> _assertion;

        public GivenListenerWithTypedCallback()
        {
            _target = new NotifyingSampleObject() { Child = new NotifyingSampleObject() { Value = "foo" } };
            _listener = new PropertyChangedHelper().BuildListener(_target, x => x.Child.Value, (o, n) => {
                _assertion.Invoke(o, n);
            });
        }


        [TestMethod]
        public void WhenObservedPropertyChanges_ThenCallbackReceivesProperOldValue()
        {
            _assertion = (o, n) => Assert.AreEqual(o, "foo");

            _target.Child.Value = "bar";
        }

        [TestMethod]
        public void WhenObservedPropertyChanges_ThenCallbackReceivesProperNewValue()
        {
            _assertion = (o, n) => Assert.AreEqual(n, "bar");

            _target.Child.Value = "bar";
        }

        [TestMethod]
        public void WhenIntermediatePropertyChanges_ThenCallbackReceivesProperOldValue()
        {
            _assertion = (o, n) => Assert.AreEqual(o, "foo");

            _target.Child = new NotifyingSampleObject() { Value = "bar" };
        }

        [TestMethod]
        public void WhenIntermediatePropertyChanges_ThenCallbackReceivesProperNewValue()
        {
            _assertion = (o, n) => Assert.AreEqual(n, "bar");
            
            _target.Child = new NotifyingSampleObject() { Value = "bar" };
        }
    }
}
