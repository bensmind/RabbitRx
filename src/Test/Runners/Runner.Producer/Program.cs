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
        private static readonly TestQueueConfiguration TestQueueConfig = TestQueueConfiguration.BuildFanoutExchangeQueue();

        
        public static void Main(string[] args)
        {
            Start();
            TestQueueConfig.Dispose();
        }

        private static CancellationTokenSource _tokenSource;

        private static void Start()
        {
            _tokenSource = new CancellationTokenSource();

            Console.WriteLine("Rabbit Producer: Press Enter to Start");
            Console.ReadLine();
            Task.Run(() => Produce());
            Console.WriteLine("Press Any Key to Stop");
            Console.ReadLine();
            _tokenSource.Cancel();
            Start();
        }

        private static void Produce()
        {
            var rand = new Random((int) DateTime.UtcNow.Ticks);
            var settings = new BasicProperties()
            {
                ContentType = "text/plain",
                DeliveryMode = 1 //1)not durable, 2)durable
            };

            var ob = Observable.Generate(rand.Next(),
                i => !_tokenSource.IsCancellationRequested,
                i => rand.Next(),
                i => i,
                x => TimeSpan.FromMilliseconds(rand.Next(250)));

            ob.Subscribe(num =>
            {
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(num));
                TestQueueConfig?.Channel.BasicPublish(
                    exchange: TestQueueConfig.ExchangeName, 
                    routingKey: "",
                    basicProperties: settings, 
                    body: bytes);
                Console.WriteLine("Published: {0}", num);
            }, _tokenSource.Token);
        }
    }
}
