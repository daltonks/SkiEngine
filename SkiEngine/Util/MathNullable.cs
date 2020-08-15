using System;

namespace SkiEngine.Util
{
    public static class MathNullable
    {
        public static float? Min(float? val1, float? val2)
        {
            if (val1 == null)
            {
                return val2;
            }

            if (val2 == null)
            {
                return val1;
            }

            return Math.Min(val1.Value, val2.Value);
        }

        public static float? Max(float? val1, float? val2)
        {
            if (val1 == null)
            {
                return val2;
            }

            if (val2 == null)
            {
                return val1;
            }

            return Math.Max(val1.Value, val2.Value);
        }
    }
}
