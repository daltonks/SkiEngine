using Foundation;
using SkiEngine.Xamarin;
using SkiEngine.Xamarin.Forms.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SkiUiView), typeof(SkiUiViewRenderer))]
namespace SkiEngine.Xamarin.Forms.iOS
{
    public class SkiUiViewRenderer : ScrollViewRenderer
    {
        private double _previousScrollY;

        public SkiUiViewRenderer()
        {
            CanCancelContentTouches = false;
            DelaysContentTouches = false;
            ExclusiveTouch = false;
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement is ScrollView oldScrollView)
            {
                oldScrollView.Scrolled -= OnScrolled;
            }

            if (e.NewElement is ScrollView newScrollView)
            {
                _previousScrollY = 0;
                newScrollView.Scrolled += OnScrolled;
            }

            foreach (var gestureRecognizer in GestureRecognizers)
            {
                RemoveGestureRecognizer(gestureRecognizer);
            }
        }

        // This is a workaround for iOS not scrolling all the way back down when an entry is unfocused.
        // : (
        private void OnScrolled(object sender, ScrolledEventArgs e)
        {
            var scrollView = (ScrollView) sender;

            var scrolledDown = e.ScrollY < _previousScrollY;
            if (scrolledDown)
            {
                _ = scrollView.ScrollToAsync(0, 0, false);
                _previousScrollY = 0;
            }
            else
            {
                _previousScrollY = e.ScrollY;
            }
        }

        public override bool TouchesShouldCancelInContentView(UIView view)
        {
            return false;
        }

        public override bool TouchesShouldBegin(NSSet touches, UIEvent withEvent, UIView inContentView)
        {
            return false;
        }
        
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            
        }
    }
}