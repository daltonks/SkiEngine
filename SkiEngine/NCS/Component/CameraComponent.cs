using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.Extensions.SkiaSharp;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.Component
{
    public class CameraComponent : Base.Component
    {
        public delegate void DrawOrderChangedDelegate(CameraComponent component, int previousDrawOrder);
        public event DrawOrderChangedDelegate DrawOrderChanged;

        private int _drawOrder;
        private readonly LayeredSets<int, IDrawableComponent> _componentLayeredSets;

        public CameraComponent(int drawOrder, int viewTarget)
        {
            _drawOrder = drawOrder;
            ViewTarget = viewTarget;
            
            _componentLayeredSets = new LayeredSets<int, IDrawableComponent>(component => component.Node.WorldZ);
        }

        public int ViewTarget { get; set; }

        public IReadOnlyList<int> OrderedLayers => _componentLayeredSets.OrderedLayers;

        private SKMatrix _xamarinToPixelMatrix;
        public ref SKMatrix XamarinToPixelMatrix => ref _xamarinToPixelMatrix;

        private SKMatrix _pixelToXamarinMatrix;
        public ref SKMatrix PixelToXamarinMatrix => ref _pixelToXamarinMatrix;

        private SKMatrix _worldToPixelMatrix;
        public ref SKMatrix WorldToPixelMatrix => ref _worldToPixelMatrix;

        private SKMatrix _pixelToWorldMatrix;
        public ref SKMatrix PixelToWorldMatrix => ref _pixelToWorldMatrix;

        public SKRectI PixelViewport { get; private set; }
        public SKRect WorldViewport { get; private set; }

        public int DrawOrder
        {
            get => _drawOrder;
            set
            {
                var previousDrawOrder = _drawOrder;
                if (value == previousDrawOrder)
                {
                    return;
                }

                _drawOrder = value;
                DrawOrderChanged?.Invoke(this, previousDrawOrder);
            }
        }

        internal void OnZChanged(IDrawableComponent drawableComponent, int previousZ)
        {
            if (_componentLayeredSets.Remove(drawableComponent, previousZ))
            {
                _componentLayeredSets.Add(drawableComponent);
            }
        }

        public void AddDrawable(IDrawableComponent component)
        {
            if (component == null)
            {
                return;
            }

            if (_componentLayeredSets.Add(component))
            {
                component.Destroyed += RemoveDrawable;
                _componentLayeredSets.Add(component);
            }
        }

        public IReadOnlyCollection<IDrawableComponent> GetDrawablesWithZ(int z)
        {
            return _componentLayeredSets.GetItems(z);
        }

        public void RemoveDrawable(IDrawableComponent component)
        {
            RemoveDrawable((IComponent) component);
        }

        private void RemoveDrawable(IComponent component)
        {
            var drawableComponent = (IDrawableComponent) component;
            _componentLayeredSets.Remove(drawableComponent);
            component.Destroyed -= RemoveDrawable;
        }

        public void ZoomTo(IEnumerable<SKPoint> worldPoints)
        {
            var localBoundingBox = worldPoints.Select(Node.WorldToLocalMatrix.MapPoint).BoundingBox();
            Node.WorldPoint = Node.LocalToWorldMatrix.MapPoint(localBoundingBox.Mid());
            var widthProportion = localBoundingBox.Width / _previousPixelViewport.Width;
            var heightProportion = localBoundingBox.Height / _previousPixelViewport.Height;

            Node.RelativeScale = Node.RelativeScale.Multiply(
                widthProportion > heightProportion 
                    ? widthProportion 
                    : heightProportion
            );
        }

        public void ZoomWithFocus(double zoomDelta, SKPoint worldPoint, float minZoom, float maxZoom)
        {
            var resultScale = Node.RelativeScale.X * (1 + zoomDelta);
            if (resultScale < minZoom)
            {
                zoomDelta = (minZoom - Node.RelativeScale.X) / Node.RelativeScale.X;
            }
            else if (resultScale > maxZoom)
            {
                zoomDelta = (maxZoom - Node.RelativeScale.X) / Node.RelativeScale.X;
            }
            ZoomWithFocus(zoomDelta, worldPoint);
        }

        public void ZoomWithFocus(double zoomDelta, SKPoint worldPoint)
        {
            Node.WorldPoint += worldPoint.VectorTo(Node.WorldPoint).Multiply(zoomDelta);
            Node.RelativeScale = Node.RelativeScale.Multiply(1 + zoomDelta);
        }

        private SKRectI _previousPixelViewport;
        private SKMatrix _halfPixelViewportTranslationMatrix;
        public void Draw(SKCanvas canvas, double widthXamarinUnits, double heightXamarinUnits)
        {
            PixelViewport = canvas.DeviceClipBounds;
            if (PixelViewport != _previousPixelViewport)
            {
                _xamarinToPixelMatrix = SKMatrix.MakeScale(
                    (float) (PixelViewport.Width / widthXamarinUnits),
                    (float) (PixelViewport.Height / heightXamarinUnits)
                );

                _pixelToXamarinMatrix = SKMatrix.MakeScale(
                    (float) (widthXamarinUnits / PixelViewport.Width),
                    (float) (heightXamarinUnits / PixelViewport.Height)
                );

                _halfPixelViewportTranslationMatrix = SKMatrix.MakeTranslation(PixelViewport.Width / 2f, PixelViewport.Height / 2f);
                _previousPixelViewport = PixelViewport;
            }

            _worldToPixelMatrix = Node.WorldToLocalMatrix;

            SKMatrix.PostConcat(ref _worldToPixelMatrix, ref _halfPixelViewportTranslationMatrix);

            _worldToPixelMatrix.TryInvert(out _pixelToWorldMatrix);

            WorldViewport = _pixelToWorldMatrix.MapRect(PixelViewport);

            foreach (var component in _componentLayeredSets)
            {
                var drawMatrix = component.Node.LocalToWorldMatrix;
                SKMatrix.PostConcat(ref drawMatrix, ref _worldToPixelMatrix);

                canvas.SetMatrix(drawMatrix);

                component.Draw(canvas);
            }
        }
    }

    public static class CameraDrawableComponentExtensions
    {
        public static TDrawable AddToCamera<TDrawable>(this TDrawable drawable, CameraComponent camera)
            where TDrawable : IDrawableComponent
        {
            camera.AddDrawable(drawable);
            return drawable;
        }
    }
}
