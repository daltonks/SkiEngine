using System;
using SkiaSharp.Views.Forms;
using SkiEngine.UI;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiUiView : SKCanvasView
    {
        private readonly SkiUiScene _skiUiScene;

        public SkiUiView()
        {
            InitializeComponent();
            
            _skiUiScene = new SkiUiScene(
                () => MainThread.BeginInvokeOnMainThread(InvalidateSurface),
                skiAnimation => new Animation(
                    skiAnimation.Callback, 
                    skiAnimation.Start, 
                    skiAnimation.End,
                    Easing.SinOut,
                    skiAnimation.Finished
                ).Commit(
                    this, 
                    Guid.NewGuid().ToString(), 
                    length: (uint) skiAnimation.Length.Milliseconds
                )
            );
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            _skiUiScene.UiComponent.View = (SkiView) BindingContext;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            _skiUiScene.OnPaintSurface(e.Surface.Canvas, Width, Height);

            base.OnPaintSurface(e);
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            e.Handled = true;

            _skiUiScene.OnTouch(e.ToSkiTouch());

            base.OnTouch(e);
        }
    }
}