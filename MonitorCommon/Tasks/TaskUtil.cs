using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using TaskExtensions = LanguageExt.TaskExtensions;

namespace MonitorCommon.Tasks;

public static class TaskUtil
{
    public static bool IsSuccessful(this Task t) => t.Status == TaskStatus.RanToCompletion;

    public static ITaskResult<Unit> Result(this Task t) => t.ToUnit().Result();

    public static ITaskResult<T> Result<T>(this Task<T> t)
    {
        if (t.IsSuccessful())
        {
            return new Success<T>(t.Result);
        } 
            
        if (t.IsFaulted)
        {
            return new Failure<T>(t.Exception ?? new AggregateException(new Exception("Task faulted empty exception")));
        }

        if (t.IsCanceled)
        {
            return new Cancel<T>();
        }

        throw new Exception("Task not complete");
    }

    public static T AwaitResult<T>(this Task<T> t)
    {
        t.Wait();

        return t.Result;
    }

    public static Task<ITaskResult<Unit>> ResultAsync(this Task src) => src.ToUnit().ResultAsync();

    public static Task<ITaskResult<T>> ResultAsync<T>(this Task<T> src) => src.ContinueWith(t => t.Result());

    public static Task<T> AsTask<T>(this ITaskResult<T> result)
    {
        TaskCompletionSource<T> tcs = new();
        tcs.Complete(result);
        return tcs.Task;
    }

    public static Task<TSource> OnComplete<TSource>(this Task<TSource> t, Action<ITaskResult<TSource>> f) => t.MapAllEx(r =>
    {
        f(r);

        return r;
    });
    public static void OnComplete(this Task t, Action<ITaskResult<Unit>> f) => t.ContinueWith(t2 => f(t2.Result()));
    public static void OnSuccess<TSource>(this Task<TSource> t, Action<TSource> f) => t.ContinueWith(t2 => f(t2.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
    public static void OnSuccess(this Task t, Action f) => t.ContinueWith(t2 => f(), TaskContinuationOptions.OnlyOnRanToCompletion);
    public static void OnFailure(this Task t, Action<AggregateException> f) => t.ContinueWith(t2 => f(t2.Exception ?? new AggregateException(new Exception("Task faulted empty exception"))), TaskContinuationOptions.OnlyOnFaulted);
    public static void OnCanceled(this Task t, Action f) => t.ContinueWith(t2 => f(), TaskContinuationOptions.OnlyOnCanceled);

    public static Task MapAsync<TSource>(this Task<TSource> src, Func<TSource, Task> f) => TaskExtensions.MapAsync(src, x => f(x).ToUnit());

    public static Task<TResult> MapAsync<TResult>(this Task src, Func<Task<TResult>> f) => TaskExtensions.MapAsync(src.ToUnit(), x => f());

    public static Task<TResult> Recover<TResult>(this Task<TResult> src, Func<AggregateException, TResult> f)
    {
        return src.RecoverWith(e => Task.FromResult(f(e)));
    }

    public static Task Recover(this Task src, Action<AggregateException> f)
    {
        return src.ToUnit().Recover(e =>
        {
            f(e);

            return Unit.Default;
        });
    }
        
    public static async Task<TResult> RecoverWith<TResult>(this Task<TResult> src, Func<AggregateException, Task<TResult>> f)
    {
        ITaskResult<TResult> r = await src.ResultAsync();
        if (!r.IsFailure)
        {
            return await r.AsTask();
        }

        return await f(r.Failed.Get);
    }

    public static Task RecoverWith(this Task src, Func<AggregateException, Task> f)
    {
        return src.ToUnit().RecoverWith(e => f(e).ToUnit());
    }

    public static Task<TResult> Map<TResult>(this Task src, Func<TResult> f) => src.ToUnit().Map(_ => f());

    public static async Task<TResult> MapAll<TSource, TResult>(this Task<TSource> src, Func<ITaskResult<TSource>, TResult> f) => f(await src.ResultAsync());

    public static async Task<TResult> MapAll<TResult>(this Task src, Func<ITaskResult<Unit>, TResult> f) => f(await src.ResultAsync());

    public static async Task<TResult> MapAllEx<TSource, TResult>(this Task<TSource> src, Func<ITaskResult<TSource>, ITaskResult<TResult>> f) => await f(await src.ResultAsync()).AsTask();

    public static async Task<TResult> FlatMapAll<TSource, TResult>(this Task<TSource> src, Func<ITaskResult<TSource>, Task<TResult>> f) => await f(await src.ResultAsync());

    public static async Task<TResult> FlatMapAll<TResult>(this Task src, Func<ITaskResult<Unit>, Task<TResult>> f) => await f(await src.ResultAsync());

    public static void Complete<T>(this TaskCompletionSource<T> tcs, ITaskResult<T> r)
    {
        switch (r)
        {
            case Success<T> s: tcs.SetResult(s.value); break;
            case Failure<T> f: tcs.SetException(f.exception); break;
            case Cancel<T>: tcs.SetCanceled(); break;
            default:
                throw new Exception($"Unknown task result type {r.GetType().Name}");
        }
    }

    public static void TryComplete<T>(this TaskCompletionSource<T> tcs, ITaskResult<T> r)
    {
        switch (r)
        {
            case Success<T> s: tcs.TrySetResult(s.value); break;
            case Failure<T> f: tcs.TrySetException(f.exception); break;
            case Cancel<T>: tcs.TrySetCanceled(); break;
            default:
                throw new Exception($"Unknown task result type {r.GetType().Name}");
        }
    }

    public static Task ToTask(this WaitHandle waitHandle)
    {
        return ToTask(waitHandle, TimeSpan.MaxValue);
    }

    public static Task ToTask(this WaitHandle waitHandle, TimeSpan timeout)
    {
        TaskCompletionSource<Unit> tcs = new();

        ThreadPool.RegisterWaitForSingleObject(
            waitObject: waitHandle,
            callBack: (_, timeoutReached) =>
            {
                if (timeoutReached)
                {
                    tcs.TrySetException(new TimeoutException($"Timeout of {timeout} exceeded while waiting for handle {waitHandle}"));
                }
                else
                {
                    tcs.TrySetResult(Unit.Default);
                }
            }, 
            state: null, 
            timeout: timeout, 
            executeOnlyOnce: true);

        return tcs.Task;
    } 
}