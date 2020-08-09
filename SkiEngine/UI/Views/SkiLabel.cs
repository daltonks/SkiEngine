﻿using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.UI.Views.Base;
using Topten.RichTextKit;

namespace SkiEngine.UI.Views
{
    public class SkiLabel : SkiView
    {
        private RichString _richString = new RichString();

        public SkiLabel()
        {
            TextProp = new LinkedProperty<string>(
                this, 
                "",
                valueChanged: (sender, oldValue, newValue) => UpdateRichString()
            );
        }

        public LinkedProperty<string> TextProp { get; }
        public string Text
        {
            get => TextProp.Value;
            set => TextProp.Value = value;
        }

        public override IEnumerable<SkiView> ChildrenEnumerable => Enumerable.Empty<SkiView>();

        protected override void OnNodeChanged() { }

        private void UpdateRichString()
        {
            _richString = new RichString
            {
                MaxWidth = _richString.MaxWidth,
                MaxHeight = _richString.MaxHeight
            }.Add(Text);

            OnSizeChanged();

            InvalidateSurface();
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            _richString.MaxWidth = maxWidth;
            _richString.MaxHeight = maxHeight;

            OnSizeChanged();
        }

        private void OnSizeChanged()
        {
            var width = _richString.MeasuredWidth;
            var height = _richString.MeasuredHeight;
            ViewPreferredWidth = width;
            ViewPreferredHeight = height;
            Size = new SKSize(width, height);
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            _richString.Paint(canvas);
        }
    }
}