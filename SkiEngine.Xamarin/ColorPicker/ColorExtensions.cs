using System;
using Xamarin.Forms;

namespace SkiEngine.Xamarin.ColorPicker
{
    public static class ColorExtensions
    {
        public static string ToArgbHex(this Color color)
        {
            var a = (byte)(Math.Round(color.A * byte.MaxValue));
            var r = (byte)(Math.Round(color.R * byte.MaxValue));
            var g = (byte)(Math.Round(color.G * byte.MaxValue));
            var b = (byte)(Math.Round(color.B * byte.MaxValue));

            return a == byte.MaxValue 
                ? $"{r:X2}{g:X2}{b:X2}" 
                : $"{a:X2}{r:X2}{g:X2}{b:X2}";
        }
    }
}
