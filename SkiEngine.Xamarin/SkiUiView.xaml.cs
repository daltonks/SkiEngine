using SkiaSharp.Views.Forms;
using SkiEngine.UI;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiUiView : SKCanvasView
    {
        private SkiUiScene _skiUiScene;

        public SkiUiView()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            _skiUiScene = new SkiUiScene(
                (SkiView) BindingContext, 
                () => Device.BeginInvokeOnMainThread(InvalidateSurface)
            );
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