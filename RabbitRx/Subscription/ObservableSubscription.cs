using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitRx.Subscription
{
    public class ObservableSubscription : SubscriptionConsumer, IObservable<BasicDeliverEventArgs>
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
