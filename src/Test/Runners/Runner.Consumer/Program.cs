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
using RabbitRx.Core.Message;
using RabbitRx.Core.Subscription;
using RabbitRx.Json.Subscription;
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
            Task.Run(() => ConsumeThrottle());
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

            _tokenSource.Token.Register(consumer.Close);

            consumer.Subscribe(message =>
            {
                Console.WriteLine("Received (Thread {1}): {0}", message.Payload, Thread.CurrentThread.GetHashCode());
                consumer.Ack(message);
                Thread.Sleep(Rand.Next(150)); //Simulate slow
            }, _tokenSource.Token);

            var stream1 = consumer.Start(_tokenSource.Token);
            var stream2 = consumer.Start(_tokenSource.Token);

            Task.WhenAll(stream1, stream2).ContinueWith(t => model.Dispose());
        }

        static void ConsumeThrottle()
        {
            var model = Connection.CreateModel();

            model.BasicQos(0, 50, false);

            var consumer = new JsonObservableSubscription<string>(model, QueueName, false);

            var throttlingConsumer = new ThrottlingConsumer<RabbitMessage<string>>(consumer, 4);

            throttlingConsumer.Subscribe(message =>
            {
                Console.WriteLine("Received (Thread {1}): {0}", message.Payload, Thread.CurrentThread.GetHashCode());
                consumer.Ack(message);
                Thread.Sleep(Rand.Next(1500)); //Simulate slow
            }, _tokenSource.Token);

            var start = throttlingConsumer.Start(_tokenSource.Token, TimeSpan.FromSeconds(5));

            start.ContinueWith(t =>
            {
                consumer.Close();
                model.Dispose();
            });
        }

    }
}
