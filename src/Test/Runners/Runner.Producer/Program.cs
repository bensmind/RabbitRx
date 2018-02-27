using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Runner.Producer
{
    public static class Program
    {
        private static readonly ConnectionFactory Factory = new ConnectionFactory { HostName = "localhost" };
        private static readonly IConnection Connection = Factory.CreateConnection();
        private static readonly IModel Channel = Connection.CreateModel();

        private const string ExchangeName = "testExchange";
        private const string QueueName = "testQueue";

        public static void Main(string[] args)
        {
            Channel.ExchangeDeclare(ExchangeName, "fanout");
            Channel.QueueDeclare(QueueName, false, false, false, null);
            Channel.QueueBind(QueueName, ExchangeName, "");

            Start();
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
            var rand = new Random();
            var settings = new BasicProperties()
            {
                ContentType = "text/plain",
                DeliveryMode = 1 //1)not durable, 2)durable
            };

            var ob = Observable.Generate(rand.Next(), 
                i => !_tokenSource.IsCancellationRequested, 
                i => rand.Next(), 
                i => i, 
                x => TimeSpan.FromMilliseconds(rand.Next(500)));

            ob.Subscribe(num =>
            {
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(num));
                Channel.BasicPublish(ExchangeName, "", settings, bytes);
                Console.WriteLine("Published: {0}", num);

            }, _tokenSource.Token);

        }
    }
}
