using System;
using System.Threading.Tasks;

namespace SkiEngine.Util
{
    public class TaskQueue
    {
        private readonly object _locker = new object();
        private volatile Task _lastTask = Task.CompletedTask;
        private volatile bool _isShutdown;

        public Task QueueAsync(Action action)
        {
            return QueueAsync(() => {
                action();
                return true;
            });
        }

        public Task<T> QueueAsync<T>(Func<T> function)
        {
            lock (_locker)
            {
                var resultTask = _lastTask.ContinueWith(
                    _ =>
                    {
                        ThrowIfShutdown();
                        return function();
                    }, 
                    TaskContinuationOptions.RunContinuationsAsynchronously
                );
                _lastTask = resultTask;
                return resultTask;
            }
        }

        public Task QueueAsync(Func<Task> asyncAction)
        {
            lock (_locker)
            {
                _lastTask = _lastTask.ContinueWith(
                    _ =>
                    {
                        ThrowIfShutdown();
                        return asyncAction();
                    }, 
                    TaskContinuationOptions.RunContinuationsAsynchronously
                ).Unwrap();
                return _lastTask;
            }
        }

        public Task<T> QueueAsync<T>(Func<Task<T>> asyncFunction)
        {
            lock (_locker)
            {
                var resultTask = _lastTask.ContinueWith(
                    _ =>
                    {
                        ThrowIfShutdown();
                        return asyncFunction();
                    }, 
                    TaskContinuationOptions.RunContinuationsAsynchronously
                ).Unwrap();

                _lastTask = resultTask;
                return resultTask;
            }
        }

        private void ThrowIfShutdown()
        {
            if (_isShutdown)
            {
                throw new ObjectDisposedException(nameof(TaskQueue));
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