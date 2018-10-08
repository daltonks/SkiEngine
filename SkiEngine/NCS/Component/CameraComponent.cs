using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;
using SkiEngine.Util;

namespace SkiEngine.NCS.Component
{
    public class CameraComponent : Base.Component
    {
        public ISet<int> ViewTargets { get; }

        public delegate void DrawOrderChangedDelegate(CameraComponent component, int previousDrawOrder);
        public event DrawOrderChangedDelegate DrawOrderChanged;

        private int _drawOrder;
        private readonly LayeredSets<int, IDrawableComponent> _layeredComponents;
        private readonly Dictionary<IDrawableComponent, int> _componentToLayerMap;

        public CameraComponent(int drawOrder, IEnumerable<int> viewTargets)
        {
            _drawOrder = drawOrder;
            ViewTargets = new HashSet<int>(viewTargets);

            Destroyed += OnDestroyed;

            _layeredComponents = new LayeredSets<int, IDrawableComponent>(component => _componentToLayerMap[component]);
            _componentToLayerMap = new Dictionary<IDrawableComponent, int>(ReferenceEqualityComparer<IDrawableComponent>.Default);
        }

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
                _componentToLayerMap[component] = order;
                _layeredComponents.Update(component, previousLayer);
            }
            else
            {
                component.Destroyed += RemoveDrawable;
                _componentToLayerMap[component] = order;
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

        public SKPoint PixelToWorld(SKPoint pixelPoint)
        {
            return _pixelToWorldMatrix.MapPoint(pixelPoint);
        }

        private SKPoint _lastPoint;
        private float _lastRotation;
        private SKPoint _lastScale;
        private SKRectI _lastDeviceClipBounds;
        private SKMatrix _worldToPixelMatrix;
        private SKMatrix _pixelToWorldMatrix;
        public void Draw(SKCanvas canvas, int viewTarget)
        {
            if (!ViewTargets.Contains(viewTarget))
            {
                return;
            }

            var currentPoint = Node.WorldPoint;
            var currentRotation = Node.WorldRotation;
            var currentScale = Node.WorldScale;
            var currentDeviceClipBounds = canvas.DeviceClipBounds;

            // Recreate _worldToPixelMatrix only if it changed
            if (!_lastPoint.Equals(currentPoint)
                || !_lastRotation.Equals(currentRotation)
                || !_lastScale.Equals(currentScale)
                || !_lastDeviceClipBounds.Equals(currentDeviceClipBounds))
            {
                _worldToPixelMatrix = SKMatrix.MakeTranslation(currentDeviceClipBounds.Width / 2f, currentDeviceClipBounds.Height / 2f);
                SKMatrix.PostConcat(ref _worldToPixelMatrix, SKMatrix.MakeScale(currentScale.X, currentScale.Y));
                SKMatrix.PostConcat(ref _worldToPixelMatrix, SKMatrix.MakeRotation(currentRotation));
                SKMatrix.PostConcat(ref _worldToPixelMatrix, SKMatrix.MakeTranslation(-currentPoint.X, -currentPoint.Y));
                
                _worldToPixelMatrix.TryInvert(out _pixelToWorldMatrix);

                _lastPoint = currentPoint;
                _lastRotation = currentRotation;
                _lastScale = currentScale;
                _lastDeviceClipBounds = currentDeviceClipBounds;
            }

            canvas.Save();

            canvas.Concat(ref _worldToPixelMatrix);

            foreach (var component in _layeredComponents)
            {
                component.DrawablePart.Draw(canvas, component.Node);
            }
            
            canvas.Restore();
        }

        private void OnDestroyed(IComponent component)
        {
            
        }
    }
}
