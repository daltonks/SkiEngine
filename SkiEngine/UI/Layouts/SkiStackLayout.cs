using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SkiEngine.UI.Layouts
{
    public class SkiStackLayout : SkiView
    {
        private readonly List<SkiView> _children = new List<SkiView>();
        public override IEnumerable<SkiView> Children => _children;

        public void Add(SkiView view)
        {
            view.Initialize(UiComponent, Node.CreateChild(new SKPoint(0, LocalBounds.Height)));
            _children.Add(view);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (LocalBounds.Width != 0)
            {
                view.Layout(LocalBounds.Width, float.MaxValue);
                LocalBounds = new SKRect(
                    0, 
                    0, 
                    Math.Max(view.LocalBounds.Width, LocalBounds.Width), 
                    LocalBounds.Height + view.LocalBounds.Height
                );
            }
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (maxWidth == LocalBounds.Width)
            {
                return;
            }

            var width = maxWidth;
            var height = 0f;
            foreach (var child in Children)
            {
                child.Node.RelativePoint = new SKPoint(0, height);
                child.Layout(maxWidth, float.MaxValue);
                height += child.LocalBounds.Height;
            }
            LocalBounds = new SKRect(0, 0, width, height);
        }

        public override void Draw(SKCanvas canvas)
        {
            foreach (var view in Children)
            {
                view.Draw(canvas);
            }
        }
    }
}
