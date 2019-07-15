using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin.ColorPicker
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorPickerView : ContentView
    {
        public ColorPickerView()
        {
            InitializeComponent();
        }

        private void OnSaturationValuePaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            // Hue
            canvas.Clear(SKColors.Red);

            using (var saturationPaint = new SKPaint())
            {
                using (
                    saturationPaint.Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(info.Width, 0),
                        new[] {SKColors.White, SKColors.Transparent},
                        null,
                        SKShaderTileMode.Clamp
                    )
                )
                {
                    canvas.DrawRect(0, 0, info.Width, info.Height, saturationPaint);
                }
            }
            
            using (var valuePaint = new SKPaint())
            {
                using (
                    valuePaint.Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(0, info.Height),
                        new[] {SKColors.Empty, SKColors.Black},
                        null,
                        SKShaderTileMode.Clamp
                    )
                )
                {
                    canvas.DrawRect(0, 0, info.Width, info.Height, valuePaint);
                }
            }
            
            canvas.Flush();
        }

        private void OnSaturationValueTouch(object sender, SKTouchEventArgs e)
        {


            e.Handled = true;
        }

        private void OnHuePaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();

            using (var paint = new SKPaint { Style = SKPaintStyle.Fill })
            {
                // Define an array of rainbow colors
                var colors = new SKColor[7];

                for (var i = 0; i < colors.Length; i++)
                {
                    colors[i] = SKColor.FromHsl(i * 360f / (colors.Length - 1), 100, 50);
                }

                paint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(info.Width, 0),
                    colors,
                    null, 
                    SKShaderTileMode.Clamp
                );

                canvas.DrawRect(0, 0, info.Width, info.Height, paint);
            }

            canvas.Flush();
        }

        private void OnHueTouch(object sender, SKTouchEventArgs e)
        {

            e.Handled = true;
        }
    }
}