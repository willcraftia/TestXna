#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public interface IModule
    {
        float Sample(float x, float y, float z);
    }
}
