using System;
using PoC.MyMassTransit.Common;
using MassTransit;
using Topshelf;

namespace PoC.MyMassTransit.Consumer
{
    class Program
    {
        public static int Main()
        {
            return (int)HostFactory.Run(cfg => cfg.Service(x => new EventConsumerService()));
        }
    }

    class EventConsumerService : ServiceControl
    {
        IBusControl _bus;

        public bool Start(HostControl hostControl)
        {
            _bus = ConfigureBus();
            _bus.Start();

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _bus?.Stop(TimeSpan.FromSeconds(30));

            return true;
        }

        IBusControl ConfigureBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("localhost");

                cfg.ReceiveEndpoint("event_queue", e =>
                {
                    e.Handler<IValueEntered>(context =>
                        Console.Out.WriteLineAsync($"Value was entered: {context.Message.Value}"));
                });
            });
        }
    }

   
}
