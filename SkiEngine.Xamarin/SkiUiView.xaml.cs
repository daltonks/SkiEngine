﻿using SkiaSharp.Views.Forms;
using SkiEngine.UI;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiView : SKCanvasView
    {
        private readonly SkiUiViewScene _skiUiViewScene;

        public SkiView()
        {
            InitializeComponent();

            _skiUiViewScene = new SkiUiViewScene(
                () => Device.BeginInvokeOnMainThread(InvalidateSurface)
            );
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            _skiUiViewScene.OnPaintSurface(e.Surface.Canvas, e.Info.Width, e.Info.Height, Width, Height);

            base.OnPaintSurface(e);
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            e.Handled = true;

            _skiUiViewScene.OnTouch(e.ToSkiTouch());

            base.OnTouch(e);
        }
    }
}