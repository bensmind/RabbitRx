using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using RabbitRx.Message;

namespace RabbitRx.Client
{
    public class JsonSubscriptionConsumer<T> : SubscriptionConsumer<IRabbitMessage<T>>
    {
        public JsonSubscriptionConsumer(IModel model, string queueName) 
            : base(model, queueName)
        {
        }

        public override void OnNext(BasicDeliverEventArgs value)
        {
            var message = new JsonRabbitMessage<T>(value);
            Subject.OnNext(message);
        }
    }

    public class SubscriptionConsumer : SubscriptionConsumer<BasicDeliverEventArgs>
    {
        public SubscriptionConsumer(IModel model, string queueName) 
            : base(model, queueName)
        {
        }

        public override void OnNext(BasicDeliverEventArgs value)
        {
            Subject.OnNext(value);
        }
    }


    public abstract class SubscriptionConsumer<TData> : ISubject<BasicDeliverEventArgs, TData>, IDisposable
    {
        private readonly Subscription _subscription;

        protected readonly Subject<TData> Subject = new Subject<TData>();

        protected SubscriptionConsumer(IModel model, string queueName)
        {
            _subscription = new Subscription(model, queueName, false);
        }

        public virtual Task Start(CancellationToken token)
        {
            var task = new Task(() => Consume(token), token, TaskCreationOptions.LongRunning);
            task.Start(TaskScheduler.Default);
            return task;
        }

        protected virtual void Consume(CancellationToken token)
        {
            token.Register(_subscription.Close); //This breaks the block below

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var evt = _subscription.Next(); //Blocking de-queue

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

        public virtual void Dispose()
        {
            _subscription.Close();
        }
    }
}
