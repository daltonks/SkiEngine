using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SkiEngine.Util
{
    public class TaskQueue
    {
        public event Action<int> NumTasksQueuedChanged;

        private readonly object _locker = new object();
        private volatile Task _lastTask = Task.CompletedTask;
        private volatile bool _isShutdown;

        private volatile int _numTasksQueued;
        public int NumTasksQueued => _numTasksQueued;

        public async Task<T> QueueAsync<T>(Func<T> function)
        {
            T result = default;

            await QueueAsync(() => {
                result = function.Invoke();
            }).ConfigureAwait(false);

            return result;
        }

        public async Task<T> QueueAsync<T>(Func<Task<T>> asyncFunction)
        {
            T result = default;

            await QueueAsync(async () => {
                result = await asyncFunction.Invoke().ConfigureAwait(false);
            }).ConfigureAwait(false);

            return result;
        }

        public Task QueueAsync(Action action)
        {
            return QueueAsync(() => {
                action();
                return Task.CompletedTask;
            });
        }
        
        public Task QueueAsync(Func<Task> asyncAction)
        {
            NumTasksQueuedChanged?.Invoke(
                Interlocked.Increment(ref _numTasksQueued)
            );

            lock (_locker)
            {
                _lastTask = _lastTask.ContinueWith(
                    async _ =>
                    {
                        if (_isShutdown)
                        {
                            throw new ObjectDisposedException(nameof(TaskQueue));
                        }

                        try
                        {
                            await asyncAction().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }

                        NumTasksQueuedChanged?.Invoke(
                            Interlocked.Decrement(ref _numTasksQueued)
                        );
                    }, 
                    TaskContinuationOptions.RunContinuationsAsynchronously
                ).Unwrap();
                return _lastTask;
            }
        }

        public Task ShutdownAsync()
        {
            return QueueAsync(() => {
                _isShutdown = true;
            });
        }
    }
}