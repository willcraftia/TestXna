#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    /// <summary>
    /// The class manages height colors.
    /// </summary>
    public sealed class HeightColorCollection
    {
        List<HeightColor> heightColors = new List<HeightColor>();

        public HeightColor this[int index]
        {
            get { return heightColors[index]; }
        }

        public int Count
        {
            get { return heightColors.Count; }
        }

        public void AddColor(float position, Color color)
        {
            AddColor(position, color.ToVector4());
        }

        public void AddColor(float position, Vector4 color)
        {
            var heightColor = new HeightColor
            {
                Position = position,
                Color = color
            };
            heightColors.Add(heightColor);
            heightColors.Sort();
        }
    }
}
