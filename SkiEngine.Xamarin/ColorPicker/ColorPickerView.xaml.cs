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

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
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
                        new float[] {0, 1},
                        SKShaderTileMode.Clamp
                    )
                )
                {
                    canvas.DrawRect(0, 0, info.Width, info.Height, saturationPaint);
                }
            }
            
            using (var valuePaint = new SKPaint { BlendMode = SKBlendMode.SrcOver })
            {
                using (
                    valuePaint.Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(0, info.Height),
                        new[] {SKColors.Empty, SKColors.Black},
                        new float[] {0, 1},
                        SKShaderTileMode.Clamp
                    )
                )
                {
                    canvas.DrawRect(0, 0, info.Width, info.Height, valuePaint);
                }
            }
        }

        private void OnTouch(SkiTouch obj)
        {
            
        }
    }
}