using System;
using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.Interfaces;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace SkiEngine.NCS
{
    public partial class Node : ITransform
    {
        private const double TwoPi = Math.PI * 2;

        private SKPoint _relativePoint;
        private float _relativeRotation;
        private SKPoint _relativeScale;

        private bool _localToWorldDirty;
        private bool _worldToLocalDirty;

        private SKMatrix _localToWorldMatrix;
        private SKMatrix _worldToLocalMatrix;

        public ref readonly SKMatrix LocalToWorldMatrix
        {
            get
            {
                if (_localToWorldDirty)
                {
                    CalculateLocalToParentMatrix(out _localToWorldMatrix);

                    if (_parent != null)
                    {
                        SKMatrix.PreConcat(ref _localToWorldMatrix, _parent.LocalToWorldMatrix);
                    }

                    _localToWorldDirty = false;
                }
                
                return ref _localToWorldMatrix;
            }
        }

        public ref readonly SKMatrix WorldToLocalMatrix
        {
            get
            {
                if (_worldToLocalDirty)
                {
                    LocalToWorldMatrix.TryInvert(out _worldToLocalMatrix);

                    _worldToLocalDirty = false;
                }

                return ref _worldToLocalMatrix;
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
        
        public SKPoint RelativePoint
        {
            get => _relativePoint;
            set
            {
                _relativePoint = value;
                SetMatricesDirty();
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
