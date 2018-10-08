using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.NCS;
using SkiEngine.NCS.Component;
using SkiEngine.Sprite;
using Xamarin.Forms;

namespace SkiEngine.TestApp
{
    public partial class MainPage : ContentPage
    {
        private static readonly SKImage tilesetImage;

        static MainPage()
        {
            // Get tilesetImage
            var assembly = typeof(MainPage).GetTypeInfo().Assembly;
            SKBitmap bitmap;
            using (var stream = assembly.GetManifestResourceStream("SkiEngine.TestApp.PrototypeMap.png"))
            {
                bitmap = SKBitmap.Decode(stream);
            }

            tilesetImage = SKImage.FromBitmap(bitmap);
        }

        private readonly Scene _scene = new Scene();

        public MainPage()
        {
            InitializeComponent();

            var cameraNode = _scene.RootNode.CreateChild();
            var cameraComponent = new CameraComponent(0, new[]{ 0 });
            cameraNode.AddComponent(cameraComponent);

            var spriteNode = _scene.RootNode.CreateChild();
            var spriteComponent = new SpriteComponent(tilesetImage, new SpriteData(new SKRectI(0, 0, 32, 32)));
            spriteNode.AddComponent(spriteComponent);
            cameraComponent.AddDrawable(spriteComponent, 0);
        }

        private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        {
            _scene.Update();
            _scene.Draw(args.Surface.Canvas, 0);
        }
    }
}
