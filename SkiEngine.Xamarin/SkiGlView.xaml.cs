using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiGlView : SKGLView
    {
        public delegate void MouseWheelScrollDelegate(int delta, SKPoint point);

        public event Action<SkiTouch> SkiTouch;
        public event MouseWheelScrollDelegate MouseWheelScroll;

        public SkiGlView()
        {
            Focused += OnFocused;
            Unfocused += OnUnfocused;

            InitializeComponent();
        }
        
        private void OnFocused(object sender, FocusEventArgs e)
        {
            InputService.Current.MouseWheelScroll += OnMouseWheelScroll;
        }

        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            InputService.Current.MouseWheelScroll -= OnMouseWheelScroll;
        }

        private SKPoint _previousTouchPoint;
        private void OnTouch(object sender, SKTouchEventArgs args)
        {
            var skiTouch = args.ToSKTouch();
            SkiTouch?.Invoke(skiTouch);

            args.Handled = true;

            _previousTouchPoint = args.Location;
        }

        private void OnMouseWheelScroll(int delta)
        {
            MouseWheelScroll?.Invoke(delta, _previousTouchPoint);
        }
    }
}