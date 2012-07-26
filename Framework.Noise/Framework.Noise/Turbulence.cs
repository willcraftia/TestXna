#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Turbulence : IModule
    {
        public const float DefaultFrequency = 1;

        public const float DefaultPower = 1;

        public const int DefaultRoughness = 3;

        Perlin noiseX = new Perlin();
        Perlin noiseY = new Perlin();
        Perlin noiseZ = new Perlin();

        PerlinFractal distortX = new PerlinFractal();
        PerlinFractal distortY = new PerlinFractal();
        PerlinFractal distortZ = new PerlinFractal();

        SampleSourceDelegate source;

        float frequency = DefaultFrequency;

        float power = DefaultPower;

        int roughness = DefaultRoughness;

        public SampleSourceDelegate Source
        {
            get { return source; }
            set { source = value; }
        }

        public float Frequency
        {
            get { return frequency; }
            set
            {
                frequency = value;

                distortX.Frequency = value;
                distortY.Frequency = value;
                distortZ.Frequency = value;
            }
        }

        public float Power
        {
            get { return power; }
            set { power = value; }
        }

        public int Roughness
        {
            get { return roughness; }
            set
            {
                roughness = value;

                distortX.OctaveCount = value;
                distortY.OctaveCount = value;
                distortZ.OctaveCount = value;
            }
        }

        public int Seed
        {
            get { return noiseX.Seed; }
            set
            {
                noiseX.Seed = value;
                noiseY.Seed = value + 1;
                noiseZ.Seed = value + 2;
            }
        }

        public Turbulence()
        {
            distortX.Source = noiseX.Sample;
            distortY.Source = noiseY.Sample;
            distortZ.Source = noiseZ.Sample;

            distortX.Frequency = frequency;
            distortY.Frequency = frequency;
            distortZ.Frequency = frequency;

            distortX.OctaveCount = roughness;
            distortY.OctaveCount = roughness;
            distortZ.OctaveCount = roughness;

            noiseY.Seed = noiseX.Seed + 1;
            noiseZ.Seed = noiseX.Seed + 2;
        }

        public float Sample(float x, float y, float z)
        {
            // from libnoise's Turbulence class.

            // I can not understand the following codes yet.
            //float x0 = x + (12414.0f / 65536.0f);
            //float y0 = y + (65124.0f / 65536.0f);
            //float z0 = z + (31337.0f / 65536.0f);
            //float x1 = x + (26519.0f / 65536.0f);
            //float y1 = y + (18128.0f / 65536.0f);
            //float z1 = z + (60493.0f / 65536.0f);
            //float x2 = x + (53820.0f / 65536.0f);
            //float y2 = y + (11213.0f / 65536.0f);
            //float z2 = z + (44845.0f / 65536.0f);
            //float dx = x + distortX.GetValue(x0, y0, z0) * power;
            //float dy = x + distortY.GetValue(x1, y1, z1) * power;
            //float dz = x + distortZ.GetValue(x2, y2, z2) * power;

            float dx = x + distortX.Sample(x, y, z) * power;
            float dy = y + distortY.Sample(x, y, z) * power;
            float dz = z + distortZ.Sample(x, y, z) * power;

            return source(dx, dy, dz);
        }
    }
}
