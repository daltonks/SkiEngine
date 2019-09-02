using System;
using System.Threading.Tasks;

namespace SkiEngine.Util
{
    public class TaskQueue
    {
        private readonly object _locker = new object();
        private Task _lastTask = Task.CompletedTask;
        private bool _isShutdown;

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
                    _ => _isShutdown ? default : function(), 
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
                    async _ =>
                    {
                        if (!_isShutdown)
                        {
                            await asyncAction();
                        }
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
                    _ => _isShutdown ? Task.FromResult(default(T)) : asyncFunction(), 
                    TaskContinuationOptions.RunContinuationsAsynchronously
                ).Unwrap();

                _lastTask = resultTask;
                return resultTask;
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