using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Input;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiCanvasView : SKCanvasView
    {
        public delegate void MouseWheelScrollDelegate(int delta, SKPoint point);

        public event Action<SkiTouch> SkiTouch;

        private SkiInputService _inputService;

        public SkiCanvasView()
        {
            InitializeComponent();
        }
        
        public void InitInput(SkiInputService inputService)
        {
            _inputService = inputService;
        }

        private void OnTouch(object sender, SKTouchEventArgs args)
        {
            var skiTouch = args.ToSKTouch();
            SkiTouch?.Invoke(skiTouch);

            args.Handled = true;
        }
    }
}