#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Select : IModule
    {
        public const float DefaultEdgeFalloff = 0;

        public const float DefaultLowerBound = -1;

        public const float DefaultUpperBound = 1;

        SampleSourceDelegate controller;

        SampleSourceDelegate source0;

        SampleSourceDelegate source1;

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

        public SampleSourceDelegate Controller
        {
            get { return controller; }
            set { controller = value; }
        }

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
            var control = controller(x, y, z);
            var halfSize = (upperBound - lowerBound) * 0.5f;
            var ef = (halfSize < edgeFalloff) ? halfSize : edgeFalloff;

            if (0 < ef)
            {
                if (control < lowerBound - edgeFalloff)
                    return source0(x, y, z);
                
                if (control < lowerBound + edgeFalloff)
                {
                    var lowerCurve = lowerBound - edgeFalloff;
                    var upperCurve = lowerBound + edgeFalloff;
                    var amount = NoiseHelper.SCurve3((control - lowerCurve) / (upperCurve - lowerCurve));
                    return MathHelper.Lerp(source0(x, y, z), source1(x, y, z), amount);
                }

                if (control < upperBound - edgeFalloff)
                    return source1(x, y, z);

                if (control < upperBound + edgeFalloff)
                {
                    var lowerCurve = upperBound - edgeFalloff;
                    var upperCurve = upperBound + edgeFalloff;
                    var amount = NoiseHelper.SCurve3((control - lowerCurve) / (upperCurve - lowerCurve));
                    return MathHelper.Lerp(source1(x, y, z), source0(x, y, z), amount);
                }

                return source0(x, y, z);
            }

            if (control < lowerBound || upperBound < control)
                return source0(x, y, z);

            return source1(x, y, z);
        }
    }
}
