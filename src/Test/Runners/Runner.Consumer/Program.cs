using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitRx.Advanced.Subscription;
using RabbitRx.Core.Message;
using RabbitRx.Json.Subscription;

namespace Runner.Consumer
{
    public static class Program
    {
        private static readonly ConnectionFactory Factory = new ConnectionFactory { HostName = "localhost" };
        private static readonly IConnection Connection = Factory.CreateConnection();

        private const string QueueName = "testQueue";

        public static void Main(string[] args)
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
            //Task.Run(() => Consume());
            Console.WriteLine("Press Any Key to Stop");
            Console.ReadLine();
            _tokenSource.Cancel();
            Start();
        }

        private static readonly Random Rand = new Random();

        public static void Consume()
        {
            var model = Connection.CreateModel();

            model.BasicQos(0, 50, false);

            var consumer = new JsonObservableSubscription<string>(model, QueueName, false);

            _tokenSource.Token.Register(consumer.Close);

            consumer.Subscribe(message =>
            {
                Console.WriteLine($"Received (Thread {Thread.CurrentThread.GetHashCode()}): {message.Payload}");
                consumer.Ack(message);

                //Task.Delay(TimeSpan.FromSeconds(Rand.Next(15))); //.Wait(); //Simulate slow
                Thread.Sleep(Rand.Next(2001));

            }, _tokenSource.Token);

            var stream1 = consumer.Start(_tokenSource.Token);
            var stream2 = consumer.Start(_tokenSource.Token);

            Task.WhenAll(stream1, stream2).ContinueWith(t => model.Dispose());
        }

        public static void ConsumeThrottle()
        {
            var model = Connection.CreateModel();

            model.BasicQos(0, 50, false);

            var consumer = new JsonObservableSubscription<string>(model, QueueName, false);

            var throttlingConsumer = new ThrottlingConsumer<RabbitMessage<string>>(consumer, 64);

            throttlingConsumer.Subscribe(onNext: message =>
            {

                Console.WriteLine($"Received (Thread {Thread.CurrentThread.GetHashCode()}): {message.Payload}");
                consumer.Ack(message);

                //await Task.Delay(TimeSpan.FromSeconds(Rand.Next(60))).ConfigureAwait(false); //Simulate slow
                Thread.Sleep(Rand.Next(5001));

            }, token: _tokenSource.Token);

            var start = throttlingConsumer.Start(_tokenSource.Token, TimeSpan.FromSeconds(5));

            start.ContinueWith(t =>
            {
                consumer.Close();
                model.Dispose();
            });
        }

    }
}
