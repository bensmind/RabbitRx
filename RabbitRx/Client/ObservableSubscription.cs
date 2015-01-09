using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using RabbitRx.Message;
using RabbitRx.Queue;

namespace RabbitRx.Client
{
    public class ObservableSubscription<TData>
    {
        private ushort PrefetchCount { get; set; }

        public ObservableSubscription(IConnection connection)
        {
            Connection = connection;
            PrefetchCount = 100;
        }

        public IConnection Connection { get; private set; }
              
        public virtual IObservable<IRabbitMessage<TData>> Consume(QueueSettings settings)
        {
            return Observable.Create<IRabbitMessage<TData>>((observer, token) => Task.Run(() =>
            {
                var model = Connection.CreateModel();
                model.BasicQos(0, PrefetchCount, false);

                var subscription = new Subscription(model, settings.Name, settings.NoAck);

                while (!token.IsCancellationRequested)
                {
                    var evt = subscription.Next();

                    subscription.Ack();

                    var next = new JsonRabbitMessage<TData>(evt);

                    observer.OnNext(next);
                }

                subscription.Close();
                model.Dispose();
                
            }, token));
        }
    }
}
