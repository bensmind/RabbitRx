using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRx.Message;
using RabbitRx.Subscription.Base;

namespace RabbitRx.Subscription
{
    public class JsonObservableSubscription<T> : ObservableSubscriptionBase<JsonRabbitMessage<T>>
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
            var message = new JsonRabbitMessage<T>(value);
            Subject.OnNext(message);
        }
    }
}
