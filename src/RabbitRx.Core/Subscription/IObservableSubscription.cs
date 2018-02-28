using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbitRx.Core.Subscription
{
    public interface IObservableSubscription<out T> : IObservable<T>
    {
        Task Start(CancellationToken token, int? timeout = null, Action onQueueEmpty = null);
        IModel Model { get; }
        string QueueName { get; }
    }
}