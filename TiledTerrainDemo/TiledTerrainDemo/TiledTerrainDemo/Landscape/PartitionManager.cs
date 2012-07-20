#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Landscape
{
    public sealed class PartitionManager
    {
        #region LoadingPartition

        struct LoadingPartition : IComparable<LoadingPartition>
        {
            public Partition Partition;

            public float EyeDistanceSquared;

            public int CompareTo(LoadingPartition other)
            {
                return EyeDistanceSquared.CompareTo(other.EyeDistanceSquared);
            }
        }

        #endregion

        Pool<Partition> partitionPool;

        List<Partition> partitions = new List<Partition>();

        List<LoadingPartition> loadingPartitions = new List<LoadingPartition>();

        PartitionLoadQueue loadQueue;

        float partitionWidth;

        float partitionHeight;

        float inversePartitionWidth;

        float inversePartitionHeight;

        float halfPartitionWidth;

        float halfPartitionHeight;

        Vector3 eyePosition;

        public float PartitionWidth
        {
            get { return partitionWidth; }
            set
            {
                partitionWidth = value;
                inversePartitionWidth = 1 / value;
                halfPartitionWidth = partitionWidth * 0.5f;
            }
        }

        public float PartitionHeight
        {
            get { return partitionHeight; }
            set
            {
                partitionHeight = value;
                inversePartitionHeight = 1 / value;
                halfPartitionHeight = partitionHeight * 0.5f;
            }
        }

        public float ActivationRange { get; set; }

        public float DeactivationRange { get; set; }

        public Vector3 EyePosition
        {
            get { return eyePosition; }
            set { eyePosition = value; }
        }

        public PartitionManager(Func<Partition> creationFunction)
        {
            if (creationFunction == null) throw new ArgumentNullException("creationFunction");

            partitionPool = new Pool<Partition>(creationFunction);
            loadQueue = new PartitionLoadQueue(LoadResultCallback);
        }

        public void Update(GameTime gameTime)
        {
            // update the queue.
            loadQueue.Update();

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
                        case PartitionLoadState.WaitLoad:
                            partitions.RemoveAt(index);
                            partition.UnloadContent();
                            partition.LoadState = PartitionLoadState.None;
                            partitionPool.Return(partition);
                            break;
                        case PartitionLoadState.Loading:
                            // This partition will be released after loaded.
                            index++;
                            break;
                        default:
                            throw new InvalidOperationException("The unexpected state of the partition.");
                            //break;
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
                    partition.LoadState = PartitionLoadState.WaitLoad;
                    partition.X = x;
                    partition.Y = y;

                    //partitions.Add(partition);

                    //// load.
                    //loadQueue.Enqueue(partition);

                    var loadingPartition = new LoadingPartition
                    {
                        Partition = partition,
                        EyeDistanceSquared = CalculateEyeDistance(partition)
                    };
                    loadingPartitions.Add(loadingPartition);
                }
            }

            // Sort by EyeDistanceSquared.
            loadingPartitions.Sort();

            // Request to load partitions.
            foreach (var loadingPartition in loadingPartitions)
            {
                partitions.Add(loadingPartition.Partition);
                loadQueue.Enqueue(loadingPartition.Partition);
            }

            loadingPartitions.Clear();
        }

        float CalculateEyeDistance(Partition partition)
        {
            var partitionCenter = new Vector3
            {
                X = partition.X * partitionWidth + halfPartitionWidth,
                Y = 0,
                Z = partition.Y * partitionHeight + halfPartitionHeight
            };

            float result;
            Vector3.DistanceSquared(ref eyePosition, ref partitionCenter, out result);
            
            return result;
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
            // mark.
            partition.LoadState = PartitionLoadState.Loaded;
        }
    }
}
