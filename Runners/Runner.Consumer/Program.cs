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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RabbitRx.Queue;


namespace Runner.Consumer
{
    class Program
    {
        static readonly ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
        static readonly IConnection connection = factory.CreateConnection();
        
        static QueueSettings _queueSettings;
        static string exchangeName = "testExchange";
        static string queueName = "testQueue";

        static void Main(string[] args)
        {
            _queueSettings = new QueueSettings { Name = queueName, NoAck = true };

            Start();

        }

        private static CancellationTokenSource tokenSource;

        private static void Start()
        {
            tokenSource = new CancellationTokenSource();
            //_queueSettings.ConsumerName = string.Format("test-{0}", Guid.NewGuid());

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
            var consumer = new ObservableSubscription<string>(connection);
            var stream = consumer.Consume(_queueSettings);

            stream.Subscribe(message =>
                    {
                        Console.WriteLine("Received (Subscription 1): {0}", message.Body);
                        Thread.Sleep(100); //Simulate slow
                    }, () => { }, tokenSource.Token);

            stream.Subscribe(message =>
            {
                Console.WriteLine("Received (Subscription 2): {0}", message.Body);
                Thread.Sleep(200); //Simulate slow
            }, () => { }, tokenSource.Token);

        }
    }
}
