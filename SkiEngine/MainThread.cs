﻿using System;
using System.Threading.Tasks;

namespace SkiEngine
{
    public static class MainThread
    {
        public static Func<Action, Task> InvokeOnMainThreadFunc { get; set; }

        public static Task InvokeOnMainThreadAsync(Action action) => InvokeOnMainThreadFunc(action);
    }
}