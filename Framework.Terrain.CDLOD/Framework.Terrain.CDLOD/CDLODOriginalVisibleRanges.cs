#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    public sealed class CDLODOriginalVisibleRanges : ICDLODVisibleRanges
    {
        public const float DefaultMaxVisibleDistance = 20000;

        public const float DefaultDetailBalance = 2;

        float[] ranges;

        public float MaxVisibleDistance { get; set; }

        public float DetailBalance { get; set; }

        public int Count
        {
            get { return ranges.Length; }
        }

        public float this[int level]
        {
            get { return ranges[level]; }
        }

        public CDLODOriginalVisibleRanges(int levelCount)
        {
            MaxVisibleDistance = DefaultMaxVisibleDistance;
            DetailBalance = DefaultDetailBalance;

            ranges = new float[levelCount];
        }

        public void Initialize()
        {

            float near = 0;
            float far = MaxVisibleDistance;

            // add pow(detailBalance, i) in sequence.
            float total = 0;
            float currentDetailBalance = 1;
            for (int i = 0; i < ranges.Length; i++)
            {
                total += currentDetailBalance;
                currentDetailBalance *= DetailBalance;
            }

            // unit length.
            float section = (far - near) / total;

            float lastRange = near;
            currentDetailBalance = 1;
            for (int i = 0; i < ranges.Length; i++)
            {
                // Calculate the visibility distance per a level.
                ranges[i] = lastRange + section * currentDetailBalance;
                lastRange = ranges[i];
                currentDetailBalance *= DetailBalance;
            }
        }
    }
}
