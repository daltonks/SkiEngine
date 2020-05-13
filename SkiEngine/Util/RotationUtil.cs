using System;

namespace SkiEngine.Util
{
    public static class RotationUtil
    {
        private const double TwoPi = Math.PI * 2;

        /// <summary>
        /// Wraps rotation to stay between -PI and PI
        /// </summary>
        public static double WrapRotation(double rotation)
        {
            return rotation - TwoPi * Math.Floor((rotation + Math.PI) / TwoPi);
        }
    }
}
