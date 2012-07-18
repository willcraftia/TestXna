#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public sealed class Turbulence/* : Fractal*/
    {
        public const float DefaultFrequency = Fractal.DefaultFrequency;

        public const float DefaultPower = 1;

        public const int DefaultRoughness = 3;

        PerlinNoise perlinNoiseX = new PerlinNoise();
        PerlinNoise perlinNoiseY = new PerlinNoise();
        PerlinNoise perlinNoiseZ = new PerlinNoise();

        PerlinFractal distortX = new PerlinFractal();
        PerlinFractal distortY = new PerlinFractal();
        PerlinFractal distortZ = new PerlinFractal();

        float frequency = DefaultFrequency;

        float power = DefaultPower;

        int roughness = DefaultRoughness;

        public NoiseDelegate Noise { get; set; }

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
            get { return perlinNoiseX.Seed; }
            set
            {
                perlinNoiseX.Seed = value;
                perlinNoiseY.Seed = value + 1;
                perlinNoiseZ.Seed = value + 2;
            }
        }

        public Turbulence()
        {
            distortX.Noise = perlinNoiseX.GetValue;
            distortY.Noise = perlinNoiseY.GetValue;
            distortZ.Noise = perlinNoiseZ.GetValue;

            distortX.Frequency = frequency;
            distortY.Frequency = frequency;
            distortZ.Frequency = frequency;

            distortX.OctaveCount = roughness;
            distortY.OctaveCount = roughness;
            distortZ.OctaveCount = roughness;

            perlinNoiseY.Seed = perlinNoiseX.Seed + 1;
            perlinNoiseZ.Seed = perlinNoiseX.Seed + 2;
        }

        public float GetValue(float x, float y, float z)
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

            float dx = x + distortX.GetValue(x, y, z) * power;
            float dy = y + distortY.GetValue(x, y, z) * power;
            float dz = z + distortZ.GetValue(x, y, z) * power;

            return Noise(dx, dy, dz);
        }

        // where did I get the following code ?
        //protected override float GetValueOverride(float x, float y, float z)
        //{
        //    x *= frequency;
        //    y *= frequency;
        //    z *= frequency;

        //    float value = 0;

        //    for (int i = 0; i < octaveCount; i++)
        //    {
        //        value += Math.Abs(Noise(x, y, z)) * spectralWeights[i];

        //        x *= lacunarity;
        //        y *= lacunarity;
        //        z *= lacunarity;
        //    }

        //    return value;
        //}
    }
}
