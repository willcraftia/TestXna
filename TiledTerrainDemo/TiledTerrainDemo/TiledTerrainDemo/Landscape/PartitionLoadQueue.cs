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
        #region PartitionInfo

        struct PartitionInfo : IComparable<PartitionInfo>
        {
            public Partition Partition;

            public float EyeDistanceSquared;

            public int CompareTo(PartitionInfo other)
            {
                // Descending order.
                return -EyeDistanceSquared.CompareTo(other.EyeDistanceSquared);
            }
        }

        #endregion

        #region PartitionInThread

        class PartitionInThread
        {
            public Partition Partition { get; set; }
        }

        #endregion

        public const int MaxThreadCount = 20;

        public const int DefaultThreadCount = 4;

        PartitionLoadResultCallback loadResultCallback;

        List<PartitionInfo> requestQueue = new List<PartitionInfo>();

        Queue<PartitionInThread> freeThreads;

        Queue<Partition> resultQueue = new Queue<Partition>();

        public int ThreadCount { get; private set;}

        public int RequestQueueCount
        {
            get { return requestQueue.Count; }
        }

        public int FreeThreadCount
        {
            get { lock (freeThreads) return freeThreads.Count; }
        }

        public PartitionLoadQueue(PartitionLoadResultCallback loadResultCallback)
            : this(loadResultCallback, DefaultThreadCount)
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
            requestQueue.Add(new PartitionInfo { Partition = partition });
        }

        public void Update(Vector3 eyePosition)
        {
            ProcessQueue(ref eyePosition);
            ProcessResult();
        }

        void ProcessQueue(ref Vector3 eyePosition)
        {
            // process one partition per update.

            // Pre-check
            if (requestQueue.Count == 0) return;

            // Check a state on each partitions in queue.
            int index = 0;
            while (index < requestQueue.Count)
            {
                if (requestQueue[index].Partition.LoadState != PartitionLoadState.WaitLoad)
                {
                    // Remove a canceled partition from queue.
                    requestQueue.RemoveAt(index);
                }
                else
                {
                    // Calculate the eye distance of this partition.
                    var p = requestQueue[index].Partition;
                    var eyeDistanceSquared = p.CalculateEyeDistanceSquared(ref eyePosition);

                    requestQueue[index] = new PartitionInfo
                    {
                        Partition = p,
                        EyeDistanceSquared = eyeDistanceSquared
                    };

                    // Next.
                    index++;
                }
            }

            // Re-check
            if (requestQueue.Count == 0) return;

            // Sort in descending order.
            requestQueue.Sort();

            PartitionInThread freeThread;
            lock (freeThreads)
            {
                // No free thread exists.
                if (freeThreads.Count == 0) return;

                // Get a free thread.
                freeThread = freeThreads.Dequeue();
            }

            // Dequeue a partition and assign it into the free thread.
            int lastIndex = requestQueue.Count - 1;
            freeThread.Partition = requestQueue[lastIndex].Partition;
            requestQueue.RemoveAt(lastIndex);
            
            // mark.
            freeThread.Partition.LoadState = PartitionLoadState.Loading;

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
