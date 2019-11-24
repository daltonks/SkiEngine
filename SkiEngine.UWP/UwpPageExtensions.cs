using System.Diagnostics;
using System.Linq;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using SkiEngine.Input;
using Page = Windows.UI.Xaml.Controls.Page;

namespace SkiEngine.UWP
{
    public static class UwpPageExtensions
    {
        public static InputService CreateInputService(this Page page)
        {
            var inputService = new InputService
            {
                CalculateNumberOfMousePointersFunc = () => PointerDevice
                    .GetPointerDevices()
                    .Count(pointer => pointer.PointerDeviceType == PointerDeviceType.Mouse)
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
