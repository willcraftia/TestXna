#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class VisibilityRanges
    {
        public const float MorphStartRatio = 0.66f;

        Settings settings;

        float[] ranges;

        public int Count
        {
            get { return settings.LevelCount; }
        }

        public float this[int level]
        {
            get { return ranges[level]; }
        }

        public VisibilityRanges(Settings settings)
        {
            this.settings = settings;

            Initialize();
        }

        public void CreateMorphConsts(out Vector2[] results)
        {
            results = new Vector2[settings.LevelCount];

            float lodNear = 0;
            float lastMorphStart = lodNear;
            for (int i = 0; i < settings.LevelCount; i++)
            {
                // Calculate a morph start/end range.
                var end = ranges[i];
                var start = lastMorphStart + (end - lastMorphStart) * MorphStartRatio;
                lastMorphStart = start;

                // Calculate a morph constant.
                const float errorFudge = 0.01f;
                end = MathHelper.Lerp(end, start, errorFudge);

                results[i] = new Vector2(end / (end - start), 1 / (end - start));
            }
        }

        void Initialize()
        {
            ranges = new float[settings.LevelCount];

            float lodNear = 0;
            float lodFar = settings.VisibilityDistance;

            // add pow(detailBalance, i) in sequence.
            float total = 0;
            float currentDetailBalance = 1;
            for (int i = 0; i < settings.LevelCount; i++)
            {
                total += currentDetailBalance;
                currentDetailBalance *= settings.DetailBalance;
            }

            // unit length.
            float section = (lodFar - lodNear) / total;

            float lastVisibilityRange = lodNear;
            float lastMorphStart = lodNear;
            currentDetailBalance = 1;
            for (int i = 0; i < settings.LevelCount; i++)
            {
                // Calculate the visibility distance per a level.
                ranges[i] = lastVisibilityRange + section * currentDetailBalance;
                lastVisibilityRange = ranges[i];
                currentDetailBalance *= settings.DetailBalance;
            }
        }
    }
}
