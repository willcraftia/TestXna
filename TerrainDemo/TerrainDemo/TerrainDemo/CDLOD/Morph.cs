#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public interface Morph
    {
        void GetMorphConsts(out Vector2[] results);

        float GetVisibilityRange(int level);
    }
}
