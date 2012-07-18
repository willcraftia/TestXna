#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public sealed class ScaleBias
    {
        public const float DefaultBias = 0;

        public const float DefaultScale = 1;

        float bias = DefaultBias;

        float scale = DefaultScale;

        public NoiseDelegate Noise { get; set; }

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

        public float GetValue(float x, float y, float z)
        {
            return Noise(x, y, z) * scale + bias;
        }
    }
}
