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

        // オリジナルはインスタンシングを使っていない。
        // しかし、シェーダでモーフィング範囲を使用しているので、
        // これを示すためのレベルとして使えば良い気がする。
        // モーフィング範囲は固定でシェーダへ設定しておく。
        // レベル値は int だが、シェーダでは float で扱う。
        public float Level;

        /// <summary>
        /// モーフィング定数。
        /// x = end / (end - start)
        /// y = 1 / (end - start)
        /// </summary>
        public Vector2 MorphConsts;

        /// <summary>
        /// 頂点宣言
        /// </summary>
        public readonly static VertexDeclaration VertexDecl = new VertexDeclaration(
            Marshal.SizeOf(typeof(PatchInstanceVertex)),
            // Offset, Scale, Level (16バイト)
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            // MorphConsts (8 バイト)
            new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2)
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
