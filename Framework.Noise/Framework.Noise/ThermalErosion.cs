#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class ThermalErosion : IModule
    {
        SampleSourceDelegate source;

        float c = 0.8f;

        float talus = 0.05f;

        public SampleSourceDelegate Source
        {
            get { return source; }
            set { source = value; }
        }

        public float Sample(float x, float y, float z)
        {
            // todo: 2d sampling for test now.

            // todo
            float s = 1f / 513f;

            var v = source(x, 0, z);
            var v1 = source(x,     0, z + s);
            var v2 = source(x - s, 0, z);
            var v3 = source(x + s, 0, z);
            var v4 = source(x,     0, z - s);

            var d1 = v - v1;
            var d2 = v - v2;
            var d3 = v - v3;
            var d4 = v - v4;

            float result = v;

            if (d1 < talus) v += c * (d1 - talus);
            if (d2 < talus) v += c * (d2 - talus);
            if (d3 < talus) v += c * (d3 - talus);
            if (d4 < talus) v += c * (d4 - talus);

            return result;
        }
    }
}
