using SkiaSharp;
using SkiEngine.UI.Layouts;

namespace SkiEngine.UI.Views
{
    public class SkiEntry : SkiScrollView
    {
        public SkiEntry()
        {
            CanScrollHorizontally = true;
            CanScrollVertically = false;

            Content = Label = new SkiLabel();
        }

        public SkiLabel Label { get; }

        public LinkedProperty<SKColor> BackgroundColorProp { get; }
        public SKColor BackgroundColor
        {
            get => BackgroundColorProp.Value;
            set => BackgroundColorProp.Value = value;
        }
    }
}
