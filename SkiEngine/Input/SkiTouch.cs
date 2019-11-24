using System.Collections.Concurrent;
using SkiaSharp;

// ReSharper disable InconsistentNaming
namespace SkiEngine.Input
{
    public class SkiTouch
    {
        private static readonly ConcurrentQueue<SkiTouch> _cachedTouches = new ConcurrentQueue<SkiTouch>();

        public static SkiTouch Get(
            long id,
            SKTouchAction type,
            SKMouseButton mouseButton,
            SKTouchDeviceType deviceType,
            SKPoint location,
            bool inContact,
            int wheelDelta
        )
        {
            if (!_cachedTouches.TryDequeue(out var result))
            {
                result = new SkiTouch();
            }

            result.Id = id;
            result.ActionType = type;
            result.MouseButton = mouseButton;
            result.DeviceType = deviceType;
            result.Location = location;
            result.InContact = inContact;
            result.WheelDelta = wheelDelta;

            return result;
        }

        public long Id { get; set; }
        public SKTouchAction ActionType { get; set; }
        public SKTouchDeviceType DeviceType { get; set; }
        public SKMouseButton MouseButton { get; set; }
        public SKPoint Location { get; set; }
        public bool InContact { get; set; }
        public int WheelDelta { get; set; }

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
        WheelChanged
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
