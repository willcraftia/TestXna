#region Using

using System;

#endregion

namespace TiledTerrainDemo.Landscape
{
    public interface PartitionFactory
    {
        Partition Create();
    }
}
