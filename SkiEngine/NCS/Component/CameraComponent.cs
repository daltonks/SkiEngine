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
        public delegate void RenderChangedDelegate(CameraComponent component);

        public event DrawOrderChangedDelegate DrawOrderChanged;
        public event RenderChangedDelegate EnabledChanged;

        private readonly LayeredSets<int, IDrawableComponent> _drawableComponents;

        public CameraComponent(CanvasComponent canvasComponent, int drawOrder)
        {
            _drawOrder = drawOrder;
            _drawableComponents = new LayeredSets<int, IDrawableComponent>(component => component.Node.WorldZ);

            canvasComponent?.AddCamera(this);
        }

        public CanvasComponent CanvasComponent { get; internal set; }

        public IReadOnlyList<int> OrderedLayers => _drawableComponents.OrderedLayers;

        public ref SKMatrix XamarinToPixelMatrix => ref CanvasComponent.XamarinToPixelMatrix;
        public ref SKMatrix PixelToXamarinMatrix => ref CanvasComponent.PixelToXamarinMatrix;

        private SKMatrix _worldToPixelMatrix;
        public ref SKMatrix WorldToPixelMatrix => ref _worldToPixelMatrix;

        private SKMatrix _pixelToWorldMatrix;
        public ref SKMatrix PixelToWorldMatrix => ref _pixelToWorldMatrix;

        public SKRectI PixelViewport { get; private set; }
        public SKRect WorldViewport { get; private set; }

        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }

                _enabled = value;
                EnabledChanged?.Invoke(this);
            }
        }

        private int _drawOrder;
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
            if (_drawableComponents.Remove(drawableComponent, previousZ))
            {
                _drawableComponents.Add(drawableComponent);
            }
        }

        public void AddDrawable(IDrawableComponent component)
        {
            if (component == null)
            {
                return;
            }

            if (_drawableComponents.Add(component))
            {
                component.Destroyed += RemoveDrawable;
                _drawableComponents.Add(component);
            }
        }

        public IReadOnlyCollection<IDrawableComponent> GetDrawablesWithZ(int z)
        {
            return _drawableComponents.GetItems(z);
        }

        public void RemoveDrawable(IDrawableComponent component)
        {
            RemoveDrawable((IComponent) component);
        }

        private void RemoveDrawable(IComponent component)
        {
            var drawableComponent = (IDrawableComponent) component;
            _drawableComponents.Remove(drawableComponent);
            component.Destroyed -= RemoveDrawable;
        }

        public void ZoomTo(IEnumerable<SKPoint> worldPoints)
        {
            var localBoundingBox = worldPoints.Select(Node.WorldToLocalMatrix.MapPoint).BoundingBox();
            Node.WorldPoint = Node.LocalToWorldMatrix.MapPoint(localBoundingBox.Mid());
            var widthProportion = localBoundingBox.Width / CanvasComponent.PixelViewport.Width;
            var heightProportion = localBoundingBox.Height / CanvasComponent.PixelViewport.Height;

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

        public void Draw(SKCanvas canvas)
        {
            RecalculatePixelMatrices();

            foreach (var component in _drawableComponents)
            {
                var drawMatrix = component.Node.LocalToWorldMatrix;
                SKMatrix.PostConcat(ref drawMatrix, ref _worldToPixelMatrix);

                canvas.SetMatrix(drawMatrix);

                component.Draw(canvas);
            }
        }

        public void RecalculatePixelMatrices()
        {
            _worldToPixelMatrix = Node.WorldToLocalMatrix;
            SKMatrix.PostConcat(ref _worldToPixelMatrix, ref CanvasComponent.HalfPixelViewportTranslationMatrix);
            _worldToPixelMatrix.TryInvert(out _pixelToWorldMatrix);

            WorldViewport = _pixelToWorldMatrix.MapRect(PixelViewport);
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
