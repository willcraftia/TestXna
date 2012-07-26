#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class ScaleBias : IModule
    {
        public const float DefaultBias = 0;

        public const float DefaultScale = 1;

        SampleSourceDelegate source;

        float bias = DefaultBias;

        float scale = DefaultScale;

        public SampleSourceDelegate Source
        {
            get { return source; }
            set { source = value; }
        }

        public float Bias
        {
            get { return bias; }
            set { bias = value; }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public float Sample(float x, float y, float z)
        {
            return source(x, y, z) * scale + bias;
        }
    }
}
