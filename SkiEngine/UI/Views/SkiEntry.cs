using SkiaSharp;
using SkiEngine.UI.Layouts;

namespace SkiEngine.UI.Views
{
    public class SkiEntry : SkiScrollView
    {
        public SkiEntry()
        {
            CanScrollHorizontally = true;
            CanScrollVertically = false;
            SizeRequest = new SKSize(-1, 40);
        }
    }
}
