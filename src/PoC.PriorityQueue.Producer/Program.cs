using System;
using System.Globalization;
using System.Threading.Tasks;
using MassTransit;

namespace PoC.PriorityQueue.Producer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("localhost");

                //cfg.ReceiveEndpoint("test-priority_queue", cep =>
                //{
                //    cep.EnablePriority(10);
                //});

            });
            
            // Important! The bus must be started before using it!
            await busControl.StartAsync();
            try
            {
                var rnd = new Random((int) DateTime.UtcNow.Ticks);

                do
                {
                    string value = await Task.Run(() =>
                    {
                        Console.WriteLine("Enter number of message (or quit to exit)");
                        Console.Write("> ");
                        return Console.ReadLine();
                    });

                    if ("quit".Equals(value, StringComparison.OrdinalIgnoreCase))
                        break;

                    int numberOfMessages = int.TryParse(value, out numberOfMessages)
                        ? numberOfMessages
                        : 0;

                    int priority = 1;
                    for (int i = 0; i < numberOfMessages; i++)
                    {
                        //priority = priority >= 10 ? 1 : priority + 1;
                        priority = rnd.Next(1,10);
                        await busControl.Publish<IValueEntered>(new
                            {
                                Value = i,
                                Priority = priority
                            },
                            x =>
                            {
                                x.SetPriority((byte) priority);
                                
                            });
                    }

                }
                while (true);
            }
            finally
            {
                await busControl.StopAsync();
            }
        }
    }
}
