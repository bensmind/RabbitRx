using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitRx.Core.Subscription
{
    public interface IObservableSubscription<out T> : IObservable<T>
    {
        Task Start(CancellationToken token, int? timeout = null, Action onQueueEmpty = null);
        IModel Model { get; }
        string QueueName { get; }
    }

    public class ObservableSubscription : SubscriptionConsumer, IObservableSubscription<BasicDeliverEventArgs>
    {
        protected ObservableSubscription(IModel model, string queueName)
            : base(model, queueName)
        {
        }

        protected ObservableSubscription(IModel model, string queueName, bool noAck)
            : base(model, queueName, noAck)
        {
        }

        protected ObservableSubscription(IModel model, string queueName, bool noAck, string consumerTag)
            : base(model, queueName, noAck, consumerTag)
        {
        }

        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return Subject.Subscribe(observer);
        }
    }
}
