#region Using

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    /// <summary>
    /// The vertex structure represents a patch instance.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PatchInstanceVertex : IVertexType
    {
        /// <summary>
        /// The position (x, z) of a patch in the world space.
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// The scale of a patch.
        /// </summary>
        public float Scale;

        /// <summary>
        /// The LOD level of a patch.
        /// </summary>
        public float Level;

        public readonly static VertexDeclaration VertexDecl = new VertexDeclaration(
            Marshal.SizeOf(typeof(PatchInstanceVertex)),
            // Offset, Scale, Level (16 bytes)
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
        );

        // I/F
        public VertexDeclaration VertexDeclaration
        {
            get { return VertexDecl; }
        }
    }
}
