#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Noise
{
    public sealed class Select
    {
        public const float DefaultEdgeFalloff = 0;

        public const float DefaultLowerBound = -1;

        public const float DefaultUpperBound = 1;

        float edgeFalloff = DefaultEdgeFalloff;

        float lowerBound = DefaultLowerBound;

        float upperBound = DefaultUpperBound;

        public float EdgeFalloff
        {
            get { return edgeFalloff; }
            set { edgeFalloff = value; }
        }

        public float LowerBound
        {
            get { return lowerBound; }
            set { lowerBound = value; }
        }

        public float UpperBound
        {
            get { return upperBound; }
            set { upperBound = value; }
        }

        public NoiseDelegate ControllerNoise { get; set; }

        public NoiseDelegate Noise0 { get; set; }

        public NoiseDelegate Noise1 { get; set; }

        public float GetValue(float x, float y, float z)
        {
            var control = ControllerNoise(x, y, z);
            var halfSize = (upperBound - lowerBound) * 0.5f;
            var ef = (halfSize < edgeFalloff) ? halfSize : edgeFalloff;

            if (0 < ef)
            {
                if (control < lowerBound - edgeFalloff)
                    return Noise0(x, y, z);
                
                if (control < lowerBound + edgeFalloff)
                {
                    var lowerCurve = lowerBound - edgeFalloff;
                    var upperCurve = lowerBound + edgeFalloff;
                    var amount = NoiseHelper.CubicSCurve((control - lowerCurve) / (upperCurve - lowerCurve));
                    return MathHelper.Lerp(Noise0(x, y, z), Noise1(x, y, z), amount);
                }

                if (control < upperBound - edgeFalloff)
                    return Noise1(x, y, z);

                if (control < upperBound + edgeFalloff)
                {
                    var lowerCurve = upperBound - edgeFalloff;
                    var upperCurve = upperBound + edgeFalloff;
                    var amount = NoiseHelper.CubicSCurve((control - lowerCurve) / (upperCurve - lowerCurve));
                    return MathHelper.Lerp(Noise1(x, y, z), Noise0(x, y, z), amount);
                }

                return Noise0(x, y, z);
            }

            if (control < lowerBound || upperBound < control)
                return Noise0(x, y, z);

            return Noise1(x, y, z);
        }
    }
}
