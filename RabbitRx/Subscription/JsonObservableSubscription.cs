using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRx.Message;

namespace RabbitRx.Subscription
{
    public interface IObservableSubscription<out T> : IObservable<T>
    {
        Task Start(CancellationToken token, int? timeout = null, Action onQueueEmpty = null);
        IModel Model { get; }
        string QueueName { get; }
    }

    public class JsonObservableSubscription<T> : SubscriptionConsumer, IObservableSubscription<RabbitMessage<T>>
    {
        public JsonObservableSubscription(IModel model, string queueName)
            : base(model, queueName)
        {
        }

        public JsonObservableSubscription(IModel model, string queueName, bool noAck)
            : base(model, queueName, noAck)
        {
        }

        public JsonObservableSubscription(IModel model, string queueName, bool noAck, string consumerTag)
            : base(model, queueName, noAck, consumerTag)
        {
        }

        private RabbitMessage<T> Convert(BasicDeliverEventArgs value)
        {
            var jsonStr = Encoding.UTF8.GetString(value.Body);

            var payload = JsonConvert.DeserializeObject<T>(jsonStr);

            var message = new RabbitMessage<T>(value, payload);

            return message;
        }

        public IDisposable Subscribe(IObserver<RabbitMessage<T>> observer)
        {
            return Subject.Select(Convert).Subscribe(observer);
        }
    }
}
