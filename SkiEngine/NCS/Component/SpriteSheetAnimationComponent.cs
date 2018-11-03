using System;
using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;
using SkiEngine.Sprite;

namespace SkiEngine.NCS.Component
{
    public class SpriteSheetAnimationComponent<TState, TLayer> : Base.Component, IDrawableComponent, IUpdateableComponent
    {
        public SKImage Image { get; set; }
        public SKPaint Paint { get; set; }
        public SpriteSheetAnimationData Data { get; }
        public UpdateableComponentPart UpdateablePart { get; }

        private SpriteSheetAnimationStateData _currentState;
        private SpriteSheetAnimationFrameData _currentFrame;

        public TState State
        {
            set
            {
                var state = Data.States[Convert.ToInt32(value)];
                SetState(state);
            }
        }

        protected SpriteSheetAnimationComponent(SKImage image, SpriteSheetAnimationData data)
        {    
            Image = image;
            Data = data;

            SetState(Data.States[0]);

            UpdateablePart = new UpdateableComponentPart(Update);
        }

        private void SetState(SpriteSheetAnimationStateData state)
        {
            if (_currentState != state)
            {
                _currentState = state;
                _currentFrame = state.Frames[0];
            }
        }

        private TimeSpan _accumTime;
        public void Update(UpdateTime updateTime)
        {
            _accumTime += updateTime.Delta;
            var frameDuration = _currentFrame.Duration;
            while (_accumTime >= frameDuration)
            {
                _currentFrame = _currentState.Frames[(_currentFrame.IndexInState + 1) % _currentState.Frames.Length];
                _accumTime -= frameDuration;
            }
        }

        public void Draw(SKCanvas canvas, ITransform transform)
        {
            foreach (var spriteData in _currentFrame.Sprites)
            {
                canvas.DrawImage(Image, Paint, spriteData);
            }
        }
    }
}
