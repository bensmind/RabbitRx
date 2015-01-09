using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitRx.Queue
{
    public class QueueSettings
    {
        public string Name { get; set; }
        public bool NoAck { get; set; }
        public string ConsumerName { get; set; }
    }
}
