using SkiaSharp;
using SkiEngine.NCS.System;

namespace SkiEngine.Interfaces
{
    public interface IDrawable
    {
        void Draw(SKCanvas canvas, UpdateTime updateTime);
    }
}