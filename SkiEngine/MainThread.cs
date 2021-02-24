using System;
using System.Threading.Tasks;

namespace SkiEngine
{
    public static class MainThread
    {
        public static Func<Func<Task>, Task> InvokeOnMainThreadFunc { get; set; }

        public static Task InvokeOnMainThreadAsync(Func<Task> func) => InvokeOnMainThreadFunc(func);

        public static Task InvokeOnMainThreadAsync(Action action) => InvokeOnMainThreadAsync(() => {
            action();
            return Task.CompletedTask;
        });
    }
}
