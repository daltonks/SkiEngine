using System.Collections.Generic;
using SkiaSharp;

namespace SkiEngine.UI.Layouts
{
    public class SkiScrollView : SkiView
    {
        private SkiView _content;
        public SkiView Content
        {
            get => _content;
            set
            {
                value.Initialize(UiComponent, Node);
                _content = value;
            }
        }

        public override IEnumerable<SkiView> Children
        {
            get { yield return Content; }
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            LocalBounds = new SKRect(0, 0, maxWidth, maxHeight);
            Content.Layout(maxWidth, maxHeight);
        }

        public override void Draw(SKCanvas canvas)
        {
            canvas.Save();
            canvas.ClipRect(LocalBounds);
            Content.Draw(canvas);
            canvas.Restore();
        }
    }
}
