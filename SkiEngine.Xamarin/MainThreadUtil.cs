using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkiEngine.Xamarin
{
    public static class MainThreadUtil
    {
        public static Task InvokeOnMainThreadAsync(Func<Task> asyncFunc)
        {
            var tcs = new TaskCompletionSource<bool>();

            Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    await asyncFunc.Invoke();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
