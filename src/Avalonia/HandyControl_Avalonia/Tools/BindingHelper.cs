using System;

namespace HandyControl.Tools;

internal static class BindingHelper
{
    public static IObservable<TOut> Select<TIn, TOut>(this IObservable<TIn> source, Func<TIn, TOut> selector) =>
        new SelectImpl<TIn, TOut>(source, selector);

    public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext) =>
        new SubscribeImpl<T>(source, onNext);

    private sealed class SelectImpl<TIn, TOut> : IObservable<TOut>
    {
        private readonly IObservable<TIn> _source;
        private readonly Func<TIn, TOut> _selector;

        public SelectImpl(IObservable<TIn> source, Func<TIn, TOut> selector)
        {
            _source = source;
            _selector = selector;
        }

        public IDisposable Subscribe(IObserver<TOut> observer) =>
            _source.Subscribe(new ObserverImpl(observer, _selector));

        private sealed class ObserverImpl : IObserver<TIn>
        {
            private readonly IObserver<TOut> _observer;
            private readonly Func<TIn, TOut> _selector;

            public ObserverImpl(IObserver<TOut> observer, Func<TIn, TOut> selector)
            {
                _observer = observer;
                _selector = selector;
            }

            public void OnCompleted() => _observer.OnCompleted();

            public void OnError(Exception error) => _observer.OnError(error);

            public void OnNext(TIn value)
            {
                try
                {
                    _observer.OnNext(_selector(value));
                }
                catch (Exception error2)
                {
                    _observer.OnError(error2);
                }
            }
        }
    }

    private sealed class SubscribeImpl<T> : IDisposable, IObserver<T>
    {
        private readonly Action<T> _onNext;
        private IDisposable? _subscription;

        public SubscribeImpl(IObservable<T> source, Action<T> onNext)
        {
            _onNext = onNext;
            _subscription = source.Subscribe(this);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value) => _onNext(value);

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
