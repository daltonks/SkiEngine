using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts.Base
{
    public abstract class SkiSingleChildLayout : SkiView
    {
        public override IEnumerable<SkiView> ChildrenEnumerable
        {
            get { yield return Content; }
        }

        private SkiView _content;
        public SkiView Content
        {
            get => _content;
            set
            {
                if (_content != null)
                {
                    _content.Node.Destroy();
                    _content.SizeProp.ValueChanged -= OnContentSizeChanged;
                }
                
                UpdateChildNode(value);
                _content = value;
                _content.SizeProp.ValueChanged += OnContentSizeChanged;
            }
        }

        protected abstract void OnContentSizeChanged(object sender, SKSize oldSize, SKSize newSize);

        protected override void OnNodeChanged()
        {
            UpdateChildNode(Content);
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            Size = new SKSize(maxWidth, maxHeight);
            LayoutInternal();
        }

        protected abstract void LayoutInternal();
    }
}
