#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    /// <summary>
    /// 頂点の位置のみを保持するカスタム頂点構造体です。
    /// </summary>
    public struct VertexPosition : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));

        /// <summary>
        /// 頂点の位置。
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="position">頂点の位置。</param>
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
