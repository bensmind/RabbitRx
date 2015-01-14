using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRx.Subscription.Base;

namespace RabbitRx.Subscription
{
    public class ObservableSubscription : ObservableSubscriptionBase<BasicDeliverEventArgs>
    {
        public ObservableSubscription(IModel model, string queueName)
            : base(model, queueName)
        {
        }

        public ObservableSubscription(IModel model, string queueName, bool noAck)
            : base(model, queueName, noAck)
        {
        }

        public ObservableSubscription(IModel model, string queueName, bool noAck, string consumerTag)
            : base(model, queueName, noAck, consumerTag)
        {
        }

        public override void OnNext(BasicDeliverEventArgs value)
        {
            Subject.OnNext(value);
        }

        public override IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return Subject.Subscribe(observer);
        }
    }
}
