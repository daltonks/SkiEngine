using System;
using SkiaSharp.Views.Forms;
using SkiEngine.Input;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiCanvasView : SKCanvasView
    {
        public event Action<SkiTouch> SkiTouch;

        public SkiCanvasView()
        {
            InitializeComponent();
        }

        private void OnTouch(object sender, SKTouchEventArgs args)
        {
            var skiTouch = args.ToSKTouch();
            SkiTouch?.Invoke(skiTouch);

            args.Handled = true;
        }
    }
}