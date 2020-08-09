using SkiaSharp;
using SkiEngine.UI.Gestures;
using SkiEngine.UI.Layouts;

namespace SkiEngine.UI.Views
{
    public class SkiEntry : SkiScrollView
    {
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
                    UiComponent.StartEntry(
                        Label.Text, 
                        textChanged: newText => Label.Text = newText
                    );
                }
            };
        }

        public SkiLabel Label { get; }

        private void OnTapped()
        {
            IsFocused = true;
        }

        public LinkedProperty<SKColor> BackgroundColorProp { get; }
        public SKColor BackgroundColor
        {
            get => BackgroundColorProp.Value;
            set => BackgroundColorProp.Value = value;
        }
    }
}
