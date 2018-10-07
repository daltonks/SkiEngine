using SkiaSharp.Views.Forms;
using SkiEngine.NCS;
using Xamarin.Forms;

namespace SkiEngine.TestApp
{
    public partial class MainPage : ContentPage
    {
        private readonly Scene _scene = new Scene();

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        {
            _scene.UpdateAndDraw(args.Surface.Canvas);
        }
    }
}
