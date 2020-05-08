using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitRx;
using RabbitRx.Formatters;
using RabbitRx.Message;
using RabbitRx.Subscription;
using Runner.Common;

namespace Runner.Consumer
{
    public static class Program
    {

        private static readonly TestQueueConfiguration TestQueueConfig = TestQueueConfiguration.BuildQueue();

        public static void Main(string[] args)
        {
            Start();
            TestQueueConfig.Dispose();
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

        private static void Consume()
        {
            var model = TestQueueConfig.Connection.CreateModel();

            model.BasicQos(0, 50, false);

            var consumer = new JsonObservableSubscription<string>(model, TestQueueConfig.QueueName, false);

            _tokenSource.Token.Register(consumer.Close);

            consumer.Subscribe(message =>
                {
                    Console.WriteLine($"Received (Thread {Thread.CurrentThread.GetHashCode()}): {message.Payload}");
                    consumer.Ack(message);

                    Thread.Sleep(Rand.Next(2001));

                },
                exception =>
                {
                    ConsoleWriteFormatter.WriteLine(exception.ToString(), ConsoleColor.DarkBlue,
                        ConsoleColor.Red);
                },
                () =>
                {
                    ConsoleWriteFormatter.WriteLine("-== Completed ==-", ConsoleColor.White,
                        ConsoleColor.Blue);
                },
                _tokenSource.Token);

            var stream1 = consumer.Start(_tokenSource.Token);
            var stream2 = consumer.Start(_tokenSource.Token);

            Task.WhenAll(stream1, stream2).ContinueWith(t =>
            {
                Console.WriteLine();
                ConsoleWriteFormatter.WriteLine("-== Closing Queue ==-", ConsoleColor.Red, ConsoleColor.White);
                Console.WriteLine();

                model.Dispose();
            });
        }

        private static void ConsumeThrottle()
        {
            var model = TestQueueConfig.Connection.CreateModel();

            model.BasicQos(0, 50, false);

            var consumer = new JsonObservableSubscription<string>(model, TestQueueConfig.QueueName, false);

            var throttlingConsumer = new ThrottlingConsumer<RabbitMessage<string>>(subscription: consumer,maxTasks: 64, minTasks: 2);

            throttlingConsumer.Subscribe(onNext: message =>
                {

                    Console.WriteLine($"Received (Thread {Thread.CurrentThread.GetHashCode()}): {message.Payload}");
                    consumer.Ack(message);

                    Thread.Sleep(Rand.Next(2501));

                }, onError: exception =>
                {
                    ConsoleWriteFormatter.WriteLine(exception.ToString(),
                        ConsoleColor.DarkBlue,
                        ConsoleColor.Red);
                },
                onCompleted: () =>
                {
                    ConsoleWriteFormatter.WriteLine("-== Completed ==-", ConsoleColor.White,
                        ConsoleColor.Blue);
                }

                , token: _tokenSource.Token);

            var start = throttlingConsumer.Start(_tokenSource.Token, TimeSpan.FromSeconds(5));

            start.ContinueWith(t =>
            {
                Console.WriteLine();
                ConsoleWriteFormatter.WriteLine("-== Closing Queue ==-", ConsoleColor.Red, ConsoleColor.White);
                Console.WriteLine();

                consumer.Close();
                model.Dispose();
            });
        }

    }
}
