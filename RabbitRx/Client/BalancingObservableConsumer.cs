using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitRx.Message;

namespace RabbitRx.Client
{
    public class BalancingObservableConsumer<TData> : ObservableConsumer<TData>
    {
        public BalancingObservableConsumer(IModel model, QueueSettings queueSettings)
            : base(model, queueSettings)
        {
        }

        protected readonly ConcurrentQueue<IRabbitMessage<TData>> Queue = new ConcurrentQueue<IRabbitMessage<TData>>();

        public override IDisposable Subscribe(IObserver<IRabbitMessage<TData>> observer)
        {
            var subscription = base.Subscribe(observer);

            var options = new ParallelOptions();

            ParallelExt.While(options, () => IsRunning, Body);

            return subscription;
        }

        private void Body(ParallelLoopState parallelLoopState)
        {
            IRabbitMessage<TData> message;
            if (Queue.TryDequeue(out message))
            {
                Publish(message);
            }
        }

        public override void OnNext(IRabbitMessage<TData> value)
        {
            Queue.Enqueue(value);
        }
    }

    public class ParallelExt
    {
        public static void While(ParallelOptions parallelOptions, Func<bool> condition, Action<ParallelLoopState> body)
        {
            Parallel.ForEach(new InfinitePartitioner(), parallelOptions,
                (ignored, loopState) =>
                {
                    if (condition()) body(loopState);
                    else loopState.Stop();
                });
        }
    }


    public class InfinitePartitioner : Partitioner<bool>
    {
        public override IList<IEnumerator<bool>> GetPartitions(int partitionCount)
        {
            if (partitionCount < 1)
                throw new ArgumentOutOfRangeException("partitionCount");
            return (from i in Enumerable.Range(0, partitionCount)
                    select InfiniteEnumerator()).ToArray();
        }

        public override bool SupportsDynamicPartitions { get { return true; } }

        public override IEnumerable<bool> GetDynamicPartitions()
        {
            return new InfiniteEnumerators();
        }

        private static IEnumerator<bool> InfiniteEnumerator()
        {
            while (true) yield return true;
        }

        private class InfiniteEnumerators : IEnumerable<bool>
        {
            public IEnumerator<bool> GetEnumerator()
            {
                return InfiniteEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }
    }
}
