using System;
using SkiaSharp;
using SkiEngine.UI.Gestures;
using SkiEngine.UI.Layouts;
using WeakEvent;

namespace SkiEngine.UI.Views
{
    public class SkiEntry : SkiScrollView
    {
        private readonly WeakEventSource<bool> _completedEventSource = new WeakEventSource<bool>();
        public event EventHandler<bool> Completed
        {
            add => _completedEventSource.Subscribe(value);
            remove => _completedEventSource.Unsubscribe(value);
        }

        public SkiEntry()
        {
            CanScrollHorizontally = true;
            CanScrollVertically = false;
            HeightRequest = 40;

            Content = Label = new SkiLabel();

            GestureRecognizers.Insert(0, new TapGestureRecognizer(this, OnTapped));

            IsFocusedProp.ValueChanged += (sender, oldValue, newValue) =>
            {
                if (newValue)
                {
                    UiComponent.ShowNativeEntry(this);
                }
                else
                {
                    OnUnfocused();
                }

                InvalidateSurface();
            };
        }

        public SkiLabel Label { get; }

        public void OnNativeEntryCompleted()
        {
            _completedEventSource.Raise(this, true);
        }

        private void OnUnfocused()
        {
            UiComponent.HideNativeEntry();
        }

        private void OnTapped()
        {
            IsFocused = true;
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            // Only draw when not focused
            if (!IsFocused)
            {
                base.DrawInternal(canvas);
            }
        }

        protected override void DrawContent(SKCanvas canvas)
        {
            using (
                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke, 
                    IsAntialias = true,
                    Color = 0xFFCCCCCC,
                    StrokeWidth = 2
                }
            )
            {
                canvas.DrawRoundRect(BoundsLocal, 6, 6, paint);
            }

            base.DrawContent(canvas);
        }
    }
}
