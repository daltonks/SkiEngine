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

        private bool _worldTransformIsDirty;

        private SKPoint _relativePoint;
        private double _relativeRotation;
        private SKPoint _relativeScale;

        private SKPoint _worldPoint;
        private double _worldRotation;
        private SKPoint _worldScale;

        public SKPoint RelativePoint
        {
            get => _relativePoint;
            set
            {
                _relativePoint = value;
                _worldTransformIsDirty = true;
            }
        }

        public double RelativeRotation
        {
            get => _relativeRotation;
            set
            {
                // Wrap rotation to stay between -PI and PI
                _relativeRotation = value - TwoPi * Math.Floor((value + Math.PI) / TwoPi);
                _worldTransformIsDirty = true;
            }
        }

        public SKPoint RelativeScale
        {
            get => _relativeScale;
            set
            {
                _relativeScale = value;
                _worldTransformIsDirty = true;
            }
        }

        public SKPoint WorldPoint
        {
            get
            {
                TryRecalculateWorldTransform();
                return _worldPoint;
            }
            set
            {
                if (_parent == null)
                {
                    RelativePoint = value;
                }
                else
                {
                    var relativePoint = (value - _parent.WorldPoint).Rotate(-_parent.WorldRotation);

                    relativePoint.X = _parent.WorldScale.X == 0
                        ? 0
                        : relativePoint.X / _parent.WorldScale.X;

                    relativePoint.Y = _parent.WorldScale.Y == 0
                        ? 0
                        : relativePoint.Y / _parent.WorldScale.Y;

                    RelativePoint = relativePoint;
                }
            }
        }

        public double WorldRotation
        {
            get
            {
                TryRecalculateWorldTransform();
                return _worldRotation;
            }
            set
            {
                if (_parent == null)
                {
                    RelativeRotation = value;
                }
                else
                {
                    RelativeRotation = value - _parent.WorldRotation;
                }
            }
        }

        public SKPoint WorldScale
        {
            get
            {
                TryRecalculateWorldTransform();
                return _worldScale;
            }
            set
            {
                if (_parent == null)
                {
                    RelativeScale = value;
                }
                else
                {
                    RelativeScale = new SKPoint(
                        _parent.WorldScale.X == 0 
                            ? 0 
                            : value.X / _parent.WorldScale.X,
                        _parent.WorldScale.Y == 0 
                            ? 0 
                            : value.Y / _parent.WorldScale.Y
                    );
                }
            }
        }

        private void TryRecalculateWorldTransform()
        {
            if (!_worldTransformIsDirty)
            {
                return;
            }

            if (_parent == null)
            {
                _worldRotation = RelativeRotation;
                _worldScale = RelativeScale;
                _worldPoint = RelativePoint;
            }
            else
            {
                _worldRotation = _parent.WorldRotation + RelativeRotation;
                _worldScale = _parent.WorldScale.Multiply(RelativeScale);
                _worldPoint = _parent.WorldPoint + RelativePoint.Multiply(_parent.WorldScale).Rotate(_parent.WorldRotation);
            }

            foreach (var child in _children)
            {
                child._worldTransformIsDirty = true;
            }

            _worldTransformIsDirty = false;
        }
    }
}
