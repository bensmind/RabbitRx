using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RabbitMQ.Client.MessagePatterns;
using RabbitRx.Subscription;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RabbitRx.Message;
using RabbitRx.Queue;


namespace Runner.Consumer
{
    class Program
    {
        static readonly ConnectionFactory Factory = new ConnectionFactory { HostName = "localhost" };
        static readonly IConnection Connection = Factory.CreateConnection();

        private const string QueueName = "testQueue";

        static void Main(string[] args)
        {
            Start();
        }

        private static CancellationTokenSource _tokenSource;

        private static void Start()
        {
            _tokenSource = new CancellationTokenSource();

            Console.WriteLine("Rabbit Consumer: Press Enter to Start");
            Console.ReadLine();
            Task.Run(() => Consume());
            //Task.Run(() => Subscribe());
            Console.WriteLine("Press Any Key to Stop");
            Console.ReadLine();
            _tokenSource.Cancel();
            Start();
        }

        static readonly Random Rand = new Random();

        static void Consume()
        {
            var model = Connection.CreateModel();

            model.BasicQos(0, 50, false);

            var consumer = new JsonObservableSubscription<string>(model, QueueName, false);
            
            consumer.Subscribe(message =>
            {
                Console.WriteLine("Received (Thread {1}): {0}", message.Payload, Thread.CurrentThread.GetHashCode());
                consumer.Ack(message);
                Thread.Sleep(Rand.Next(150)); //Simulate slow
            }, _tokenSource.Token);

            var stream1 = consumer.Start(_tokenSource.Token);
            var stream2 = consumer.Start(_tokenSource.Token);

            Task.WhenAll(stream1, stream2).ContinueWith(t =>
            {
                consumer.Close();
                model.Dispose();
            });
        }
    }
}
