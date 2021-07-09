using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SkiaSharp;
using SkiEngine.Canvas;
using SkiEngine.Drawable;
using SkiEngine.Util;
using SkiEngine.Util.Extensions.SkiaSharp;

namespace SkiEngine.Camera
{
    public class CameraComponent : Component, IEnumerable<IDrawableComponent>
    {
        public delegate void EnabledChangedDelegate(CameraComponent component);
        public delegate void DrawOrderChangedDelegate(CameraComponent component, int previousDrawOrder);

        public event EnabledChangedDelegate EnabledChanged;
        public event DrawOrderChangedDelegate DrawOrderChanged;

        private readonly LayeredSets<float, IDrawableComponent> _drawableComponents;

        public CameraComponent(CameraGroup cameraGroup, int drawOrder, bool enabled = true)
        {
            _drawOrder = drawOrder;
            _drawableComponents = new LayeredSets<float, IDrawableComponent>(component => component.Node.WorldZ);
            _enabled = enabled;

            cameraGroup?.Add(this);
        }

        public CameraGroup Group { get; internal set; }
        public CanvasComponent CanvasComponent => Group.CanvasComponent;

        public IReadOnlyList<float> OrderedLayers => _drawableComponents.OrderedLayers;
        public IEnumerable<IDrawableComponent> ReversedComponents => _drawableComponents.ReversedItems;

        public SKMatrix DpToWorldMatrix => DpToPixelMatrix.PostConcat(PixelToWorldMatrix);
        public SKMatrix WorldToDpMatrix => WorldToPixelMatrix.PostConcat(PixelToDpMatrix);

        public ref SKMatrix DpToPixelMatrix => ref CanvasComponent.DpToPixelMatrix;
        public ref SKMatrix PixelToDpMatrix => ref CanvasComponent.PixelToDpMatrix;

        private SKMatrix _worldToPixelMatrix;
        public ref SKMatrix WorldToPixelMatrix => ref _worldToPixelMatrix;

        private SKMatrix _pixelToWorldMatrix;
        public ref SKMatrix PixelToWorldMatrix => ref _pixelToWorldMatrix;

        public ref SKRect DpViewport => ref CanvasComponent.DpViewport;
        public ref SKRectI PixelViewport => ref CanvasComponent.PixelViewport;

        public SKSize WorldViewportSize
        {
            get
            {
                var worldViewportWidth = PixelToWorldMatrix
                    .MapVector(0, PixelViewport.Width)
                    .Length;
                var worldViewportHeight = PixelToWorldMatrix
                    .MapVector(0, PixelViewport.Height)
                    .Length;

                return new SKSize(worldViewportWidth, worldViewportHeight);
            }
        }

        private bool _enabled;
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

        public float Opacity { get; set; } = 1;

        internal void OnZChanged(IDrawableComponent drawableComponent, float previousZ)
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
            if (_drawableComponents.Remove(drawableComponent))
            {
                component.Destroyed -= RemoveDrawable;
            }
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

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void Draw(SKCanvas canvas, DrawOptions options)
        {
            if (options.UseOpacity && Opacity == 0)
            {
                return;
            }

            using var layerPaint = new SKPaint
            {
                Color = new SKColor(
                    byte.MaxValue, 
                    byte.MaxValue, 
                    byte.MaxValue, 
                    (byte)Math.Round(Opacity * byte.MaxValue)
                )
            };
            canvas.SaveLayer(layerPaint);

            RecalculatePixelMatrices();

            foreach (var component in _drawableComponents)
            {
                var drawMatrix = component.Node.LocalToWorldMatrix;
                SKMatrix.PostConcat(ref drawMatrix, ref _worldToPixelMatrix);

                canvas.SetMatrix(drawMatrix);

                component.Draw(canvas, this);
            }

            canvas.Restore();
        }

        public void RecalculatePixelMatrices()
        {
            if (CanvasComponent == null)
            {
                return;
            }

            _worldToPixelMatrix = Node.WorldToLocalMatrix;
            SKMatrix.PostConcat(ref _worldToPixelMatrix, ref CanvasComponent.HalfPixelViewportTranslationMatrix);
            _worldToPixelMatrix.TryInvert(out _pixelToWorldMatrix);
        }

        public IEnumerator<IDrawableComponent> GetEnumerator()
        {
            return _drawableComponents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
