using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.Interfaces;

namespace SkiEngine.NCS
{
    public partial class Node : ITransform
    {
        private bool _worldTransformIsDirty;

        private SKPoint _relativePoint;
        private float _relativeRotation;
        private SKPoint _relativeScale;

        private SKPoint _worldPoint;
        private float _worldRotation;
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

        public float RelativeRotation
        {
            get => _relativeRotation;
            set
            {
                _relativeRotation = value;
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
            set => _worldPoint = value;
        }

        public float WorldRotation
        {
            get
            {
                TryRecalculateWorldTransform();
                return _worldRotation;
            }
            set => _worldRotation = value;
        }

        public SKPoint WorldScale
        {
            get
            {
                TryRecalculateWorldTransform();
                return _worldScale;
            }
            set => _worldScale = value;
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
