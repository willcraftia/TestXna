#region Using

using System;

#endregion

namespace Willcraftia.Framework.Noise
{
    public sealed class Add : INoiseModule
    {
        NoiseDelegate noise0;

        NoiseDelegate noise1;

        public NoiseDelegate Noise0
        {
            get { return noise0; }
            set { noise0 = value; }
        }

        public NoiseDelegate Noise1
        {
            get { return noise1; }
            set { noise1 = value; }
        }

        public float GetValue(float x, float y, float z)
        {
            return noise0(x, y, z) + noise1(x, y, z);
        }
    }
}
