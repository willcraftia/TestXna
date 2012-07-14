#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class DefaultVisibleRanges : IVisibleRanges
    {
        public const float DefaultMostDetailRange = 200;

        public const float DefaultDetailBalance = 2;

        Settings settings;

        float[] ranges;

        public float MostDetailRange { get; set; }

        public float DetailBalance { get; set; }

        public int Count
        {
            get { return ranges.Length; }
        }

        public float this[int level]
        {
            get { return ranges[level]; }
        }

        public DefaultVisibleRanges(Settings settings)
        {
            this.settings = settings;

            MostDetailRange = DefaultMostDetailRange;
            DetailBalance = DefaultDetailBalance;
            ranges = new float[settings.LevelCount];
        }

        public void Initialize()
        {
            float near = 0;
            float lastRange = near;
            float currentDetailBalance = 1;
            float section = MostDetailRange / settings.PatchScale;
            for (int i = 0; i < ranges.Length; i++)
            {
                ranges[i] = lastRange + section * currentDetailBalance;
                lastRange = ranges[i];
                currentDetailBalance *= DetailBalance;
            }
        }
    }
}
