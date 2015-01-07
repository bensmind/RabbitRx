using System;
using RabbitMQ.Client;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var exchangeName = "testExchange";
                channel.ExchangeDeclare(exchangeName, "fanout");
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, exchangeName, "");

                var consumer = new ObservableBasicConsumer(channel);

                var observable = consumer.Consume<string>(queueName, false, "testConsumer");

                observable.Subscribe(message => Console.WriteLine("Received: {0}", message.Body));

                Console.ReadLine();
            }
        }
    }
}
