#region Using

using System;

#endregion

namespace TiledTerrainDemo.Landscape
{
    public abstract class Partition
    {
        public int X { get; set; }

        public int Y { get; set; }

        // Don't set a value into this property in subclasses.
        public PartitionLoadState LoadState { get; set; }

        public abstract void LoadContent();

        public abstract void UnloadContent();

        public abstract void Draw();
    }
}
