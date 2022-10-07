using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public class ObservableValue<T> : ViewModelBase, IDisposable
    {
        private T _value;
        public T Value
        {
            get => _value;
            set => Set(ref _value, value);
        }

        public ObservableValue() { }
        public ObservableValue(T _value)
        {
            Value = _value;
        }

        public static implicit operator T(ObservableValue<T> obj) => obj.Value;
        public static implicit operator ObservableValue<T>(T val) => new ObservableValue<T>(val);

        public void Dispose()
        {
            if (Value is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
