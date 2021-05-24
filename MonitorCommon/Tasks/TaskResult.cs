using System;
using System.Collections;
using System.Collections.Generic;

namespace MonitorCommon.Tasks
{
    public interface ITaskResult<out T> : IEnumerable<T>
    {
        bool IsFailure { get; }
        bool IsSuccess { get; }
        bool IsCanceled { get; }

        //U GetOrElse<U>(Func<U> def) where T : U;
        //TaskResult<U> OrElse<U>(Func<TaskResult<U>> def) where T : U;

        T Get { get; }

        void Foreach(Action<T> f);

        ITaskResult<TNew> FlatMap<TNew>(Func<T, ITaskResult<TNew>> f);
        ITaskResult<TNew> Map<TNew>(Func<T, TNew> f);

        ITaskResult<AggregateException> Failed { get; }
        ITaskResult<bool> Canceled { get; }

        ITaskResult<TNew> Transform<TNew>(Func<T, ITaskResult<TNew>> s, Func<AggregateException, ITaskResult<TNew>> f,
            Func<bool, ITaskResult<TNew>> c);
    }

    public sealed class Success<T> : ITaskResult<T>
    {
        public readonly T value;

        public Success(T value)
        {
            this.value = value;
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            yield return value;
        }
        
        public bool IsFailure => false;
        public bool IsSuccess => true;
        public bool IsCanceled => false;
        public T Get => value;
        public void Foreach(Action<T> f) => f(value);
        public ITaskResult<TNew> FlatMap<TNew>(Func<T, ITaskResult<TNew>> f) => f(value);
        public ITaskResult<TNew> Map<TNew>(Func<T, TNew> f) => new Success<TNew>(f(value));

        public ITaskResult<AggregateException> Failed => new Failure<AggregateException>(new AggregateException("Success.Failed"));
        public ITaskResult<bool> Canceled => new Failure<bool>(new AggregateException("Success.Canceled"));

        public ITaskResult<TNew> Transform<TNew>(Func<T, ITaskResult<TNew>> s, Func<AggregateException, ITaskResult<TNew>> f, Func<bool, ITaskResult<TNew>> c) => s(value);

        public override string ToString()
        {
            return $"Success({value})";
        }
    }

    public sealed class Failure<T> : ITaskResult<T>
    {
        public readonly AggregateException exception;

        public Failure(AggregateException exception)
        {
            this.exception = exception;
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            yield break;
        }

        public bool IsFailure => true;
        public bool IsSuccess => false;
        public bool IsCanceled => false;
        public T Get => throw exception;
        public void Foreach(Action<T> f) { }

        public ITaskResult<TNew> FlatMap<TNew>(Func<T, ITaskResult<TNew>> f) => new Failure<TNew>(exception);
        public ITaskResult<TNew> Map<TNew>(Func<T, TNew> f) => new Failure<TNew>(exception);

        public ITaskResult<AggregateException> Failed => new Success<AggregateException>(exception);
        public ITaskResult<bool> Canceled => new Failure<bool>(new AggregateException("Failure.Canceled"));
        public ITaskResult<TNew> Transform<TNew>(Func<T, ITaskResult<TNew>> s, Func<AggregateException, ITaskResult<TNew>> f, Func<bool, ITaskResult<TNew>> c) => f(exception);

        public override string ToString()
        {
            return $"Failure({exception})";
        }
    }

    public sealed class Cancel<T> : ITaskResult<T>
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            yield break;
        }

        public bool IsFailure => false;
        public bool IsSuccess => false;
        public bool IsCanceled => true;
        public T Get => throw new InvalidOperationException("Canceled.Get");
        public void Foreach(Action<T> f) { }

        public ITaskResult<TNew> FlatMap<TNew>(Func<T, ITaskResult<TNew>> f) => new Cancel<TNew>();
        public ITaskResult<TNew> Map<TNew>(Func<T, TNew> f) => new Cancel<TNew>();

        public ITaskResult<AggregateException> Failed => new Failure<AggregateException>(new AggregateException("Cancel.Failed"));
        public ITaskResult<bool> Canceled => new Success<bool>(true);
        public ITaskResult<TNew> Transform<TNew>(Func<T, ITaskResult<TNew>> s, Func<AggregateException, ITaskResult<TNew>> f, Func<bool, ITaskResult<TNew>> c) => c(true);

        public override string ToString()
        {
            return "Canceled";
        }
    }
}