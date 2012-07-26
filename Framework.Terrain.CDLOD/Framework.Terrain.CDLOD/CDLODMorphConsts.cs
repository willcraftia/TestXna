#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    public sealed class CDLODMorphConsts
    {
        public const float MorphStartRatio = 0.66f;

        const float errorFudge = 0.01f;

        public static void Create(ICDLODVisibleRanges visibleRanges, out Vector2[] results)
        {
            results = new Vector2[visibleRanges.Count];

            float lastStart = 0;
            for (int i = 0; i < visibleRanges.Count; i++)
            {
                // Calculate a morph start/end range.
                var end = visibleRanges[i];
                var start = lastStart + (end - lastStart) * MorphStartRatio;
                end = MathHelper.Lerp(end, start, errorFudge);
                lastStart = start;

                // Calculate a morph constant.
                results[i] = new Vector2(end / (end - start), 1 / (end - start));
            }
        }
    }
}
