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

            page.KeyDown += (sender, args) =>
            {
                var intKey = (int) args.Key;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                inputService.OnKeyDown(skiVirtualKey);
            };

            page.KeyUp += (sender, args) =>
            {
                var intKey = (int) args.Key;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                inputService.OnKeyUp(skiVirtualKey);
            };

            return inputService;
        }
    }
}
