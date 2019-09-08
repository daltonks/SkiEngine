using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiEngine.Util
{
    public class TaskQueue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly object _locker = new object();
        private volatile Task _lastTask = Task.CompletedTask;
        private volatile bool _isShutdown;

        private volatile int _numTasksQueued;
        public int NumTasksQueued => _numTasksQueued;

        public Task QueueAsync(Action action)
        {
            return QueueAsync(() => {
                action();
                return true;
            });
        }

        public Task<T> QueueAsync<T>(Func<T> function)
        {
            Interlocked.Increment(ref _numTasksQueued);
            OnPropertyChanged(nameof(NumTasksQueued));

            lock (_locker)
            {
                var resultTask = _lastTask.ContinueWith(
                    _ =>
                    {
                        ThrowIfShutdown();
                        var result = function();

                        Interlocked.Decrement(ref _numTasksQueued);
                        OnPropertyChanged(nameof(NumTasksQueued));

                        return result;
                    }, 
                    TaskContinuationOptions.RunContinuationsAsynchronously
                );
                _lastTask = resultTask;
                return resultTask;
            }
        }

        public Task QueueAsync(Func<Task> asyncAction)
        {
            Interlocked.Increment(ref _numTasksQueued);
            OnPropertyChanged(nameof(NumTasksQueued));

            lock (_locker)
            {
                _lastTask = _lastTask.ContinueWith(
                    async _ =>
                    {
                        ThrowIfShutdown();
                        await asyncAction();

                        Interlocked.Decrement(ref _numTasksQueued);
                        OnPropertyChanged(nameof(NumTasksQueued));
                    }, 
                    TaskContinuationOptions.RunContinuationsAsynchronously
                ).Unwrap();
                return _lastTask;
            }
        }

        public Task<T> QueueAsync<T>(Func<Task<T>> asyncFunction)
        {
            Interlocked.Increment(ref _numTasksQueued);
            OnPropertyChanged(nameof(NumTasksQueued));

            lock (_locker)
            {
                var resultTask = _lastTask.ContinueWith(
                    async _ =>
                    {
                        ThrowIfShutdown();

                        var result = await asyncFunction();

                        Interlocked.Decrement(ref _numTasksQueued);
                        OnPropertyChanged(nameof(NumTasksQueued));

                        return result;
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}