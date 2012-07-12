#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public abstract class Morph
    {
        float[] visibilityRanges;

        float[] morphStartRanges;

        float[] morphEndRanges;

        public bool Initialized { get; protected set; }

        public Morph(int heightMapSize)
        {
            int levelCount = 0;
            for (int size = heightMapSize - 1; 2 <= size; size /= 2)
                levelCount++;

            visibilityRanges = new float[levelCount];
            morphStartRanges = new float[levelCount];
            morphEndRanges = new float[levelCount];
        }

        public void Initialize()
        {
            Initialize(visibilityRanges, morphStartRanges, morphEndRanges);

            Initialized = true;
        }

        public float GetVisibilityRange(int level)
        {
            return visibilityRanges[level];
        }

        // モーフィング定数を事前にシェーダへ配列として渡し、
        // シェーダ内でレベルから配列を参照するようにする？
        public void GetMorphConsts(int level, out Vector2 morphConsts)
        {
            var start = morphStartRanges[level];
            var end = morphEndRanges[level];

            const float errorFudge = 0.01f;
            end = MathHelper.Lerp(end, start, errorFudge);

            morphConsts = new Vector2(end / (end - start), 1 / (end - start));
        }

        protected abstract void Initialize(float[] visibilityRanges, float[] morphStartRanges, float[] morphEndRanges);
    }
}
