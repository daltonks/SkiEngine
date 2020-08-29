using Android.Content;
using Android.Views;
using SkiEngine.Xamarin;
using SkiEngine.Xamarin.Forms.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SkiUiView), typeof(SkiUiViewRenderer))]
namespace SkiEngine.Xamarin.Forms.Android
{
    public class SkiUiViewRenderer : ScrollViewRenderer
    {
        public SkiUiViewRenderer(Context context) : base(context) { }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            return false;
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            return false;
        }
    }
}