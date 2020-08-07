using System;
using SkiaSharp.Views.Forms;
using SkiEngine.UI;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiUiView : AbsoluteLayout
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
                ? () => Application.Current.Dispatcher.BeginInvokeOnMainThread(CanvasView.InvalidateSurface)
                : (Action) CanvasView.InvalidateSurface;

            _skiUiScene = new SkiUiScene(
                invalidateSurface, 
                (node, camera, invalidate) => new SkiXamarinUiComponent(CanvasView, HiddenEntry, node, camera, invalidate)
            );
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            _skiUiScene.UiComponent.View = (SkiView) BindingContext;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            _skiUiScene.OnPaintSurface(e.Surface.Canvas, Width, Height);
        }

        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            e.Handled = true;

            _skiUiScene.OnTouch(e.ToSkiTouch());
        }
    }
}