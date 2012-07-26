#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Add : IModule
    {
        SampleSourceDelegate source0;

        SampleSourceDelegate source1;

        public SampleSourceDelegate Source0
        {
            get { return source0; }
            set { source0 = value; }
        }

        public SampleSourceDelegate Source1
        {
            get { return source1; }
            set { source1 = value; }
        }

        public float Sample(float x, float y, float z)
        {
            return source0(x, y, z) + source1(x, y, z);
        }
    }
}
