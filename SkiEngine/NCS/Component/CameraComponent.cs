using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.Component
{
    public class CameraComponent : Base.Component
    {
        public delegate void DrawOrderChangedDelegate(CameraComponent component, int previousDrawOrder);
        public event DrawOrderChangedDelegate DrawOrderChanged;

        private int _drawOrder;
        private readonly LayeredSets<OrderAndDepth, IDrawableComponent> _layeredComponents;
        private readonly Dictionary<IDrawableComponent, OrderAndDepth> _componentToLayerMap;

        private SKMatrix _worldToPixelMatrix;
        private SKMatrix _pixelToWorldMatrix;

        public CameraComponent(int drawOrder, int viewTarget)
        {
            _drawOrder = drawOrder;
            ViewTarget = viewTarget;
            
            _layeredComponents = new LayeredSets<OrderAndDepth, IDrawableComponent>(component => _componentToLayerMap[component]);
            _componentToLayerMap = new Dictionary<IDrawableComponent, OrderAndDepth>(ReferenceEqualityComparer<IDrawableComponent>.Default);
        }

        public struct OrderAndDepth
        {
            private readonly int _order;
            private readonly int _depth;

            public OrderAndDepth(int order, int depth)
            {
                _order = order;
                _depth = depth;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is OrderAndDepth))
                {
                    return false;
                }

                var depth = (OrderAndDepth)obj;
                return _order == depth._order 
                    && _depth == depth._depth;
            }

            public override int GetHashCode()
            {
                var hashCode = -425239920;
                hashCode = hashCode * -1521134295 + _order.GetHashCode();
                hashCode = hashCode * -1521134295 + _depth.GetHashCode();
                return hashCode;
            }
        }

        public int ViewTarget { get; set; }

        public ref SKMatrix WorldToPixelMatrix => ref _worldToPixelMatrix;
        public ref SKMatrix PixelToWorldMatrix => ref _pixelToWorldMatrix;

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

        public void AddDrawable(IDrawableComponent component, int order)
        {
            if (component == null)
            {
                return;
            }

            if (_componentToLayerMap.ContainsKey(component))
            {
                var previousLayer = _componentToLayerMap[component];
                _componentToLayerMap[component] = new OrderAndDepth(order, component.Node.Depth);
                _layeredComponents.Update(component, previousLayer);
            }
            else
            {
                component.Destroyed += RemoveDrawable;
                _componentToLayerMap[component] = new OrderAndDepth(order, component.Node.Depth);
                _layeredComponents.Add(component);
            }
        }

        public void RemoveDrawable(IDrawableComponent component)
        {
            RemoveDrawable((IComponent) component);
        }

        private void RemoveDrawable(IComponent component)
        {
            var drawableComponent = (IDrawableComponent) component;
            _layeredComponents.Remove(drawableComponent);
            _componentToLayerMap.Remove(drawableComponent);
            component.Destroyed -= RemoveDrawable;
        }

        public void ZoomTo(IEnumerable<SKPoint> worldPoints)
        {
            var localBoundingBox = worldPoints.Select(Node.WorldToLocalMatrix.MapPoint).BoundingBox();
            Node.WorldPoint = Node.LocalToWorldMatrix.MapPoint(localBoundingBox.Mid());
            var widthProportion = localBoundingBox.Width / _previousDeviceClipBounds.Width;
            var heightProportion = localBoundingBox.Height / _previousDeviceClipBounds.Height;

            Node.RelativeScale = Node.RelativeScale.Multiply(
                widthProportion > heightProportion 
                    ? widthProportion 
                    : heightProportion
            );
        }

        private SKRectI _previousDeviceClipBounds;
        private SKMatrix _deviceClipBoundsTranslationMatrix;
        public void Draw(SKCanvas canvas)
        {
            var deviceClipBounds = canvas.DeviceClipBounds;
            if (deviceClipBounds != _previousDeviceClipBounds)
            {
                _deviceClipBoundsTranslationMatrix = SKMatrix.MakeTranslation(deviceClipBounds.Width / 2f, deviceClipBounds.Height / 2f);
                _previousDeviceClipBounds = deviceClipBounds;
            }

            _worldToPixelMatrix = Node.WorldToLocalMatrix;

            SKMatrix.PostConcat(ref _worldToPixelMatrix, ref _deviceClipBoundsTranslationMatrix);

            _worldToPixelMatrix.TryInvert(out _pixelToWorldMatrix);

            WorldViewport = _pixelToWorldMatrix.MapRect(deviceClipBounds);

            foreach (var component in _layeredComponents)
            {
                var drawMatrix = component.Node.LocalToWorldMatrix;
                SKMatrix.PostConcat(ref drawMatrix, ref _worldToPixelMatrix);

                canvas.SetMatrix(drawMatrix);

                component.DrawablePart.Draw(canvas, component.Node);
            }
        }
    }
}
