using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Runner.Common;

namespace Runner.Producer
{
    public static class Program
    {
        
        public static void Main(string[] args)
        {
            using (var randomIntProducer = new RandomIntProducer())
            {
                randomIntProducer.Start();
            }
        }
    }
}
