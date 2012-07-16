#region Using

using System;

#endregion

namespace TiledTerrainDemo.Framework
{
    /// <summary>
    /// Options for StringBuilderExtension class.
    /// </summary>
    [Flags]
    public enum AppendNumberOptions
    {
        // The normal format.
        None = 0,

        // Appends "+" to use a positive value.
        PositiveSign = 1,

        // Appends "," every 3 digits.
        NumberGroup = 2,
    }
}
