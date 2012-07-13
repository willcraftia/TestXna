#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class DefaultMorph : Morph
    {
        float visibilityDistance/* = 50000*/;

        float detailBalance = 2;

        float morphStartRatio/* = 0.66f*/;

        float[] visibilityRanges;

        float[] morphStartRanges;

        float[] morphEndRanges;

        Vector2[] morphConsts;

        public float VisibilityDistance
        {
            get { return visibilityDistance; }
            set
            {
                if (visibilityDistance == value) return;

                visibilityDistance = value;
                Initialized = false;
            }
        }

        public float DetailBalance
        {
            get { return detailBalance; }
            set
            {
                if (detailBalance == value) return;

                detailBalance = value;
                Initialized = false;
            }
        }

        public float MorphStartRatio
        {
            get { return morphStartRatio; }
            set
            {
                if (morphStartRatio == value) return;

                morphStartRatio = value;
                Initialized = false;
            }
        }

        public DefaultMorph(int levelCount)
        {
            visibilityRanges = new float[levelCount];
            morphStartRanges = new float[levelCount];
            morphEndRanges = new float[levelCount];
            morphConsts = new Vector2[levelCount];
        }

        public override void GetMorphConsts(out Vector2[] results)
        {
            results = new Vector2[morphConsts.Length];
            Array.Copy(morphConsts, results, morphConsts.Length);
        }

        public override float GetVisibilityRange(int level)
        {
            return visibilityRanges[level];
        }

        protected override void InitializeOverride()
        {
            float lodNear = 0;
            float lodFar = visibilityDistance;

            // pow(detailBalance, i) を順に足して total とする。
            float total = 0;
            float currentDetailBalance = 1;
            for (int i = 0; i < visibilityRanges.Length; i++)
            {
                total += currentDetailBalance;
                currentDetailBalance *= detailBalance;
            }

            // total での単位長さを求める。
            float section = (lodFar - lodNear) / total;

            // 詳細な LOD から順に視覚範囲を計算して配列に設定。
            // および、詳細な LOD から順にモーフィング開始距離と終了距離を配列に設定。
            // 添字 0 はリーフ ノードに対応する LOD。
            float lastVisibilityRange = lodNear;
            float lastMorphStart = lodNear;
            currentDetailBalance = 1;
            for (int i = 0; i < visibilityRanges.Length; i++)
            {
                // Calculate a visibility range.
                visibilityRanges[i] = lastVisibilityRange + section * currentDetailBalance;
                lastVisibilityRange = visibilityRanges[i];
                currentDetailBalance *= detailBalance;

                // Calculate a morph start/end range.
                morphEndRanges[i] = visibilityRanges[i];
                morphStartRanges[i] = lastMorphStart + (morphEndRanges[i] - lastMorphStart) * morphStartRatio;
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
