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

        public virtual Task Start(CancellationToken token)
        {
            var task = new Task(() => Consume(token), token, TaskCreationOptions.LongRunning);
            task.Start(TaskScheduler.Default);
            return task;
        }

        protected virtual void Consume(CancellationToken token)
        {
            token.Register(Close); //This breaks the block below

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var evt = Next(); //Blocking de-queue

                    if (evt != null)
                    {
                        OnNext(evt); //Publish
                    }
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                OnCompleted(); //End of stream
            }
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

        public IDisposable Subscribe(IObserver<TData> observer)
        {
            return Subject.Subscribe(observer);
        }
    }
}
