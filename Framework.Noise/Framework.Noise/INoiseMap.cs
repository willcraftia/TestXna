﻿#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public interface INoiseMap
    {
        int Width { get; }

        int Height { get; }

        float this[int x, int y] { get; set; }
    }
}
