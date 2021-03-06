﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace SkiEngine
{
    public interface ISkiEngineInitializer
    {
        double DisplayDensity { get; }
        bool AllowInvalidateSurfaceIfDrawStillPending { get; }
        
        Task InvokeOnMainThreadAsync(Func<Task> func);
        Task<Stream> OpenAppPackageFileAsync(string path);
    }
}
