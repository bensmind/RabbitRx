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
    internal class JsonRabbitMessage<TBody> : IRabbitMessage<TBody>
    {
        public JsonRabbitMessage(){}

        public JsonRabbitMessage(BasicDeliverEventArgs eventArgs)
        {
            ConsumerTag = eventArgs.ConsumerTag;
            DeliveryTag = eventArgs.DeliveryTag;
            Redelivered = eventArgs.Redelivered;
            Exchange = eventArgs.Exchange;
            RoutingKey = eventArgs.RoutingKey;
            Properties = eventArgs.BasicProperties;
            RawBody = eventArgs.Body;

        }

        public string ConsumerTag { get; set; }
        public ulong DeliveryTag { get; set; }
        public bool Redelivered { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public IBasicProperties Properties { get; set; }
        public byte[] RawBody { get; set; }

        private TBody _body;
        public TBody Body
        {
            get
            {
                if (_body == null)
                {
                    var jsonStr = Encoding.UTF8.GetString(RawBody);

                    _body = JsonConvert.DeserializeObject<TBody>(jsonStr); ;
                }
                return _body;
            }
        }
    }
}
