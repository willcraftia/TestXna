#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public struct Bounds
    {
        public float X;

        public float Y;
        
        public float Width;

        public float Height;

        public static Bounds One
        {
            get { return new Bounds { X = 0, Y = 0, Width = 1, Height = 1 }; }
        }
    }
}
