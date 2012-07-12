#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class DefaultMorph : Morph
    {
        float visibilityDistance/* = 50000*/;

        float detailBalance = 2;

        float morphStartRatio/* = 0.66f*/;

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

        public DefaultMorph(int heightMapSize)
            : base(heightMapSize)
        {
        }

        protected override void Initialize(float[] visibilityRanges, float[] morphStartRanges, float[] morphEndRanges)
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
                visibilityRanges[i] = lastVisibilityRange + section * currentDetailBalance;
                lastVisibilityRange = visibilityRanges[i];
                currentDetailBalance *= detailBalance;

                morphEndRanges[i] = visibilityRanges[i];
                morphStartRanges[i] = lastMorphStart + (morphEndRanges[i] - lastMorphStart) * morphStartRatio;
                lastMorphStart = morphStartRanges[i];
            }
        }
    }
}
