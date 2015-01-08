using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbitRx.Message
{
    public interface IRabbitMessage<out T>
    {
        string ConsumerTag { get; set; }
        ulong DeliveryTag { get; set; }
        bool Redelivered { get; set; }
        string Exchange { get; set; }
        string RoutingKey { get; set; }
        IBasicProperties Properties { get; set; }
        byte[] RawBody { get; set; }
        T Body { get; }
    }
}
