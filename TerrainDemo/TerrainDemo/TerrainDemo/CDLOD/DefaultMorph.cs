#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class DefaultMorph : Morph
    {
        public const float DefaultVisibilityDistance = 20000;

        public const float DefaultDetailBalance = 2;

        public const float DefaultMorphStartRatio = 0.66f;

        float[] visibilityRanges;

        float[] morphStartRanges;

        float[] morphEndRanges;

        Vector2[] morphConsts;

        public int LevelCount { get; private set; }

        public float VisibilityDistance { get; private set; }

        public float DetailBalance { get; private set; }

        public float MorphStartRatio { get; private set; }

        public DefaultMorph(int levelCount)
            : this(levelCount, DefaultVisibilityDistance, DefaultDetailBalance, DefaultMorphStartRatio)
        {
        }

        public DefaultMorph(int levelCount, float visibilityDistance, float detailBalance, float morphStartRatio)
        {
            LevelCount = levelCount;
            VisibilityDistance = visibilityDistance;
            DetailBalance = detailBalance;
            MorphStartRatio = morphStartRatio;

            Initialize();
        }

        // I/F
        public void GetMorphConsts(out Vector2[] results)
        {
            results = new Vector2[morphConsts.Length];
            Array.Copy(morphConsts, results, morphConsts.Length);
        }

        // I/F
        public float GetVisibilityRange(int level)
        {
            return visibilityRanges[level];
        }

        void Initialize()
        {
            visibilityRanges = new float[LevelCount];
            morphStartRanges = new float[LevelCount];
            morphEndRanges = new float[LevelCount];
            morphConsts = new Vector2[LevelCount];

            float lodNear = 0;
            float lodFar = VisibilityDistance;

            // add pow(detailBalance, i) in sequence.
            float total = 0;
            float currentDetailBalance = 1;
            for (int i = 0; i < visibilityRanges.Length; i++)
            {
                total += currentDetailBalance;
                currentDetailBalance *= DetailBalance;
            }

            // unit length.
            float section = (lodFar - lodNear) / total;

            float lastVisibilityRange = lodNear;
            float lastMorphStart = lodNear;
            currentDetailBalance = 1;
            for (int i = 0; i < visibilityRanges.Length; i++)
            {
                // Calculate a visibility range.
                visibilityRanges[i] = lastVisibilityRange + section * currentDetailBalance;
                lastVisibilityRange = visibilityRanges[i];
                currentDetailBalance *= DetailBalance;

                // Calculate a morph start/end range.
                morphEndRanges[i] = visibilityRanges[i];
                morphStartRanges[i] = lastMorphStart + (morphEndRanges[i] - lastMorphStart) * MorphStartRatio;
                lastMorphStart = morphStartRanges[i];

                // Calculate a morph constant.
                var start = morphStartRanges[i];
                var end = morphEndRanges[i];

                const float errorFudge = 0.01f;
                end = MathHelper.Lerp(end, start, errorFudge);

                morphConsts[i] = new Vector2(end / (end - start), 1 / (end - start));
            }
        }
    }
}
