using System;
using RabbitMQ.Client;

namespace Runner.Common
{
    public class TestQueueConfiguration : IDisposable
    {
        public ConnectionFactory Factory { get; private set; }
        public IConnection Connection { get; private set; } 
        public IModel Channel { get; private set; }

        public string ExchangeName
        {
            get { return RabbitMqSettings.Default.ExchangeName; }
        }

        public string QueueName
        {
            get { return RabbitMqSettings.Default.QueueName; }
        }

        private TestQueueConfiguration()
        {
            
        }

        public static TestQueueConfiguration BuildQueue()
        {
            var testQueueConfiguration = new TestQueueConfiguration
            {
                Factory = new ConnectionFactory
                {
                    HostName = RabbitMqSettings.Default.HostName
                }
            };


            testQueueConfiguration.Connection = testQueueConfiguration.Factory.CreateConnection();
            testQueueConfiguration.Channel = testQueueConfiguration.Connection.CreateModel();

            testQueueConfiguration.Channel.ExchangeDeclare(RabbitMqSettings.Default.ExchangeName, "fanout");
            testQueueConfiguration.Channel.QueueDeclare(RabbitMqSettings.Default.QueueName, false, false, false, null);
            testQueueConfiguration.Channel.QueueBind(RabbitMqSettings.Default.QueueName, RabbitMqSettings.Default.ExchangeName, "");

            return testQueueConfiguration;
        }

        public static TestQueueConfiguration BuildFanoutExchangeQueue()
        {
            var testQueueConfiguration = new TestQueueConfiguration
            {
                Factory = new ConnectionFactory
                {
                    HostName = RabbitMqSettings.Default.HostName
                }
            };


            testQueueConfiguration.Connection = testQueueConfiguration.Factory.CreateConnection();
            testQueueConfiguration.Channel = testQueueConfiguration.Connection.CreateModel();

            testQueueConfiguration.Channel.ExchangeDeclare(RabbitMqSettings.Default.ExchangeName, "fanout");
            testQueueConfiguration.Channel.QueueDeclare(RabbitMqSettings.Default.QueueName, false, false, false, null);
            testQueueConfiguration.Channel.QueueBind(RabbitMqSettings.Default.QueueName, RabbitMqSettings.Default.ExchangeName, "");

            return testQueueConfiguration;
        }

        public void Dispose()
        {
            Connection?.Dispose();
            Channel?.Dispose();
        }
    }
}
