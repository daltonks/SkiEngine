using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts.Base
{
    public abstract class SkiSingleChildLayout : SkiLayout
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
                    _content.HorizontalOptionsProp.ValueChanged -= OnContentHorizontalOptionsChanged;
                    _content.VerticalOptionsProp.ValueChanged -= OnContentVerticalOptionsChanged;
                }
                
                UpdateChildNode(value);
                _content = value;

                _content.SizeProp.ValueChanged += OnContentSizeChanged;
                _content.HorizontalOptionsProp.ValueChanged += OnContentHorizontalOptionsChanged;
                _content.VerticalOptionsProp.ValueChanged += OnContentVerticalOptionsChanged;

                QueueLayout();
            }
        }

        protected abstract void OnContentSizeChanged(object sender, SKSize oldSize, SKSize newSize);
        protected abstract void OnContentHorizontalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue);
        protected abstract void OnContentVerticalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue);

        protected override void OnNodeChanged()
        {
            UpdateChildNode(Content);
        }
    }
}
