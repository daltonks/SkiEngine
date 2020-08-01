using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.NCS.Component.Camera;

namespace SkiEngine.UI
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

        public override void Draw(SKCanvas canvas, CameraComponent camera)
        {
            canvas.Save();
            canvas.ClipRect(LocalBounds);
            Content.Draw(canvas, camera);
            canvas.Restore();
        }
    }
}
