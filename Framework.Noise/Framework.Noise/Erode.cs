#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Erode : IModule
    {
        SampleSourceDelegate source;

        float ratio = 0.2f;

        float threshold = 0.3f;

        public SampleSourceDelegate Source
        {
            get { return source; }
            set { source = value; }
        }

        public float Sample(float x, float y, float z)
        {
            // todo: 2d sampling for test now.

            var v = source(x, 0, z);
            var v1 = source(x,     0, z + 1);
            var v2 = source(x - 1, 0, z);
            var v3 = source(x + 1, 0, z);
            var v4 = source(x,     0, z - 1);

            var d1 = v - v1;
            var d2 = v - v2;
            var d3 = v - v3;
            var d4 = v - v4;

            throw new NotImplementedException();
        }
    }
}
