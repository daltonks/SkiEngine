using System;
using SkiEngine.Updateable;

namespace SkiEngine.UI
{
    public class SkiAnimation
    {
        public SkiAnimation(
            Action<double> callback, 
            double start, 
            double end,
            TimeSpan length,
            Action finished = null
        )
        {
            Callback = callback;
            Start = start;
            End = end;
            Length = length;
            Finished = finished;
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Action<double> Callback { get; }
        public double Start { get; }
        public double End { get; }
        public TimeSpan Length { get; }
        public Action Finished { get; }
    }
}
