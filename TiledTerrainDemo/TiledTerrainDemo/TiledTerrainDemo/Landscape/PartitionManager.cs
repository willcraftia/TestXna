﻿#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Landscape
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

        public int WaitLoadPartitionCount
        {
            get { return loadQueue.RequestQueueCount; }
        }

        public int PartitionLoadingThreadCount
        {
            get { return loadQueue.ThreadCount; }
        }

        public int FreePartitionLoadingThreadCount
        {
            get { return loadQueue.FreeThreadCount; }
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
                            partition.UnloadContent();
                            partitions.RemoveAt(index);
                            partition.LoadState = PartitionLoadState.None;
                            partitionPool.Return(partition);
                            break;
                        case PartitionLoadState.WaitLoad:
                            partitions.RemoveAt(index);
                            partition.LoadState = PartitionLoadState.None;
                            partitionPool.Return(partition);
                            break;
                        case PartitionLoadState.Loading:
                            // This partition will be released after loaded.
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
            // mark.
            partition.LoadState = PartitionLoadState.Loaded;
        }
    }
}
