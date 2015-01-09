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
using RabbitRx.Message;
using RabbitRx.Queue;

namespace RabbitRx.Client
{
    public class ObservableQueueingConsumer<TData> : IBasicConsumer, IDisposable
    {
        public ObservableQueueingConsumer(IModel model)
        {
            Model = model;
        }       

        public IModel Model { get; private set; }
              
        public event ConsumerCancelledEventHandler ConsumerCancelled;

        public virtual IObservable<IRabbitMessage<TData>> Consume(QueueSettings settings)
        {
            Model.BasicQos(0, 100, false);
            Model.BasicConsume(settings.Name, settings.NoAck, settings.ConsumerName, this);

            return Observable.Create<IRabbitMessage<TData>>((observer, token) => Work(observer, token, settings));
        }

        protected virtual Task<IDisposable> Work(IObserver<IRabbitMessage<TData>> observer, CancellationToken token, QueueSettings settings)
        {
            Task.Run(() =>
            {

                
            }, token);


            return Unsubscribe(settings.ConsumerName);
        }

        protected Task<IDisposable> Unsubscribe(string consumerName)
        {
            var unsubscribe = Disposable.Create(() =>
            {
                Debug.WriteLine("Observer has unsubscribed");
                Model.BasicCancel(consumerName);
            });

            return Task.FromResult(unsubscribe);
        }


        public void HandleBasicConsumeOk(string consumerTag)
        {
            
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            Cancel(consumerTag);
        }

        public void HandleBasicCancel(string consumerTag)
        {
            Cancel(consumerTag);
        }

        public void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            //TODO: Something
        }

        private void Cancel(string consumerTag)
        {
            if (ConsumerCancelled != null)
            {
                ConsumerCancelled(this, new ConsumerEventArgs(consumerTag));
            }
        }

        public void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
            IBasicProperties properties, byte[] body)
        {
            var message = new JsonRabbitMessage<TData>
            {
                ConsumerTag = consumerTag,
                DeliveryTag = deliveryTag,
                Redelivered = redelivered,
                Exchange = exchange,
                RoutingKey = routingKey,
                Properties = properties,
                RawBody = body
            };

            OnNext(message);
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
