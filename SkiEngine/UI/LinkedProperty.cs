using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SkiEngine.UI
{
    public class LinkedProperty<T>
    {
        public event Action<T, T> ValueChanged;

        private readonly Func<T, T, T> _valueChanging;
        private readonly List<WeakReference<LinkedProperty<T>>> _links = new List<WeakReference<LinkedProperty<T>>>();

        public LinkedProperty(T startingValue = default, Func<T, T, T> valueChanging = null, Action<T, T> valueChanged = null)
        {
            _valueChanging = valueChanging;
            if (valueChanged != null)
            {
                ValueChanged += valueChanged;
            }

            Value = startingValue;
        }

        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (_valueChanging != null)
                {
                    value = _valueChanging(_value, value);
                }

                if (EqualityComparer<T>.Default.Equals(_value, value))
                {
                    return;
                }

                var previousValue = _value;
                _value = value;

                lock (_links)
                {
                    for (var i = 0; i < _links.Count; i++)
                    {
                        var weakLink = _links[i];
                        if (weakLink.TryGetTarget(out var link))
                        {
                            link.Value = value;
                        }
                        else
                        {
                            _links.RemoveAt(i);
                            i--;
                        }
                    }
                }

                ValueChanged?.Invoke(previousValue, _value);
            }
        }

        public void SetAndLink(LinkedProperty<T> otherProperty)
        {
            lock (_links)
            {
                _links.Add(new WeakReference<LinkedProperty<T>>(otherProperty));
            }
            lock (otherProperty._links)
            {
                otherProperty._links.Add(new WeakReference<LinkedProperty<T>>(this));
            }
            otherProperty.Value = Value;
        }

        public void RaiseChanged()
        {
            lock (_links)
            {
                for (var i = 0; i < _links.Count; i++)
                {
                    var weakLink = _links[i];
                    if (weakLink.TryGetTarget(out var link))
                    {
                        link.RaiseChanged();
                    }
                    else
                    {
                        _links.RemoveAt(i);
                        i--;
                    }
                }
            }

            ValueChanged?.Invoke(Value, _value);
        }
    }
}
