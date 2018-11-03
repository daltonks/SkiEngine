using System;
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
        private readonly LayeredSets<int, IDrawableComponent> _componentLayeredSets;
        private readonly Dictionary<IDrawableComponent, int> _componentToZMap;

        private SKMatrix _worldToPixelMatrix;
        private SKMatrix _pixelToWorldMatrix;

        public CameraComponent(int drawOrder, int viewTarget)
        {
            _drawOrder = drawOrder;
            ViewTarget = viewTarget;
            
            _componentLayeredSets = new LayeredSets<int, IDrawableComponent>(component => _componentToZMap[component]);
            _componentToZMap = new Dictionary<IDrawableComponent, int>(ReferenceEqualityComparer<IDrawableComponent>.Default);
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

            if (_componentToZMap.ContainsKey(component))
            {
                var previousZ = _componentToZMap[component];
                _componentToZMap[component] = component.Node.WorldZ;
                _componentLayeredSets.Update(component, previousZ);
            }
            else
            {
                component.Destroyed += RemoveDrawable;
                _componentToZMap[component] = component.Node.WorldZ;
                _componentLayeredSets.Add(component);
            }
        }

        public void RemoveDrawable(IDrawableComponent component)
        {
            RemoveDrawable((IComponent) component);
        }

        private void RemoveDrawable(IComponent component)
        {
            var drawableComponent = (IDrawableComponent) component;
            _componentLayeredSets.Remove(drawableComponent);
            _componentToZMap.Remove(drawableComponent);
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

            foreach (var component in _componentLayeredSets)
            {
                var drawMatrix = component.Node.LocalToWorldMatrix;
                SKMatrix.PostConcat(ref drawMatrix, ref _worldToPixelMatrix);

                canvas.SetMatrix(drawMatrix);

                component.DrawablePart.Draw(canvas, component.Node);
            }
        }
    }
}
