#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class NoiseRainMap : Map<float>
    {
        NoiseMapBuilder builder = new NoiseMapBuilder();

        SampleSourceDelegate noiseSource;

        float rainAmount = 0.01f;

        ScaleBias scaleBias = new ScaleBias();

        public SampleSourceDelegate NoiseSource
        {
            get { return noiseSource; }
            set { noiseSource = value; }
        }

        public Bounds Bounds
        {
            get { return builder.Bounds; }
            set { builder.Bounds = value; }
        }

        public float RainAmount
        {
            get { return rainAmount; }
            set { rainAmount = value; }
        }

        public NoiseRainMap(int width, int height)
            : base(width, height)
        {
            builder.Destination = this;
        }

        public void Build()
        {
            scaleBias.Source = noiseSource;
            scaleBias.Scale = rainAmount;
            scaleBias.Bias = rainAmount;

            builder.Source = scaleBias.Sample;
            builder.Build();
        }
    }
}
