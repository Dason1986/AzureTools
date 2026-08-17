using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace AzureTools.Extensions
{
    public static class UiThreadRunner
    {
        public static Task<TResult> RunAsync<TResult>(Func<TResult> func)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    TResult result = func();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }

        public static Task RunAsync(Action action)
        {
            TaskCompletionSource tcs = new TaskCompletionSource();
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }

        public static Task RunAsync(Func<Task> func)
        {
            TaskCompletionSource tcs = new TaskCompletionSource();
            Dispatcher.UIThread.Post(async () =>
            {
                try
                {
                    await func();
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }

        public static Task<TResult> RunAsync<TResult>(Func<Task<TResult>> func)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            Dispatcher.UIThread.Post(async () =>
            {
                try
                {
                    TResult result = await func();
                    tcs.TrySetResult(result);
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