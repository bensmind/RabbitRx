using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Runner.Producer
{
    class Program
    {
        static readonly ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
        static readonly IConnection connection = factory.CreateConnection();
        static readonly IModel channel = connection.CreateModel();
        static string exchangeName = "testExchange";
        static string queueName = "testQueue";

        static void Main(string[] args)
        {
            channel.ExchangeDeclare(exchangeName, "fanout");
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, "");

            Start();
        }

        private static CancellationTokenSource tokenSource;

        private static void Start()
        {
            tokenSource = new CancellationTokenSource();

            Console.WriteLine("Rabbit Producer: Press Enter to Start");
            Console.ReadLine();
            Task.Run(() => Produce());
            Console.WriteLine("Press Any Key to Stop");
            Console.ReadLine();
            tokenSource.Cancel();
            Start();
        }

        private static void Produce()
        {
            var rand = new Random();
            var settings = new BasicProperties()
            {
                ContentType = "text/plain",
                DeliveryMode = 1 //1)not durable, 2)durable
            };

            var ob = Observable.Generate(rand.Next(), i => !tokenSource.IsCancellationRequested, i => rand.Next(), i => i, x => TimeSpan.FromMilliseconds(rand.Next(50)));

            ob.Subscribe(num =>
            {
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(num));
                channel.BasicPublish(exchangeName, "", settings, bytes);
                Console.WriteLine("Published: {0}", num);

            }, tokenSource.Token);

        }
    }
}
