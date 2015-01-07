using System;
using System.Reactive.Linq;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Runner
{
    public class ObservableBasicConsumer : EventingBasicConsumer
    {
        public ObservableBasicConsumer(IModel model)
            : base(model){}

        public IObservable<IRabbitMessage<T>> Consume<T>(string queueName, bool noAck, string consumerName)
        {
            var consumerTag = Model.BasicConsume(queueName, noAck, consumerName, this);

            var fromEvent = Observable.FromEventPattern<BasicDeliverEventHandler, BasicDeliverEventArgs>(
                ev => Received += ev,
                ev => Received -= ev);

            var observable = fromEvent.Select(e => new JsonRabbitMessage<T>(e.EventArgs));

            return observable;
        }

        private class JsonRabbitMessage<TBody> : IRabbitMessage<TBody>
        {
            public JsonRabbitMessage(BasicDeliverEventArgs args)
            {
                ConsumerTag = args.ConsumerTag;
                DeliveryTag = args.DeliveryTag;
                Redelivered = args.Redelivered;
                Exchange = args.Exchange;
                RoutingKey = args.RoutingKey;
                Properties = args.BasicProperties;

                Body = Convert(args.Body);
            }

            public string ConsumerTag { get; private set; }
            public ulong DeliveryTag { get; private set; }
            public bool Redelivered { get; private set; }
            public string Exchange { get; private set; }
            public string RoutingKey { get; private set; }
            public IBasicProperties Properties { get; private set; }
            public TBody Body { get; private set; }

            public TBody Convert(byte[] body)
            {
                var jsonStr = Encoding.UTF8.GetString(body);

                var deserialized = JsonConvert.DeserializeObject<TBody>(jsonStr);

                return deserialized;
            }
        }
    }


    public interface IRabbitMessage<out T>
    {
        string ConsumerTag { get; }
        ulong DeliveryTag { get; }
        bool Redelivered { get; }
        string Exchange { get; }
        string RoutingKey { get; }
        IBasicProperties Properties { get; }
        T Body { get; }

        T Convert(byte[] body);
    }


}
