using System.Threading.Tasks;
using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Views
{
    public class SkiImageView : SkiView
    {
        public SkiImageView(CachedResourceUsage<SKImage> imageUsage = null)
        {
            ImageUsageProp = new LinkedProperty<CachedResourceUsage<SKImage>>(
                this,
                valueChanged: async (sender, args) =>
                {
                    args.OldValue?.Dispose();

                    if (args.NewValue != null)
                    {
                        await args.NewValue.GetValueAsync();
                    }

                    InvalidateSurface();
                }
            );

            ImageUsage = imageUsage;
        }

        public LinkedProperty<CachedResourceUsage<SKImage>> ImageUsageProp { get; }
        public CachedResourceUsage<SKImage> ImageUsage
        {
            get => ImageUsageProp.Value;
            set => ImageUsageProp.Value = value;
        }

        protected override void OnNodeChanged()
        {
            
        }

        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            Size = new SKSize(maxWidth ?? 400, maxHeight ?? 400);
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            var image = ImageUsage?.Value;
            if (image != null)
            {
                using var paint = new SKPaint{ FilterQuality = SKFilterQuality.High };
                canvas.DrawImage(image, BoundsLocal, paint);
            }
        }
    }
}
