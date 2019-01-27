using Windows.UI.Xaml.Controls;
using SkiEngine.Input;

namespace SkiEngine.UWP
{
    public static class SkiGlViewExtensions
    {
        public static void InitializeSkiEngine(this Page page)
        {
            page.PointerWheelChanged += (sender, args) =>
            {
                var delta = args.GetCurrentPoint(page).Properties.MouseWheelDelta;
                InputService.Current.OnMouseWheelScroll(delta);
            };
        }
    }
}
