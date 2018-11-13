using System;
using SkiaSharp;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace SkiEngine.NCS
{
    public partial class Node
    {
        private const double TwoPi = Math.PI * 2;

        private SKPoint _relativePoint;
        private float _relativeRotation;
        private SKPoint _relativeScale;
        private int _worldZ;

        private bool _localToWorldDirty;
        private bool _worldToLocalDirty;

        private SKMatrix _localToWorldMatrix;
        private SKMatrix _worldToLocalMatrix;

        public ref SKMatrix LocalToWorldMatrix
        {
            get
            {
                if (_localToWorldDirty)
                {
                    CalculateLocalToParentMatrix(out _localToWorldMatrix);

                    if (Parent != null)
                    {
                        SKMatrix.PreConcat(ref _localToWorldMatrix, ref Parent.LocalToWorldMatrix);
                    }

                    _localToWorldDirty = false;
                }
                
                return ref _localToWorldMatrix;
            }
        }

        public ref SKMatrix WorldToLocalMatrix
        {
            get
            {
                if (_worldToLocalDirty)
                {
                    ref var localToWorldMatrix = ref LocalToWorldMatrix;
                    localToWorldMatrix.TryInvert(out _worldToLocalMatrix);

                    _worldToLocalDirty = false;
                }

                return ref _worldToLocalMatrix;
            }
        }
        
        public SKPoint RelativePoint
        {
            get => _relativePoint;
            set
            {
                _relativePoint = value;
                SetMatricesDirty();
            }
        }

        public int RelativeZ
        {
            get => Parent == null ? WorldZ : WorldZ - Parent.WorldZ;
            set
            {
                var parentWorldZ = Parent?.WorldZ ?? 0;
                WorldZ = parentWorldZ + value;
            }
        }

        public float RelativeRotation
        {
            get => _relativeRotation;
            set
            {
                // Wrap rotation to stay between -PI and PI
                _relativeRotation = (float) (value - TwoPi * Math.Floor((value + Math.PI) / TwoPi));
                SetMatricesDirty();
            }
        }

        public SKPoint RelativeScale
        {
            get => _relativeScale;
            set
            {
                _relativeScale = value;
                SetMatricesDirty();
            }
        }

        public SKPoint WorldPoint
        {
            get => LocalToWorldMatrix.MapPoint(SKPoint.Empty);
            set => RelativePoint = Parent?.WorldToLocalMatrix.MapPoint(value) ?? value;
        }

        public int WorldZ
        {
            get => _worldZ;
            set
            {
                if (_worldZ == value)
                {
                    return;
                }

                var previousZ = _worldZ;
                _worldZ = value;

                var difference = value - _worldZ;
                foreach (var child in _children)
                {
                    child.WorldZ += difference;
                }

                Scene.OnNodeZChanged(this, previousZ);
            }
        }

        public void CalculateLocalToParentMatrix(out SKMatrix result)
        {
            var translationMatrix = SKMatrix.MakeTranslation(_relativePoint.X, _relativePoint.Y);
            var rotationMatrix = SKMatrix.MakeRotation(_relativeRotation);
            var scaleMatrix = SKMatrix.MakeScale(_relativeScale.X, _relativeScale.Y);
            
            result = translationMatrix;
            SKMatrix.PreConcat(ref result, ref rotationMatrix);
            SKMatrix.PreConcat(ref result, ref scaleMatrix);
        }

        private void SetMatricesDirty()
        {
            if (_localToWorldDirty)
            {
                return;
            }

            _localToWorldDirty = true;
            _worldToLocalDirty = true;

            foreach (var child in _children)
            {
                child.SetMatricesDirty();
            }
        }
    }
}
