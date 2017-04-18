using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyChangedHelper
{
    public delegate void PropertyChangedHandler<T>(T oldValue, T newValue);
}
