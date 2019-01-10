using SkiaSharp;
// ReSharper disable InconsistentNaming

namespace SkiEngine.Touch
{
    // Represents a SkiaSharp.Views.Forms.SKTouchEventArgs for projects that shouldn't have a Xamarin.Forms dependency.
    public class SKTouch
    {
        public bool Handled { get; set; }
        public long Id { get; }
        public SKTouchAction ActionType { get; }
        public SKTouchDeviceType DeviceType { get; }
        public SKMouseButton MouseButton { get; }
        public SKPoint Location { get; }
        public bool InContact { get; }

        public SKTouch(
            long id,
            SKTouchAction type,
            SKMouseButton mouseButton,
            SKTouchDeviceType deviceType,
            SKPoint location,
            bool inContact)
        {
            Id = id;
            ActionType = type;
            DeviceType = deviceType;
            MouseButton = mouseButton;
            Location = location;
            InContact = inContact;
        }
    }

    public enum SKTouchAction
    {
        Entered,
        Pressed,
        Moved,
        Released,
        Cancelled,
        Exited,
    }

    public enum SKTouchDeviceType
    {
        Touch,
        Mouse,
        Pen
    }

    public enum SKMouseButton
    {
        Unknown,
        Left,
        Middle,
        Right
    }
}
