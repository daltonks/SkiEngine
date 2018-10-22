﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.Component
{
    public class CameraComponent : Base.Component
    {
        public delegate void DrawOrderChangedDelegate(CameraComponent component, int previousDrawOrder);
        public event DrawOrderChangedDelegate DrawOrderChanged;

        private int _drawOrder;
        private readonly LayeredSets<int, IDrawableComponent> _layeredComponents;
        private readonly Dictionary<IDrawableComponent, int> _componentToLayerMap;

        private SKMatrix _worldToPixelMatrix;
        private SKMatrix _pixelToWorldMatrix;

        public CameraComponent(int drawOrder, int viewTarget)
        {
            _drawOrder = drawOrder;
            ViewTarget = viewTarget;
            
            _layeredComponents = new LayeredSets<int, IDrawableComponent>(component => _componentToLayerMap[component]);
            _componentToLayerMap = new Dictionary<IDrawableComponent, int>(ReferenceEqualityComparer<IDrawableComponent>.Default);
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

        public void ZoomTo(IEnumerable<SKPoint> worldPoints)
        {
            ZoomTo(worldPoints.BoundingBox());
        }

        public void ZoomTo(SKRect rect)
        {
            var localBoundingBox = Node.WorldToLocalMatrix.MapRect(rect);

            Node.RelativePoint += localBoundingBox.Mid();

            var widthProportion = rect.Width / WorldViewport.Width;
            var heightProportion = rect.Height / WorldViewport.Height;

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
