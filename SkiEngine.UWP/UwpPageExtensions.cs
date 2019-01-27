using Windows.UI.Xaml.Controls;
using SkiEngine.Input;

namespace SkiEngine.UWP
{
    public static class UwpPageExtensions
    {
        public static InputService CreateInputService(this Page page)
        {
            var inputService = new InputService();

            page.PointerWheelChanged += (sender, args) =>
            {
                var delta = args.GetCurrentPoint(page).Properties.MouseWheelDelta;
                inputService.OnMouseWheelScroll(delta);
            };

            return inputService;
        }
    }
}
