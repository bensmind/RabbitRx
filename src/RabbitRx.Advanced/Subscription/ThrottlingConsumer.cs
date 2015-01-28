using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using RabbitRx.Core.Subscription;

namespace RabbitRx.Advanced.Subscription
{
    public class ThrottlingConsumer<T> : IObservable<T> 
    {
        private readonly Subject<T>  _subject = new Subject<T>();
        private readonly int _maxTasks;
        private readonly int _minTasks;
        private readonly IObservableSubscription<T> _subscription;

        private readonly ConcurrentBag<Tuple<CancellationTokenSource, Task>> _tasks = new ConcurrentBag<Tuple<CancellationTokenSource, Task>>();
        private int _processedCount;

        public ThrottlingConsumer(IObservableSubscription<T> subscription, int maxTasks, int minTasks = 1)
        {
            _maxTasks = maxTasks;
            _minTasks = minTasks;
            _subscription = subscription;

            _subscription.Subscribe(_subject);
        }

        private void UpdateScale(T last)
        {
            var queue = _subscription.Model.QueueDeclarePassive(_subscription.QueueName);
            var thisSample = (int) queue.MessageCount;
            var lastSample = Interlocked.Exchange(ref _processedCount, thisSample);

            if (lastSample != 0)
            {
                if (thisSample < lastSample)
                {
                    RemoveWorker();
                }
                else
                {
                    AddWorker();
                }
            }
            Console.WriteLine("Thread Count: {0}", _tasks.Count);
        }

        public Task Start(CancellationToken token, TimeSpan samplingInterval)
        {
            _subject.Sample(samplingInterval).Subscribe(UpdateScale, token);
            token.Register(CancelAll);

            for (var i = 0; i < _minTasks; i++)
            {
                AddWorker();
            }

            return Task.WhenAll(_tasks.Select(t => t.Item2));
        }

        private void CancelAll()
        {
            foreach (var task in _tasks)
            {
                task.Item1.Cancel();
            }
        }

        private void AddWorker()
        {
            if (_tasks.Count < _maxTasks)
            {
                var tkn = new CancellationTokenSource();

                var task = _subscription.Start(tkn.Token, 300, RemoveWorker);
                _tasks.Add(new Tuple<CancellationTokenSource, Task>(tkn, task));
            }
        }

        private void RemoveWorker()
        {
            if (_tasks.Count > _minTasks)
            {
                Tuple<CancellationTokenSource, Task> removed;
                if (_tasks.TryTake(out removed))
                {
                    removed.Item1.Cancel();
                }
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
