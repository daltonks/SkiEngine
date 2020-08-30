using System;
using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Views
{
    public class SkiImageView : SkiView
    {
        protected override void OnNodeChanged()
        {
            
        }

        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            Size = new SKSize(maxWidth ?? 400, maxHeight ?? 400);
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            
        }
    }
}
