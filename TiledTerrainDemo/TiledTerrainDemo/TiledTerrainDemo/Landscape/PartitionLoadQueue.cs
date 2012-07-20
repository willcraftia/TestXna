#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Landscape
{
    public sealed class PartitionLoadQueue
    {
        #region PartitionInThread

        class PartitionInThread
        {
            public Partition Partition { get; set; }
        }

        #endregion

        /// <summary>
        /// 利用できる Thread の上限。
        /// </summary>
        public const int MaxThreadCount = 1;

        PartitionLoadResultCallback loadResultCallback;

        Queue<Partition> queue = new Queue<Partition>();

        Queue<PartitionInThread> freeThreads;

        Queue<Partition> resultQueue = new Queue<Partition>();

        public int ThreadCount { get; private set;}

        public PartitionLoadQueue(PartitionLoadResultCallback loadResultCallback)
            : this(loadResultCallback, MaxThreadCount)
        {
        }

        public PartitionLoadQueue(PartitionLoadResultCallback loadResultCallback, int threadCount)
        {
            if (loadResultCallback == null) throw new ArgumentNullException("loadResultCallback");
            if (threadCount < 1 || MaxThreadCount < threadCount) throw new ArgumentOutOfRangeException("threadCount");

            this.loadResultCallback = loadResultCallback;
            ThreadCount = threadCount;

            freeThreads = new Queue<PartitionInThread>(threadCount);
            for (int i = 0; i < threadCount; i++)
                freeThreads.Enqueue(new PartitionInThread());
        }

        public void Enqueue(Partition partition)
        {
            queue.Enqueue(partition);
        }

        public void Update()
        {
            ProcessQueue();
            ProcessResult();
        }

        Partition Dequeue()
        {
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                if (item.LoadState == PartitionLoadState.WaitLoad)
                    return item;
            }

            return null;
        }

        void ProcessQueue()
        {
            // process one partition per update.

            // Pre-check
            if (queue.Count == 0) return;

            Partition partition = null;
            PartitionInThread freeThread;
            lock (freeThreads)
            {
                // No free thread exists.
                if (freeThreads.Count == 0) return;

                // Try to get a partition that should be loaded.
                partition = Dequeue();

                // No loadable partition exists.
                if (partition == null) return;

                // Get a free thread.
                freeThread = freeThreads.Dequeue();
            }

            // assign the partition into the free thread.
            freeThread.Partition = partition;
            // mark.
            partition.LoadState = PartitionLoadState.Loading;

            // assign a real thread.
            ThreadPool.QueueUserWorkItem(WaitCallback, freeThread);
        }

        void ProcessResult()
        {
            // process one result per update.

            Partition result;
            lock (resultQueue)
            {
                // No results.
                if (resultQueue.Count == 0) return;

                result = resultQueue.Dequeue();
            }

            // callback.
            loadResultCallback(result);
        }

        void WaitCallback(object state)
        {
            var partitionInThread = state as PartitionInThread;

            partitionInThread.Partition.LoadContent();

            lock (resultQueue)
                resultQueue.Enqueue(partitionInThread.Partition);

            // free this slot.
            partitionInThread.Partition = null;
            lock (freeThreads)
                freeThreads.Enqueue(partitionInThread);
        }
    }
}
