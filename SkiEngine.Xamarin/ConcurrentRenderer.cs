using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        private readonly Action<Action> _queueDrawAction;
        private readonly DrawDelegate _drawAction;
        private readonly Action _drawCompleteAction;
        
        private volatile bool _pendingDraw;
        private SKSurface _surface;

        private readonly object _snapshotLock = new object();
        private readonly SnapshotHandler _snapshotHandler;
        private readonly Dictionary<int, SnapshotImage> _snapshots = new Dictionary<int, SnapshotImage>(1);
       
        private int _widthPixels;
        private int _heightPixels;
        private double _widthXamarinUnits;
        private double _heightXamarinUnits;

        public ConcurrentRenderer(
            Action<Action> queueDrawAction,
            DrawDelegate drawAction,
            Action drawCompleteAction
        )
        {
            _drawAction = drawAction;
            _drawCompleteAction = drawCompleteAction;
            _queueDrawAction = queueDrawAction;

            _snapshotHandler = new SnapshotHandler(this);
        }

        public SnapshotImage GetSnapshotAndAddUser(int id)
        {
            lock (_snapshotLock)
            {
                if (_snapshots.TryGetValue(id, out var snapshotImage))
                {
                    snapshotImage.AddUser();
                    return snapshotImage;
                }

                return new SnapshotImage(
                    SKImage.Create(new SKImageInfo(1, 1)),
                    new SKSizeI(0, 0)
                );
            }
        }

        private readonly object _pendingDrawLock = new object();
        public void TryDraw()
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
        public void OnPaintSurface(int widthPixels, int heightPixels, double widthXamarinUnits, double heightXamarinUnits, Action<Dictionary<int, SnapshotImage>> drawAction)
        {
            Dictionary<int, SnapshotImage> snapshots;
            lock (_snapshotLock)
            {
                snapshots = new Dictionary<int, SnapshotImage>(_snapshots);
                foreach (var snapshot in snapshots.Values)
                {
                    snapshot.AddUser();
                }
            }

            drawAction(snapshots);

            foreach (var snapshot in snapshots.Values)
            {
                snapshot.RemoveUser();
            }

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
                // Update _snapshots with new snapshots
                foreach(var pair in _snapshotHandler.Snapshots)
                {
                    var id = pair.Key;
                    var newSnapshot = pair.Value;

                    if (_snapshots.TryGetValue(id, out var existingSnapshot))
                    {
                        existingSnapshot.RemoveUser();
                    }
                    _snapshots[id] = newSnapshot;
                }
            }

            _drawCompleteAction();
        }

        public void Dispose()
        {
            _surface?.Dispose();

            foreach (var snapshotImage in _snapshots.Values)
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

            private readonly Dictionary<int, SnapshotImage> _snapshots = new Dictionary<int, SnapshotImage>();
            public IReadOnlyDictionary<int, SnapshotImage> Snapshots => _snapshots;

            internal void Reset()
            {
                _snapshots.Clear();
            }

            public SnapshotImage Snapshot(int id)
            {
                var skImage = _renderer._surface.Snapshot();

                var snapshot = new SnapshotImage(
                    skImage, 
                    new SKSizeI(_renderer._widthPixels, _renderer._heightPixels)
                );

                _snapshots[id] = snapshot;

                return snapshot;
            }
        }
    }
}
