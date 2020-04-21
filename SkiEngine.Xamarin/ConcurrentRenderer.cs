using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiEngine.Xamarin
{
    public delegate void DrawDelegate(
        SKSurface surface, 
        ConcurrentRenderer.SnapshotHandler snapshotHandler, 
        double widthXamarinUnits, 
        double heightXamarinUnits,
        bool canvasSizeChanged
    );

    public class ConcurrentRenderer : IDisposable
    {
        private readonly DrawDelegate _drawAction;
        private readonly Action<Action> _queueDrawAction;

        private volatile bool _pendingDraw;
        private SKSurface _surface;

        private readonly object _snapshotLock = new object();
        private readonly SnapshotHandler _snapshotHandler;
        private readonly List<SnapshotImage> _snapshotImages = new List<SnapshotImage>(1);
       
        private int _widthPixels;
        private int _heightPixels;
        private double _widthXamarinUnits;
        private double _heightXamarinUnits;

        public ConcurrentRenderer(
            Action<Action> queueDrawAction,
            DrawDelegate drawAction
        )
        {
            _drawAction = drawAction;
            _queueDrawAction = queueDrawAction;

            _snapshotHandler = new SnapshotHandler(this);
        }

        public SnapshotImage[] GetSnapshotsAndAddUsers(IList<int> indices)
        {
            var result = new SnapshotImage[indices.Count];

            lock (_snapshotLock)
            {
                for(var i = 0; i < indices.Count; i++)
                {
                    var index = indices[i];

                    if (index < _snapshotImages.Count)
                    {
                        var snapshotImage = _snapshotImages[index];
                        snapshotImage.AddUser();
                        result[i] = snapshotImage;
                    }
                    else
                    {
                        result[i] = new SnapshotImage(
                            SKImage.Create(new SKImageInfo(1, 1)),
                            new SKSizeI(0, 0)
                        );
                    }
                }
            }

            return result;
        }

        private readonly object _pendingDrawLock = new object();
        public void TryDrawAsync()
        {
            var shouldDraw = false;

            lock (_pendingDrawLock)
            {
                if (!_pendingDraw)
                {
                    _pendingDraw = true;
                    shouldDraw = true;
                }
            }

            if (shouldDraw)
            {
                _queueDrawAction(() => {
                    Draw(false);
                });
            }
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void OnPaintSurface(SKPaintGLSurfaceEventArgs e, double widthXamarinUnits, double heightXamarinUnits, Action<IReadOnlyList<SnapshotImage>> drawAction)
        {
            SnapshotImage[] snapshots;
            lock (_snapshotLock)
            {
                snapshots = _snapshotImages.ToArray();
                foreach (var snapshot in snapshots)
                {
                    snapshot.AddUser();
                }
            }

            drawAction(snapshots);

            foreach (var snapshot in snapshots)
            {
                snapshot.RemoveUser();
            }

            var widthPixels = e.BackendRenderTarget.Width;
            var heightPixels = e.BackendRenderTarget.Height;

            if (_widthPixels == widthPixels 
                && _heightPixels == heightPixels 
                && _widthXamarinUnits == widthXamarinUnits 
                && _heightXamarinUnits == heightXamarinUnits)
            {
                return;
            }

            // Canvas size has changed
            _widthPixels = widthPixels;
            _heightPixels = heightPixels;
            _widthXamarinUnits = widthXamarinUnits;
            _heightXamarinUnits = heightXamarinUnits;
            
            _queueDrawAction(() => {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Recreate _surface
                _surface?.Dispose();

                _surface = SKSurface.Create(
                    new SKImageInfo(
                        _widthPixels,
                        _heightPixels,
                        SKImageInfo.PlatformColorType,
                        SKAlphaType.Premul
                    )
                );
                Debug.WriteLine($"Create canvas: {stopwatch.Elapsed.TotalMilliseconds}");

                // Redraw
                Draw(true);
            });
        }

        private void Draw(bool canvasSizeChanged)
        {
            if (_surface == null)
            {
                return;
            }

            _snapshotHandler.Reset();

            _pendingDraw = false;

             _drawAction(
                _surface, 
                _snapshotHandler, 
                _widthXamarinUnits, 
                _heightXamarinUnits,
                canvasSizeChanged
            );

            lock (_snapshotLock)
            {
                // Update _snapshotImages with new snapshots
                for(var i = 0; i < _snapshotHandler.Snapshots.Count; i++)
                {
                    var newSnapshot = _snapshotHandler.Snapshots[i];

                    if (i < _snapshotImages.Count)
                    {
                        _snapshotImages[i].RemoveUser();
                        _snapshotImages[i] = newSnapshot;
                    }
                    else
                    {
                        _snapshotImages.Add(newSnapshot);
                    }
                }

                // Dispose snapshots of unused indices
                for (var i = _snapshotHandler.Snapshots.Count; i < _snapshotImages.Count;)
                {
                    _snapshotImages[i].RemoveUser();
                    _snapshotImages.RemoveAt(i);
                }
            }
        }

        public SKColor GetPixelColor(IList<int> snapshotIndices, SKPoint pixelPoint)
        {
            using (
                var skImage = CreateImage(
                    snapshotIndices,
                    SKRectI.Create(
                        (int) pixelPoint.X,
                        (int) pixelPoint.Y,
                        1,
                        1
                    )
                )
            )
            using (var pixels = skImage.PeekPixels())
            {
                var pixelColor = pixels.GetPixelColor(0, 0);
                return pixelColor;
            }
        }

        public SKImage CreateImage(IList<int> snapshotIndices)
        {
            return CreateImage(snapshotIndices, new SKRectI(0, 0, _widthPixels, _heightPixels));
        }

        public SKImage CreateImage(IList<int> snapshotIndices, SKRectI rect)
        {
            using (
                var surface = SKSurface.Create(
                    new SKImageInfo(
                        rect.Width,
                        rect.Height,
                        SKImageInfo.PlatformColorType,
                        SKAlphaType.Premul
                    )
                )
            )
            {
                var canvas = surface.Canvas;
                canvas.Clear();
                canvas.Translate(-rect.Left, -rect.Top);

                var snapshots = GetSnapshotsAndAddUsers(snapshotIndices);
                foreach (var snapshot in snapshots)
                {
                    canvas.DrawImage(snapshot.SkImage, 0, 0);
                    snapshot.RemoveUser();
                }

                return surface.Snapshot();
            }
        }

        public void Dispose()
        {
            _surface?.Dispose();

            foreach (var snapshotImage in _snapshotImages)
            {
                snapshotImage.RemoveUser();
            }
        }

        public class SnapshotHandler
        {
            private readonly ConcurrentRenderer _renderer;

            public SnapshotHandler(ConcurrentRenderer renderer)
            {
                _renderer = renderer;
            }

            private readonly List<SnapshotImage> _snapshots = new List<SnapshotImage>();
            public IReadOnlyList<SnapshotImage> Snapshots => _snapshots;

            internal void Reset()
            {
                _snapshots.Clear();
            }

            public SnapshotImage Snapshot()
            {
                var skImage = _renderer._surface.Snapshot();

                var snapshot = new SnapshotImage(
                    skImage, 
                    new SKSizeI(_renderer._widthPixels, _renderer._heightPixels)
                );

                _snapshots.Add(snapshot);

                return snapshot;
            }
        }
    }
}
