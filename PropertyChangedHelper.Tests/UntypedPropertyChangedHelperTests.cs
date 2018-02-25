using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PropertyChangedHelper.Tests
{
    [TestClass]
    public class UntypedPropertyChangedHelperTests
    {
        public class SampleObject : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public SampleObject2 Child { get { return _child; } set { _child = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child))); } }
            private SampleObject2 _child;
            public NotNotifyingObject NotNotifying { get { return _notNotifying; } set { _notNotifying = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotNotifying))); } }
            private NotNotifyingObject _notNotifying;

            public SampleObject2 Field;
        }

        public class SampleObject2 : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string SampleString { get { return _sampleString; } set { _sampleString = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SampleString))); } }
            private string _sampleString;
        }

        public class NotNotifyingObject
        {
            public string SampleString { get; set; }
        }

        #region property chain building

        [TestMethod]
        public void CanBuildListener()
        {
            var obj = new SampleObject() { Child = new SampleObject2() { SampleString = "foo" } };
            var listener = new PropertyChangedHelper().BuildListener(obj, x => x.Child.SampleString, () => { });
        }

        [TestMethod]
        public void WhenNotINotifyPropertyChangedInSelectorExpression_ShouldThrowNotSupportedExceptionOnBuilding()
        {
            Assert.ThrowsException<NotSupportedException>(() =>
            {
                var obj = new SampleObject() { NotNotifying = new NotNotifyingObject() { SampleString = "foo" } };
                var listener = new PropertyChangedHelper().BuildListener(obj, x => x.NotNotifying.SampleString, () => { });
            });
        }

        [TestMethod]
        public void WhenFieldInSelectorExpression_ShouldThrowNotSupportedExceptionOnBuilding()
        {
            Assert.ThrowsException<NotSupportedException>(() =>
            {
                var obj = new SampleObject() { Field = new SampleObject2() };
                var listener = new PropertyChangedHelper().BuildListener(obj, x => x.Field, () => { });
            });
        }

        #endregion

        #region invoking callback on property change

        [TestMethod]
        public void WhenPropertyChanged_ShouldInvokeCallback()
        {
            bool wasCalled = false;
            var obj = new SampleObject() { Child = new SampleObject2() { SampleString = "foo" } };
            var listener = new PropertyChangedHelper().BuildListener(obj, x => x.Child.SampleString, () => { wasCalled = true; });
            obj.Child.SampleString = "bar";
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void WhenNoChange_ShouldNotInvokeCallback()
        {
            bool wasCalled = false;
            var obj = new SampleObject() { Child = new SampleObject2() { SampleString = "foo" } };
            var listener = new PropertyChangedHelper().BuildListener(obj, x => x.Child.SampleString, () => { wasCalled = true; });
            Assert.IsFalse(wasCalled);
        }

        [TestMethod]
        public void WhenIntermediatePropertyChanged_ShouldInvokeCallback()
        {
            var wasCalled = false;
            var obj = new SampleObject() { Child = new SampleObject2() { SampleString = "foo" } };
            new PropertyChangedHelper().BuildListener(obj, x => x.Child.SampleString, () => { wasCalled = true; });

            obj.Child = new SampleObject2() { SampleString = "bar" };

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void WhenListenerDisposed_ShouldNotInvokeCallback()
        {
            bool wasCalled = false;
            var obj = new SampleObject() { Child = new SampleObject2() { SampleString = "foo" } };
            
            using (var listener = new PropertyChangedHelper().BuildListener(obj, x => x.Child.SampleString, () => {
                wasCalled = true;
            }))
            {
                // do nothing
            }
            
            obj.Child.SampleString = "bar";
            Assert.IsFalse(wasCalled);
        }

        #endregion
    }
}
