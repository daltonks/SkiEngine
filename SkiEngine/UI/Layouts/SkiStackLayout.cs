using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.Input;

namespace SkiEngine.UI.Layouts
{
    public class SkiStackLayout : SkiView
    {
        private readonly List<SkiView> _children = new List<SkiView>();
        public override IEnumerable<SkiView> Children => _children;

        public override bool ListensForPressedTouches => false;

        protected override void OnNodeChanged()
        {
            var copiedChildren = _children.ToList();
            _children.Clear();
            foreach (var child in copiedChildren)
            {
                Add(child);
            }
        }

        public void Add(SkiView view)
        {
            _children.Add(view);

            if (Node != null)
            {
                CreateChildNode(view, new InitialNodeTransform(new SKPoint(0, LocalBounds.Height)));

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
