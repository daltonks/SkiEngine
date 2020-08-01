using SkiaSharp;
using SkiEngine.Util;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace SkiEngine
{
    public partial class Node
    {
        private bool _localToWorldDirty;
        private bool _worldToLocalDirty;

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

        private SKMatrix _localToWorldMatrix;
        public ref SKMatrix LocalToWorldMatrix
        {
            get
            {
                if (_localToWorldDirty)
                {
                    CalculateLocalToParentMatrix(out _localToWorldMatrix);

                    if (Parent != null)
                    {
                        SKMatrix.PostConcat(ref _localToWorldMatrix, ref Parent.LocalToWorldMatrix);
                    }

                    _localToWorldDirty = false;
                }
                
                return ref _localToWorldMatrix;
            }
        }

        private SKMatrix _worldToLocalMatrix;
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
        
        private SKPoint _relativePoint;
        public SKPoint RelativePoint
        {
            get => _relativePoint;
            set
            {
                _relativePoint = value;
                SetMatricesDirty();
            }
        }

        public SKPoint WorldPoint
        {
            get => LocalToWorldMatrix.MapPoint(SKPoint.Empty);
            set => RelativePoint = Parent?.WorldToLocalMatrix.MapPoint(value) ?? value;
        }

        public float RelativeZ
        {
            get => Parent == null ? WorldZ : WorldZ - Parent.WorldZ;
            set
            {
                var parentWorldZ = Parent?.WorldZ ?? 0;
                WorldZ = parentWorldZ + value;
            }
        }

        private float _worldZ;
        public float WorldZ
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

                var difference = value - previousZ;
                foreach (var child in _children)
                {
                    child.WorldZ += difference;
                }

                Scene.OnNodeZChanged(this, previousZ);
            }
        }

        private float _relativeRotation;
        public float RelativeRotation
        {
            get => _relativeRotation;
            set
            {
                _relativeRotation = (float) RotationUtil.WrapRotation(value);
                SetMatricesDirty();
            }
        }

        private SKPoint _relativeScale;
        public SKPoint RelativeScale
        {
            get => _relativeScale;
            set
            {
                _relativeScale = value;
                SetMatricesDirty();
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
    }
}
