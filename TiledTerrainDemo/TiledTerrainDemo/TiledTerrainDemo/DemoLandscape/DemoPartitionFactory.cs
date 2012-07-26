#region Using

using System;
using Willcraftia.Xna.Framework.Landscape;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoPartitionFactory
    {
        DemoPartitionContext context;

        public DemoPartitionFactory(DemoPartitionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;
        }

        public Partition Create()
        {
            return new DemoPartition(context);
        }
    }
}
