using RabbitMQ.Client.Events;

namespace RabbitRx.Message
{
    public class RabbitMessage<T> : BasicDeliverEventArgs
    {
        public RabbitMessage(BasicDeliverEventArgs eventArgs, T payload)
        {
            ConsumerTag = eventArgs.ConsumerTag;
            DeliveryTag = eventArgs.DeliveryTag;
            Redelivered = eventArgs.Redelivered;
            Exchange = eventArgs.Exchange;
            RoutingKey = eventArgs.RoutingKey;
            BasicProperties = eventArgs.BasicProperties;
            Body = eventArgs.Body;
            Payload = payload;
        }

        public T Payload { get; private set; }
    }
}
