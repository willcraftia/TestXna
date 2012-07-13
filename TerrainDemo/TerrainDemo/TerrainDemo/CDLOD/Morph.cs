#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public abstract class Morph
    {
        public bool Initialized { get; protected set; }

        public void Initialize()
        {
            InitializeOverride();

            Initialized = true;
        }

        public abstract void GetMorphConsts(out Vector2[] results);

        public abstract float GetVisibilityRange(int level);

        protected abstract void InitializeOverride();
    }
}
