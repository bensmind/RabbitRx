using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitRx.Subscription.Base
{
    public abstract class ObservableSubscriptionBase<TData> : RabbitMQ.Client.MessagePatterns.Subscription, ISubject<BasicDeliverEventArgs, TData>
    {
        protected readonly Subject<TData> Subject = new Subject<TData>();

        protected ObservableSubscriptionBase(IModel model, string queueName)
            : base(model, queueName)
        {
        }

        protected ObservableSubscriptionBase(IModel model, string queueName, bool noAck)
            : base(model, queueName, noAck)
        {
        }

        protected ObservableSubscriptionBase(IModel model, string queueName, bool noAck, string consumerTag)
            : base(model, queueName, noAck, consumerTag)
        {
        }

        public virtual Task Start(CancellationToken token, int? timeout = null, Action onQueueEmpty = null)
        {
            var task = new Task(() => Consume(token, timeout, onQueueEmpty), token, TaskCreationOptions.LongRunning);
            task.Start(TaskScheduler.Default);
            return task;
        }

        protected virtual void Consume(CancellationToken token, int? timeout = null, Action onQueueEmpty = null)
        {
            token.Register(Close); //This breaks the block below
            
            while (true)
            {
                try
                {
                    BasicDeliverEventArgs evt;
                    if (timeout.HasValue)
                    {
                        Next(timeout.Value, out evt);
                    }
                    else
                    {
                        evt = Next(); //Blocking de-queue
                    }

                    if (token.IsCancellationRequested) break;

                    if (evt != null)
                    {
                        OnNext(evt); //Publish
                    }
                    else if (onQueueEmpty != null)
                    {
                        onQueueEmpty();
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }

            OnCompleted(); //End of stream
        }

        public abstract void OnNext(BasicDeliverEventArgs value);

        public virtual void OnError(Exception error)
        {
            Subject.OnError(error);
        }

        public virtual void OnCompleted()
        {
            Subject.OnCompleted();
        }

        public abstract IDisposable Subscribe(IObserver<TData> observer);
    }
}
