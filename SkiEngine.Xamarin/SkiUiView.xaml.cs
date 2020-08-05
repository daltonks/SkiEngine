using System;
using System.Threading;
using SkiaSharp.Views.Forms;
using SkiEngine.UI;
using Xamarin.Essentials;
using Xamarin.Forms;
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

            // UWP immediately redraws when calling InvalidateSurface,
            // which breaks logical flow if you make multiple changes
            // that invalidate the surface.
            // Because of this, use the dispatcher.
            var invalidateSurface = Device.RuntimePlatform == Device.UWP
                ? () => Application.Current.Dispatcher.BeginInvokeOnMainThread(InvalidateSurface)
                : (Action) InvalidateSurface;

            _skiUiScene = new SkiUiScene(
                invalidateSurface, 
                (node, camera, invalidate) => new SkiXamarinUiComponent(this, node, camera, invalidate)
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