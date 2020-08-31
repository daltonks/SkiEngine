using System;
using System.Threading.Tasks;
using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Views.ImageSources
{
    public class ImageSource : IDisposable
    {
        private readonly CachedResourceUsage<SKImage> _resourceUsage;

        public ImageSource(SkiView view, string path)
        {
            _resourceUsage = ResourceCache.GetAppPackageFile(
                path,
                stream => Task.Run(() => SKImage.FromEncodedData(stream))
            );

            Task.Run(async () => {
                await _resourceUsage.WaitForLoadingAsync();
                if (!_resourceUsage.IsDisposed)
                {
                    view.InvalidateSurface();
                }
            });
        }

        public void Draw(SKCanvas canvas)
        {
            var image = _resourceUsage?.Value;
            if (image == null)
            {
                return;
            }
            canvas.DrawImage(image, 0, 0);
        }

        public void Dispose()
        {
            _resourceUsage.Dispose();
        }
    }
}
