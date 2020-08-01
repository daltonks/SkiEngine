using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Input;

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
                _content?.Node.Destroy();
                CreateChildNode(value);
                _content = value;
            }
        }

        public override IEnumerable<SkiView> Children
        {
            get { yield return Content; }
        }

        public override bool ListensForPressedTouches => true;

        protected override void OnNodeChanged()
        {
            CreateChildNode(Content);
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

        public override ViewTouchResult OnPressed(SkiTouch touch)
        {
            return ViewTouchResult.CancelLowerListeners;
        }

        public override ViewTouchResult OnMoved(SkiTouch touch)
        {
            return ViewTouchResult.CancelLowerListeners;
        }
    }
}
