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
        private TextBlock _textBlock = new TextBlock();

        public SkiLabel()
        {
            TextProp = new LinkedProperty<string>(
                this, 
                "",
                valueChanged: (sender, oldValue, newValue) => UpdateRichString()
            );
            FontSizeProp = new LinkedProperty<float>(
                this, 
                16,
                valueChanged: (sender, oldValue, newValue) => UpdateRichString()
            );
            CursorPositionProp = new LinkedProperty<int?>(
                this,
                valueChanged: (sender, oldValue, newValue) => InvalidateSurface()
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

        public LinkedProperty<int?> CursorPositionProp { get; }
        public int? CursorPosition
        {
            get => CursorPositionProp.Value;
            set => CursorPositionProp.Value = value;
        }

        public override IEnumerable<SkiView> ChildrenEnumerable => Enumerable.Empty<SkiView>();

        protected override void OnNodeChanged() { }

        private void UpdateRichString()
        {
            _textBlock = new TextBlock
            {
                MaxWidth = _textBlock.MaxWidth,
                MaxHeight = _textBlock.MaxHeight,
            };
            _textBlock.AddText(Text, StyleManager.Default.DefaultStyle);

            OnSizeChanged();

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

            OnSizeChanged();
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private void OnSizeChanged()
        {
            var width = _textBlock.MeasuredWidth;
            var height = _textBlock.MeasuredHeight;
            ViewPreferredWidth = width;
            ViewPreferredHeight = height;
            Size = new SKSize(width, height);
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            _textBlock.Paint(canvas);
            if (CursorPosition != null)
            {
                var cursorRectangle = _textBlock.GetCaretInfo(CursorPosition.Value).CaretRectangle;
                using (var paint = new SKPaint { Color = SKColors.Black })
                {
                    canvas.DrawRect(cursorRectangle.Left, cursorRectangle.Top, 1, cursorRectangle.Height, paint);
                }
            }
        }
    }
}
