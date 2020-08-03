using SkiaSharp.Views.Forms;
using SkiEngine.UI;
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
                InvalidateSurface, 
                (node, camera, invalidateSurface) => new SkiXamarinUiComponent(this, node, camera, invalidateSurface)
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