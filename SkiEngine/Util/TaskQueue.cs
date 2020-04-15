using System;
using System.ComponentModel;
using System.Diagnostics;
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
        private readonly bool _enablePropertyChanged;

        private volatile int _numTasksQueued;
        public int NumTasksQueued => _numTasksQueued;

        public TaskQueue(bool enablePropertyChanged = false)
        {
            _enablePropertyChanged = enablePropertyChanged;
        }

        public async Task<T> QueueAsync<T>(Func<T> function)
        {
            T result = default;

            await QueueAsync(() => {
                result = function.Invoke();
            });

            return result;
        }

        public async Task<T> QueueAsync<T>(Func<Task<T>> asyncFunction)
        {
            T result = default;

            await QueueAsync(async () => {
                result = await asyncFunction.Invoke();
            });

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
            Interlocked.Increment(ref _numTasksQueued);
            OnPropertyChanged(nameof(NumTasksQueued));

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
                            await asyncAction();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }

                        Interlocked.Decrement(ref _numTasksQueued);
                        OnPropertyChanged(nameof(NumTasksQueued));
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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_enablePropertyChanged)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}