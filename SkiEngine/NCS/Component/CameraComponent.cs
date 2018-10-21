﻿using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.Component
{
    public class CameraComponent : Base.Component, IDrawable
    {
        public delegate void DrawOrderChangedDelegate(CameraComponent component, int previousDrawOrder);
        public event DrawOrderChangedDelegate DrawOrderChanged;

        private int _drawOrder;
        private readonly LayeredSets<int, IDrawableComponent> _layeredComponents;
        private readonly Dictionary<IDrawableComponent, int> _componentToLayerMap;

        private SKMatrix _pixelToWorldMatrix;

        public CameraComponent(int drawOrder, IEnumerable<int> viewTargets)
        {
            _drawOrder = drawOrder;
            ViewTargets = new HashSet<int>(viewTargets);

            Destroyed += OnDestroyed;

            _layeredComponents = new LayeredSets<int, IDrawableComponent>(component => _componentToLayerMap[component]);
            _componentToLayerMap = new Dictionary<IDrawableComponent, int>(ReferenceEqualityComparer<IDrawableComponent>.Default);
        }

        public ISet<int> ViewTargets { get; }

        public ref readonly SKMatrix PixelToWorldMatrix => ref _pixelToWorldMatrix;

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
        
        public void Draw(SKCanvas canvas, int viewTarget)
        {
            if (!ViewTargets.Contains(viewTarget))
            {
                return;
            }

            var deviceClipBounds = canvas.DeviceClipBounds;
            var deviceClipTranslationMatrix = SKMatrix.MakeTranslation(deviceClipBounds.Width / 2f, deviceClipBounds.Height / 2f);

            var cameraMatrix = Node.WorldToLocalMatrix;

            SKMatrix.PreConcat(ref cameraMatrix, ref deviceClipTranslationMatrix);

            cameraMatrix.TryInvert(out _pixelToWorldMatrix);

            foreach (var component in _layeredComponents)
            {
                var drawMatrix = component.Node.LocalToWorldMatrix;
                SKMatrix.PreConcat(ref drawMatrix, ref cameraMatrix);

                canvas.SetMatrix(drawMatrix);

                component.DrawablePart.Draw(canvas, component.Node);
            }
        }

        private void OnDestroyed(IComponent component)
        {
            
        }
    }
}
