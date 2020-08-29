using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SkiaSharp;
using SkiEngine.UI.Views.Base;
using Topten.RichTextKit;

namespace SkiEngine.UI.Views
{
    public class SkiLabel : SkiView
    {
        private readonly TextBlock _textBlock = new TextBlock();

        public SkiLabel()
        {
            TextProp = new LinkedProperty<string>(
                this, 
                "",
                valueChanged: (sender, oldValue, newValue) => UpdateTextBlock()
            );
            FontSizeProp = new LinkedProperty<float>(
                this, 
                16,
                valueChanged: (sender, oldValue, newValue) => UpdateTextBlock()
            );
        }

        public LinkedProperty<string> TextProp { get; }
        public string Text
        {
            get => TextProp.Value;
            set => TextProp.Value = value;
        }

        public LinkedProperty<float> FontSizeProp { get; }
        public float FontSize
        {
            get => FontSizeProp.Value;
            set => FontSizeProp.Value = value;
        }

        public override IEnumerable<SkiView> ChildrenEnumerable => Enumerable.Empty<SkiView>();

        protected override void OnNodeChanged() { }

        private void UpdateTextBlock()
        {
            _textBlock.Clear();
            _textBlock.AddText(Text, UiComponent.DefaultTextStyle);

            UpdateSize();

            InvalidateSurface();
        }

        public int GetClosestCharacterIndex(SKPoint pointWorld)
        {
            var localPoint = Node.WorldToLocalMatrix.MapPoint(pointWorld);
            var codePointIndex = _textBlock.HitTest(localPoint.X, localPoint.Y).ClosestCodePointIndex;
            return _textBlock.CodePointToCharacterIndex(codePointIndex);
        }
        
        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            _textBlock.MaxWidth = maxWidth;
            _textBlock.MaxHeight = maxHeight;

            UpdateSize();
        }

        private void UpdateSize()
        {
            Size = new SKSize(_textBlock.MeasuredWidth, _textBlock.MeasuredHeight);
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            DrawBackground(canvas);
            _textBlock.Paint(canvas);
        }
    }
}
