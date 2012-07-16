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
        #region ThreadResult

        struct ThreadResult
        {
            public Partition Partition;

            public Exception Exception;
        }

        #endregion

        #region PartitionInThread

        class PartitionInThread
        {
            public Partition Partition { get; set; }
        }

        #endregion

        /// <summary>
        /// 利用できる Thread の上限。
        /// </summary>
        public const int MaxThreadCount = 3;

        PartitionLoadResultCallback loadResultCallback;

        Queue<Partition> queue = new Queue<Partition>();

        Queue<PartitionInThread> freeThreads;

        Queue<ThreadResult> resultQueue = new Queue<ThreadResult>();

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

        void ProcessQueue()
        {
            // process one partition per update.

            Partition partition = null;
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                if (item.LoadState == PartitionLoadState.WaitLoad)
                {
                    // load a partition with WaitLoad state.
                    partition = item;
                    break;
                }
            }

            // No partition that should be loaded exists.
            if (partition == null) return;

            PartitionInThread freeThread;
            lock (freeThreads)
            {
                // No free thread exists.
                if (freeThreads.Count == 0)
                {
                    // enqueue again.
                    queue.Enqueue(partition);
                    return;
                }

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

            ThreadResult result;
            lock (resultQueue)
            {
                // No results.
                if (resultQueue.Count == 0) return;

                result = resultQueue.Dequeue();
            }

            // callback.
            loadResultCallback(result.Partition, result.Exception);
        }

        void WaitCallback(object state)
        {
            var partitionInThread = state as PartitionInThread;

            // load.
            var result = new ThreadResult
            {
                Partition = partitionInThread.Partition
            };
            try
            {
                partitionInThread.Partition.LoadContent();
            }
            catch (Exception e)
            {
                result.Exception = e;
            }

            lock (resultQueue)
                resultQueue.Enqueue(result);

            // free this slot.
            partitionInThread.Partition = null;
            lock (freeThreads)
                freeThreads.Enqueue(partitionInThread);
        }
    }
}
