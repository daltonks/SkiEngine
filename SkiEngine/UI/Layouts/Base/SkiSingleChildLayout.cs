﻿using System.Collections.Generic;
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

                InvalidateLayout();
            }
        }

        protected override void OnNodeChanged()
        {
            UpdateChildNode(Content);
        }

        protected abstract void OnContentSizeChanged(object sender, SKSize oldSize, SKSize newSize);
        protected abstract void OnContentHorizontalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue);
        protected abstract void OnContentVerticalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue);

        protected virtual bool UpdateChildPoint() => UpdateChildPoint(new SKPoint());

        protected bool UpdateChildPoint(SKPoint offset) =>
            UpdateChildPoint(
                Content,
                SKRect.Create(
                    offset.X + Padding.Left,
                    offset.Y + Padding.Top,
                    Size.Width - Padding.Left - Padding.Right,
                    Size.Height - Padding.Top - Padding.Bottom
                )
            );
    }
}
