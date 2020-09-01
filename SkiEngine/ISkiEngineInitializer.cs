﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace SkiEngine
{
    public interface ISkiEngineInitializer
    {
        double DisplayDensity { get; }

        Task InvokeOnMainThreadAsync(Action action);
        Task<Stream> OpenAppPackageFileAsync(string path);
    }
}
