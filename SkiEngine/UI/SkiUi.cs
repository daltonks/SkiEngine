using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SkiEngine.UI
{
    public static class SkiUi
    {
        private static readonly Thread Thread;
        private static readonly BlockingCollection<(Action action, TaskCompletionSource<bool> completionSource)> Actions = new BlockingCollection<(Action action, TaskCompletionSource<bool> completionSource)>();

        static SkiUi()
        {
            Thread = new Thread(() => {
                foreach (var (action, completionSource) in Actions.GetConsumingEnumerable())
                {
                    try
                    {
                        action();
                        completionSource.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        completionSource.SetException(ex);
                    }
                }
            });
            Thread.Start();
        }

        public static Task RunAsync(Action action, bool forceQueue = false)
        {
            if (!forceQueue && Thread.CurrentThread == Thread)
            {
                action();
                return Task.CompletedTask;
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();
            Actions.Add((action, taskCompletionSource));
            return taskCompletionSource.Task;
        }
    }
}
