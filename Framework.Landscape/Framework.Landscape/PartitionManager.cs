#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class PartitionManager
    {
        Pool<Partition> partitionPool;

        List<Partition> partitions = new List<Partition>();

        PartitionLoadQueue loadQueue;

        float partitionWidth;

        float partitionHeight;

        float inversePartitionWidth;

        float inversePartitionHeight;

        Vector3 eyePosition;

        public float PartitionWidth
        {
            get { return partitionWidth; }
            set
            {
                partitionWidth = value;
                inversePartitionWidth = 1 / value;
            }
        }

        public float PartitionHeight
        {
            get { return partitionHeight; }
            set
            {
                partitionHeight = value;
                inversePartitionHeight = 1 / value;
            }
        }

        public float ActivationRange { get; set; }

        public float DeactivationRange { get; set; }

        public Vector3 EyePosition
        {
            get { return eyePosition; }
            set { eyePosition = value; }
        }

        #region Debug

        public int WaitLoadPartitionCount
        {
            get { return loadQueue.RequestQueueCount; }
        }

        public int LoadingParitionCount
        {
            get
            {
                int counter = 0;
                foreach (var p in partitions)
                    if (p.LoadState == PartitionLoadState.Loading)
                        counter++;

                return counter;
            }
        }

        public int NonePartitionCount
        {
            get
            {
                int counter = 0;
                foreach (var p in partitions)
                    if (p.LoadState == PartitionLoadState.None)
                        counter++;

                return counter;
            }
        }

        public int ActivePartitionCount
        {
            get { return partitions.Count; }
        }

        public int PartitionLoadingThreadCount
        {
            get { return loadQueue.ThreadCount; }
        }

        public int FreePartitionLoadingThreadCount
        {
            get { return loadQueue.FreeThreadCount; }
        }

        public int TotalPartitionObjectCount
        {
            get { return partitionPool.TotalObjectCount; }
        }

        public int FreePartitionObjectCount
        {
            get { return partitionPool.FreeObjectCount; }
        }

        public int MaxPartitionObjectCount
        {
            get { return partitionPool.MaxCapacity; }
        }

        #endregion

        public PartitionManager(Func<Partition> createFunction, int loadThreadCount,
            int initialPartitionPoolCapacity, int maxPartitionPoolCapacity)
        {
            if (createFunction == null) throw new ArgumentNullException("createFunction");

            partitionPool = new Pool<Partition>(createFunction, initialPartitionPoolCapacity, maxPartitionPoolCapacity);
            loadQueue = new PartitionLoadQueue(LoadResultCallback, loadThreadCount);
        }

        public void Update(GameTime gameTime)
        {
            // update the queue.
            loadQueue.Update(eyePosition);

            // select partitions.
            SelectPartitions();
        }

        public void Draw(GameTime gameTime)
        {
            foreach (var partition in partitions)
            {
                if (partition.LoadState == PartitionLoadState.Loaded)
                    partition.Draw(gameTime);
            }
        }

        void SelectPartitions()
        {
            Rectangle deactivationBounds;
            CalculateBounds(DeactivationRange, out deactivationBounds);

            // deactivate partitions.
            int index = 0;
            while (index < partitions.Count)
            {
                var partition = partitions[index];

                if (!deactivationBounds.Contains(partition.X, partition.Y))
                {
                    // This active partition is out of the deactivation bounds.
                    switch (partition.LoadState)
                    {
                        case PartitionLoadState.Loaded:
                            // Unload and return this.
                            partition.UnloadContent();
                            partitions.RemoveAt(index);
                            partition.LoadState = PartitionLoadState.None;
                            partitionPool.Return(partition);
                            break;
                        case PartitionLoadState.WaitLoad:
                            // Return this into pool immediately.
                            partitions.RemoveAt(index);
                            partition.LoadState = PartitionLoadState.None;
                            partitionPool.Return(partition);
                            break;
                        case PartitionLoadState.Loading:
                            // This will be released after loaded.
                            index++;
                            break;
                        default:
                            throw new InvalidOperationException("The unexpected state of the partition.");
                    }
                }
                else
                {
                    index++;
                }
            }

            // activate partitions.
            Rectangle activationBounds;
            CalculateBounds(ActivationRange, out activationBounds);
            for (int y = activationBounds.Y; y < activationBounds.Y + activationBounds.Height; y++)
            {
                for (int x = activationBounds.X; x < activationBounds.X + activationBounds.Width; x++)
                {
                    bool alreadyExists = false;
                    foreach (var p in partitions)
                    {
                        if (x == p.X && y == p.Y)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (alreadyExists) continue;

                    // A new partition.
                    var partition = partitionPool.Borrow();
                    
                    // Skip if no partition exists in pool.
                    if (partition == null)
                        continue;

                    partition.LoadState = PartitionLoadState.WaitLoad;
                    partition.Initialize(x, y, partitionWidth, partitionHeight);

                    // Add.
                    partitions.Add(partition);

                    // Load async.
                    loadQueue.Enqueue(partition);
                }
            }
        }

        void CalculateBounds(float range, out Rectangle rectangle)
        {
            int minX = (int) ((eyePosition.X - range) * inversePartitionWidth);
            int minY = (int) ((eyePosition.Z - range) * inversePartitionHeight);
            int maxX = (int) ((eyePosition.X + range) * inversePartitionWidth);
            int maxY = (int) ((eyePosition.Z + range) * inversePartitionHeight);

            rectangle = new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        void LoadResultCallback(Partition partition)
        {
            int x = partition.X;
            int y = partition.Y;

            foreach (var p in partitions)
            {
                if (p.LoadState != PartitionLoadState.Loaded)
                    continue;

                bool isNeighbor = false;
                if (p.Y == y && (p.X == x - 1 || p.X == x + 1))
                {
                    isNeighbor = true;
                }
                if (p.X == x && (p.Y == y - 1 || p.Y == y + 1))
                {
                    isNeighbor = true;
                }

                if (isNeighbor)
                    p.NeighborLoaded(partition);
            }

            // mark.
            partition.LoadState = PartitionLoadState.Loaded;
        }
    }
}
