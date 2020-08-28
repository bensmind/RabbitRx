using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Topshelf;

namespace PoC.PriorityQueue.Consumer
{
    public class Program
    {
        public static int Main(string[] args)
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
                cfg.PrefetchCount = 2;
                cfg.EnablePriority(10);

                cfg.ReceiveEndpoint("event_queue", e =>
                {
                    e.Handler<IValueEntered>(context =>
                        Console.Out.WriteLineAsync($"(Priority) Value: ({context.Message.Priority}) {context.Message.Value}"));
                });
            });
        }
    }
}
