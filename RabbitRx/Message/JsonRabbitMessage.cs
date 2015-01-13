using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitRx.Message
{
    public class JsonRabbitMessage<T> : BasicDeliverEventArgs, IRabbitMessage<T>
    {
        public JsonRabbitMessage(){}

        public JsonRabbitMessage(BasicDeliverEventArgs eventArgs)
        {
            ConsumerTag = eventArgs.ConsumerTag;
            DeliveryTag = eventArgs.DeliveryTag;
            Redelivered = eventArgs.Redelivered;
            Exchange = eventArgs.Exchange;
            RoutingKey = eventArgs.RoutingKey;
            BasicProperties = eventArgs.BasicProperties;
            Body = eventArgs.Body;

        }

        private T _payload;
        public T Payload
        {
            get
            {
                if (_payload == null)
                {
                    var jsonStr = Encoding.UTF8.GetString(Body);

                    _payload = JsonConvert.DeserializeObject<T>(jsonStr); ;
                }
                return _payload;
            }
        }
    }
}
