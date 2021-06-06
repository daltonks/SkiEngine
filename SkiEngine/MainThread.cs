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

        public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) => InvokeOnMainThreadAsync(() => Task.FromResult(func()));

        public static async Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func)
        {
            T result = default;

            await InvokeOnMainThreadAsync(async () => {
                result = await func().ConfigureAwait(false);
            }).ConfigureAwait(false);

            return result;
        }
    }
}
