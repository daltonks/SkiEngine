using System;
using System.Drawing;
using SkiaSharp;
using Color = Xamarin.Forms.Color;

namespace SkiEngine.Xamarin.ColorPicker
{
    // ReSharper disable once InconsistentNaming
    internal static class SKColorUtil
    {
        /// <summary>
        /// SKColor.FromHsv(...) does not properly invert SKColor.ToHsv(...),
        /// so I have this bad boy for now.
        /// https://github.com/mono/SkiaSharp/issues/912
        /// </summary>
        public static SKColor FromHsv(float h, float s, float v, byte a = 255)
        {
            // convert from percentages
            h = h / 360f;
            s = s / 100f;
            v = v / 100f;

            // RGB results from 0 to 255
            var r = v;
            var g = v;
            var b = v;

            // HSL from 0 to 1
            if (Math.Abs (s) > 0.001f)
            {
                h = h * 6f;
                if (Math.Abs (h - 6f) < 0.001f)
                    h = 0f; // H must be < 1

                var hInt = (int)h;
                var v1 = v * (1f - s);
                var v2 = v * (1f - s * (h - hInt));
                var v3 = v * (1f - s * (1f - (h - hInt)));

                if (hInt == 0)
                {
                    r = v;
                    g = v3;
                    b = v1;
                }
                else if (hInt == 1)
                {
                    r = v2;
                    g = v;
                    b = v1;
                }
                else if (hInt == 2)
                {
                    r = v1;
                    g = v;
                    b = v3;
                }
                else if (hInt == 3)
                {
                    r = v1;
                    g = v2;
                    b = v;
                }
                else if (hInt == 4)
                {
                    r = v3;
                    g = v1;
                    b = v;
                }
                else
                {
                    r = v;
                    g = v1;
                    b = v2;
                }
            }

            // RGB results from 0 to 255
            r = r * 255f;
            g = g * 255f;
            b = b * 255f;

            return new SKColor ((byte)Math.Round(r), (byte)Math.Round(g), (byte)Math.Round(b), a);
        }

        public static SKColor ToSKColor(this Color color)
        {
            return new SKColor(
                (byte) Math.Round(color.R * byte.MaxValue), 
                (byte) Math.Round(color.G * byte.MaxValue), 
                (byte) Math.Round(color.B * byte.MaxValue), 
                (byte) Math.Round(color.A * byte.MaxValue)
            );
        }
    }
}
