using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

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
                UpdateChildNode(view, new InitialNodeTransform(new SKPoint(0, Size.Height)));

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Size.Width != 0)
                {
                    view.Layout(Size.Width, float.MaxValue);
                    Size = new SKSize(
                        Math.Max(view.Size.Width, Size.Width), 
                        Size.Height + view.Size.Height
                    );
                }
            }
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (maxWidth == Size.Width)
            {
                return;
            }

            var height = 0f;
            foreach (var child in Children)
            {
                child.Node.RelativePoint = new SKPoint(0, height);
                child.Layout(maxWidth, float.MaxValue);
                height += child.Size.Height;
            }
            Size = new SKSize(maxWidth, height);
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            foreach (var view in Children)
            {
                view.Draw(canvas);
            }
        }
    }
}
