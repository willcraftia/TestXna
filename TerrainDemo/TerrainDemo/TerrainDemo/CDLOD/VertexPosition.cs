#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    /// <summary>
    /// The vertex structure contains a position of a PatchMesh.
    /// </summary>
    public struct VertexPosition : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));

        public Vector3 Position;

        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        // I/F
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
