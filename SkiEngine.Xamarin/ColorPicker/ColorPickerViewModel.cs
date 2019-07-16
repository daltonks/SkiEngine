using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

// ReSharper disable CompareOfFloatsByEqualityOperator
namespace SkiEngine.Xamarin.ColorPicker
{
    public class ColorPickerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Color Color
        {
            get => SKColorUtil.FromHsv(H, S, V, A).ToFormsColor();
            set => Hex = value.ToArgbHex();
        }

        public Color HueColor => SKColorUtil.FromHsv(H, 100, 100).ToFormsColor();

        private string _hex = "000000";
        public string Hex
        {
            get => _hex;
            set
            {
                var stringBuilder = new StringBuilder();
                foreach (var c in value)
                {
                    if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                    {
                        stringBuilder.Append(c);
                    }
                }

                value = stringBuilder.ToString();

                if (SetProperty(ref _hex, value) && value.Length >= 3)
                {
                    var color = Color.FromHex(value);
                    if (color == Color.Default)
                    {
                        color = Color.White;
                    }

                    var skColor = color.ToSKColor();

                    if (_a != skColor.Alpha)
                    {
                        _a = skColor.Alpha;
                        RaisePropertyChanged(nameof(A));
                    }

                    skColor.ToHsv(out var h, out var s, out var v);

                    if (_h != h)
                    {
                        _h = h;
                        RaisePropertyChanged(nameof(H));
                        RaisePropertyChanged(nameof(HueColor));
                    }

                    if (_s != s)
                    {
                        _s = s;
                        RaisePropertyChanged(nameof(S));
                    }

                    if (_v != v)
                    {
                        _v = v;
                        RaisePropertyChanged(nameof(V));
                    }
                    
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        private byte _a = byte.MaxValue;
        public byte A
        {
            get => _a;
            set
            {
                if (_a != value)
                {
                    _a = value;
                    Hex = Color.ToArgbHex();
                }
            }
        }

        private float _h;
        public float H
        {
            get => _h;
            set
            {
                if (_h != value)
                {
                    _h = value;
                    Hex = Color.ToArgbHex();

                    RaisePropertyChanged(nameof(HueColor));
                }
            }
        }

        private float _s;
        public float S
        {
            get => _s;
            set
            {
                if (_s != value)
                {
                    _s = value;
                    Hex = Color.ToArgbHex();
                }
            }
        }

        private float _v;
        public float V
        {
            get => _v;
            set
            {
                if (_v != value)
                {
                    _v = value;
                    Hex = Color.ToArgbHex();
                }
            }
        }

        private bool SetProperty<T>(ref T obj, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(obj, value))
            {
                return false;
            }

            obj = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
