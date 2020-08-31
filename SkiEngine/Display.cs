using System;

namespace SkiEngine
{
    public static class Display
    {
        public static Func<double> DensityFunc { get;set; }
        public static double Density => DensityFunc();
    }
}
