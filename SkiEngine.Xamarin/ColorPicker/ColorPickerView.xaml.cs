using System;
using System.ComponentModel;
using SkiaSharp;
using SkiaSharp.Views.Forms;
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
            BindingContextChanged += OnBindingContextChanged;
        }

        public ColorPickerViewModel ViewModel
        {
            get => BindingContext as ColorPickerViewModel;
            set => BindingContext = value;
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ColorPickerViewModel.H) 
               || e.PropertyName == nameof(ColorPickerViewModel.S) 
               || e.PropertyName == nameof(ColorPickerViewModel.V))
            {
                SelectedSaturationValueCanvasView.InvalidateSurface();
            }

            if (e.PropertyName == nameof(ColorPickerViewModel.H))
            {
                SelectedHueRainbowCanvasView.InvalidateSurface();
            }
        }

        private void OnHexUnfocused(object sender, FocusEventArgs e)
        {
            ViewModel.Hex = ViewModel.Color.ToArgbHex();
        }

        private void OnSaturationValueGradientsPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

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

        private SKSizeI _selectedSaturationValuePixelSize;
        private void OnSelectedSaturationValuePaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            _selectedSaturationValuePixelSize = info.Size;

            canvas.Clear(SKColors.Transparent);

            var center = new SKPoint(
                ViewModel.S / 100 * info.Width, 
                info.Height - ViewModel.V / 100 * info.Height
            );

            var pixelsPerXamarinUnit = info.Width / SelectedSaturationValueCanvasView.Width;

            var innerRadius = (float) (10 * pixelsPerXamarinUnit);
            var strokeWidth = (float) (1.5f * pixelsPerXamarinUnit);

            using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = strokeWidth })
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = SKColorUtil.FromHsv(ViewModel.H, ViewModel.S, ViewModel.V);
                canvas.DrawCircle(center, innerRadius, paint);

                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.White;
                canvas.DrawCircle(center, innerRadius, paint);

                paint.Color = SKColors.Black;
                canvas.DrawCircle(center, innerRadius + strokeWidth, paint);
            }

            canvas.Flush();
        }

        private void OnSelectedSaturationValueTouch(object sender, SKTouchEventArgs e)
        {
            e.Handled = true;

            if (!e.InContact)
            {
                return;
            }

            var x = e.Location.X;
            var y = e.Location.Y;

            if (x < 0)
            {
                x = 0;
            }
            else if (x > _selectedSaturationValuePixelSize.Width)
            {
                x = _selectedSaturationValuePixelSize.Width;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y > _selectedSaturationValuePixelSize.Height)
            {
                y = _selectedSaturationValuePixelSize.Height;
            }

            var saturation = x / _selectedSaturationValuePixelSize.Width * 100;
            var value = 100 - y / _selectedSaturationValuePixelSize.Height * 100;

            ViewModel.S = saturation;
            ViewModel.V = value;
        }

        private void OnHueRainbowPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            using (var paint = new SKPaint { Style = SKPaintStyle.Fill })
            {
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

        private SKSizeI _selectedHueRainbowPixelSize;
        private void OnSelectedHueRainbowPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            _selectedHueRainbowPixelSize = info.Size;

            canvas.Clear(SKColors.Transparent);

            var center = new SKPoint(
                ViewModel.H / 360 * info.Width, 
                info.Height / 2f
            );

            var pixelsPerXamarinUnit = info.Width / SelectedHueRainbowCanvasView.Width;

            var strokeWidth = (float) (1.5 * pixelsPerXamarinUnit);
            var innerRadius = info.Height / 2f - 2 * strokeWidth;

            using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = strokeWidth })
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = SKColorUtil.FromHsv(ViewModel.H, 100, 100);
                canvas.DrawCircle(center, innerRadius, paint);

                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.White;
                canvas.DrawCircle(center, innerRadius, paint);

                paint.Color = SKColors.Black;
                canvas.DrawCircle(center, innerRadius + strokeWidth, paint);
            }

            canvas.Flush();
        }

        private void OnSelectedHueRainbowTouch(object sender, SKTouchEventArgs e)
        {
            e.Handled = true;

            if (!e.InContact)
            {
                return;
            }

            var x = e.Location.X;
            var width = _selectedHueRainbowPixelSize.Width;
            if (x < 0)
            {
                x = 0;
            }
            else if (x > width)
            {
                x = width;
            }

            ViewModel.H = x / width * 360;
        }
    }
}