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
        string ConsumerTag { get;  }
        ulong DeliveryTag { get;  }
        bool Redelivered { get;  }
        string Exchange { get; }
        string RoutingKey { get; }
        IBasicProperties Properties { get;}
        byte[] RawBody { get; }
        T Body { get; }
    }
}
