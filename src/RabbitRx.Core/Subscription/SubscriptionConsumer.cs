using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitRx.Core.Subscription
{
    public class SubscriptionConsumer : RabbitMQ.Client.MessagePatterns.Subscription
    {
        protected readonly Subject<BasicDeliverEventArgs> Subject = new Subject<BasicDeliverEventArgs>();

        public SubscriptionConsumer(IModel model, string queueName)
            : base(model, queueName)
        {
        }

        public SubscriptionConsumer(IModel model, string queueName, bool noAck)
            : base(model, queueName, noAck)
        {
        }

        public SubscriptionConsumer(IModel model, string queueName, bool noAck, string consumerTag)
            : base(model, queueName, noAck, consumerTag)
        {
        }

        public virtual Task Start(CancellationToken token, int? timeout = null, Action onQueueEmpty = null)
        {
            var task = new Task(() => Consume(token, timeout, onQueueEmpty), token, TaskCreationOptions.LongRunning);
            task.Start(TaskScheduler.Default);
            return task;
        }

        protected virtual void Consume(CancellationToken token, int? timeout = null, Action onQueueEmpty = null)
        {
            while (true)
            {
                try
                {
                    BasicDeliverEventArgs evt;
                    if (timeout.HasValue)
                    {
                        Next(timeout.Value, out evt);
                    }
                    else
                    {
                        evt = Next(); //Blocking de-queue
                    }
                    
                    if (token.IsCancellationRequested) break;

                    if (evt != null)
                    {
                        Subject.OnNext(evt); //Publish
                    }
                    else if (onQueueEmpty != null)
                    {
                        onQueueEmpty();
                    }
                }
                catch (Exception ex)
                {
                    Subject.OnError(ex);
                }
            }
            if (!Subject.HasObservers)
            {
                Subject.OnCompleted(); //End of stream
            }
        }
    }
}
