using System.Collections.Concurrent;
using SkiaSharp;
// ReSharper disable InconsistentNaming

namespace SkiEngine.Touch
{
    // Represents a SkiaSharp.Views.Forms.SKTouchEventArgs for projects that shouldn't have a Xamarin.Forms dependency.
    public class SKTouch
    {
        private static readonly ConcurrentQueue<SKTouch> _cachedTouches = new ConcurrentQueue<SKTouch>();

        public static SKTouch Get(
            long id,
            SKTouchAction type,
            SKMouseButton mouseButton,
            SKTouchDeviceType deviceType,
            SKPoint location,
            bool inContact
        )
        {
            if (!_cachedTouches.TryDequeue(out var result))
            {
                result = new SKTouch();
            }

            result.Handled = false;
            result.Id = id;
            result.ActionType = type;
            result.MouseButton = mouseButton;
            result.DeviceType = deviceType;
            result.Location = location;
            result.InContact = inContact;

            return result;
        }

        public bool Handled { get; set; }
        public long Id { get; private set; }
        public SKTouchAction ActionType { get; private set; }
        public SKTouchDeviceType DeviceType { get; private set; }
        public SKMouseButton MouseButton { get; private set; }
        public SKPoint Location { get; private set; }
        public bool InContact { get; private set; }

        public void Recycle()
        {
            _cachedTouches.Enqueue(this);
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
