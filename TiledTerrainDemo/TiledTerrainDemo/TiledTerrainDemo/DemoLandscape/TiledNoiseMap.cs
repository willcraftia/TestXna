#region Using

using System;
using TiledTerrainDemo.Noise;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class TiledNoiseMap
    {
        NoiseMap backingNoiseMap = new NoiseMap();

        float boundX;

        float boundY;

        float boundWidth;

        float boundHeight;

        public NoiseMap.DelegateGetValue2 GetValue2
        {
            get { return backingNoiseMap.GetValue2; }
            set { backingNoiseMap.GetValue2 = value; }
        }

        public int Width
        {
            get { return backingNoiseMap.Width; }
            set { backingNoiseMap.Width = value; }
        }

        public int Height
        {
            get { return backingNoiseMap.Height; }
            set { backingNoiseMap.Height = value; }
        }

        public float this[int x, int y]
        {
            get
            {
                return backingNoiseMap.Values[x + y * backingNoiseMap.Width];
            }
        }

        public float[] RawValues
        {
            get { return backingNoiseMap.Values; }
        }

        public void SetBounds(float x, float y, float w, float h)
        {
            boundX = x;
            boundY = y;
            boundWidth = w;
            boundHeight = h;
        }

        public void Build()
        {
            var dx = boundWidth * (1 / (float) backingNoiseMap.Width);
            var dy = boundHeight * (1 / (float) backingNoiseMap.Height);
            backingNoiseMap.SetBounds(boundX, boundY, boundWidth + dx, boundWidth + dy);

            backingNoiseMap.Build();
        }
    }
}
