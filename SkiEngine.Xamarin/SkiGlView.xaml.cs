using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Input;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiGlView : SKCanvasView
    {
        public delegate void MouseWheelScrollDelegate(int delta, SKPoint point);

        public event Action<SkiTouch> SkiTouch;
        public event MouseWheelScrollDelegate MouseWheelScroll;

        private InputService _inputService;

        public SkiGlView()
        {
            InitializeComponent();
        }
        
        public void InitInput(InputService inputService)
        {
            _inputService = inputService;
            inputService.MouseWheelScroll += OnMouseWheelScroll;
        }

        public void DisposeInput()
        {
            _inputService.MouseWheelScroll -= OnMouseWheelScroll;
        }

        private SKPoint _previousTouchPoint;
        private void OnTouch(object sender, SKTouchEventArgs args)
        {
            var skiTouch = args.ToSKTouch();
            SkiTouch?.Invoke(skiTouch);

            args.Handled = true;

            _previousTouchPoint = args.Location;
        }

        private void OnMouseWheelScroll(int scrollDelta)
        {
            MouseWheelScroll?.Invoke(scrollDelta, _previousTouchPoint);
        }
    }
}