#region Using

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    /// <summary>
    /// パッチ インスタンス情報格納用の頂点データ構造です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PatchInstanceVertex : IVertexType
    {
        /// <summary>
        /// ノードの位置 (x, z)。
        /// QuadTree のオフセット分も含む。
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// ノードのスケール。
        /// </summary>
        public float Scale;

        /// <summary>
        /// LOD レベル。
        /// </summary>
        public float Level;

        /// <summary>
        /// 頂点宣言
        /// </summary>
        public readonly static VertexDeclaration VertexDecl = new VertexDeclaration(
            Marshal.SizeOf(typeof(PatchInstanceVertex)),
            // Offset, Scale, Level (16バイト)
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
        );

        /// <summary>
        /// 頂点宣言の取得
        /// </summary>
        public VertexDeclaration VertexDeclaration
        {
            get { return VertexDecl; }
        }
    }
}
