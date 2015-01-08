using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RabbitRx.Client;

namespace Runner.Consumer
{
    class Program
    {
        static readonly ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
        static readonly IConnection connection = factory.CreateConnection();
        static readonly IModel channel = connection.CreateModel();
        static QueueSettings _queueSettings;
        static string exchangeName = "testExchange";
        static string queueName = "testQueue";

        static void Main(string[] args)
        {
            //channel.ExchangeDeclare(exchangeName, "fanout");
            //channel.QueueDeclare(queueName, false, false, true, null);
            //channel.QueueBind(queueName, exchangeName, "");

            _queueSettings = new QueueSettings { Name = queueName, NoAck = false };

            Start();

        }

        private static CancellationTokenSource tokenSource;

        private static void Start()
        {
            tokenSource = new CancellationTokenSource();
            _queueSettings.ConsumerName = string.Format("test-{0}", Guid.NewGuid());

            Console.WriteLine("Rabbit Consumer: Press Enter to Start");
            Console.ReadLine();
            Task.Run(() => Consume());
            Console.WriteLine("Press Any Key to Stop");
            Console.ReadLine();
            tokenSource.Cancel();
            Start();
        }


        static void Consume()
        {
            var consumer = new ObservableConsumer<string>(channel, _queueSettings);

            consumer.Subscribe(message => Console.WriteLine("Received: {0}", message.Body),tokenSource.Token);
        }
    }
}
