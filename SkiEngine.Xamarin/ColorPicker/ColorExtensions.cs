using System;
using Xamarin.Forms;

namespace SkiEngine.Xamarin.ColorPicker
{
    public static class ColorExtensions
    {
        public static string ToArgbHex(this Color color)
        {
            var a = (byte)(color.A * byte.MaxValue);
            var r = (byte)(color.R * byte.MaxValue);
            var g = (byte)(color.G * byte.MaxValue);
            var b = (byte)(color.B * byte.MaxValue);

            return a == byte.MaxValue 
                ? $"{r:X2}{g:X2}{b:X2}" 
                : $"{a:X2}{r:X2}{g:X2}{b:X2}";
        }
    }
}
