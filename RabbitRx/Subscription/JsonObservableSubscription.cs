using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRx.Message;
using RabbitRx.Subscription.Base;

namespace RabbitRx.Subscription
{
    public class JsonObservableSubscription<T> : ObservableSubscriptionBase<RabbitMessage<T>>
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

        public override void OnNext(BasicDeliverEventArgs value)
        {
            var jsonStr = Encoding.UTF8.GetString(value.Body);

            var payload = JsonConvert.DeserializeObject<T>(jsonStr);

            var message = new RabbitMessage<T>(value, payload);

            Subject.OnNext(message);
        }

        public override IDisposable Subscribe(IObserver<RabbitMessage<T>> observer)
        {
            return Subject.Subscribe(observer);
        }
    }
}
