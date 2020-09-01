using System;
using System.Collections.Generic;
using System.Linq;
using WeakEvent;

namespace SkiEngine.UI
{
    public class LinkedProperty<T>
    {
        public delegate void ValueChangedDelegate(object sender, ValueChangedArgs<T> args);

        private readonly WeakEventSource<ValueChangedArgs<T>> _valueChangedEventSource = new WeakEventSource<ValueChangedArgs<T>>();
        public event EventHandler<ValueChangedArgs<T>> ValueChanged
        {
            add => _valueChangedEventSource.Subscribe(value);
            remove => _valueChangedEventSource.Unsubscribe(value);
        }

        private readonly object _owner;
        private readonly Func<T, T, T> _valueChanging;
        private readonly Func<T> _updateValue;
        private readonly List<WeakReference<LinkedProperty<T>>> _links = new List<WeakReference<LinkedProperty<T>>>();

        public LinkedProperty(
            object owner,
            T startingValue = default, 
            Func<T, T, T> valueChanging = null, 
            ValueChangedDelegate valueChanged = null,
            Func<T> updateValue = null
        )
        {
            _owner = owner;
            _valueChanging = valueChanging;
            _updateValue = updateValue;
            if (valueChanged != null)
            {
                ValueChanged += (sender, args) => valueChanged(sender, args);
            }

            _value = startingValue;
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

                _valueChangedEventSource.Raise(_owner, new ValueChangedArgs<T>(previousValue, _value));
            }
        }

        public void UpdateValue()
        {
            if (_updateValue != null)
            {
                Value = _updateValue();
            }
        }

        public void Link(LinkedProperty<T> otherProperty)
        {
            AddLinkInternal(otherProperty);
            otherProperty.AddLinkInternal(this);
            otherProperty.Value = Value;
        }

        private void AddLinkInternal(LinkedProperty<T> linkToAdd)
        {
            lock (_links)
            {
                _links.Add(new WeakReference<LinkedProperty<T>>(linkToAdd));
            }
        }

        public void Unlink(LinkedProperty<T> otherProperty)
        {
            RemoveLinkInternal(otherProperty);
            otherProperty.RemoveLinkInternal(this);
        }

        public void UnlinkAll()
        {
            lock (_links)
            {
                foreach (var weakLink in _links.ToList())
                {
                    if (weakLink.TryGetTarget(out var link))
                    {
                        link.RemoveLinkInternal(this);
                    }
                }

                _links.Clear();
            }
        }

        private void RemoveLinkInternal(LinkedProperty<T> linkToRemove)
        {
            lock (_links)
            {
                for (var i = 0; i < _links.Count; i++)
                {
                    var weakLink = _links[i];
                    if (weakLink.TryGetTarget(out var link))
                    {
                        if (link == linkToRemove)
                        {
                            _links.RemoveAt(i);
                            break;
                        }
                    }
                    else
                    {
                        _links.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public void RaiseValueChanged()
        {
            RaiseValueChanged(true);
        }

        private void RaiseValueChanged(bool forwardToLinks)
        {
            if (forwardToLinks)
            {
                lock (_links)
                {
                    for (var i = 0; i < _links.Count; i++)
                    {
                        var weakLink = _links[i];
                        if (weakLink.TryGetTarget(out var link))
                        {
                            link.RaiseValueChanged(false);
                        }
                        else
                        {
                            _links.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
            
            _valueChangedEventSource.Raise(this, new ValueChangedArgs<T>(Value, Value));
        }

        public override string ToString()
        {
            return Value?.ToString();
        }

        ~LinkedProperty()
        {
            UnlinkAll();
        }
    }

    public struct ValueChangedArgs<T>
    {
        public ValueChangedArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; }
        public T NewValue { get; }
    }
}
