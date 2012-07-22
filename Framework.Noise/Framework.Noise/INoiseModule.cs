#region Using

using System;

#endregion

namespace Willcraftia.Framework.Noise
{
    public interface INoiseModule
    {
        float GetValue(float x, float y, float z);
    }
}
