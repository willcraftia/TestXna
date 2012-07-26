#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// 連続書き込み用の頂点バッファ管理クラス
    /// </summary>
    /// <remarks>
    /// 動的頂点バッファへの書き込みは書き込み先がGPUの読み込み中の場所と
    /// バッティングしないよう注意する必要がある。
    /// 
    /// http://blogs.msdn.com/ito/archive/2009/03/25/how-gpus-works-01.aspx
    /// 
    /// 管理自体は簡単でも余計な変数や
    /// 処理が増えるので、その処理をこのクラスで管理している。
    /// DynamicVertexBufferとの違いはSetData&gt;T&lt;をメソッドが書き込んだ
    /// オフセットを返すのでその値を以下の様に使うことができる
    /// 
    /// int offset = writableVB.WriteData( ... );
    /// GraphicsDevice.SetVertices.Vertices[0].SetSource( writableVB.VertexBuffer, offset, stride );
    /// 
    /// </remarks>
    public class WritableVertexBuffer<T> where T : struct
    {
        // 実際の頂点バッファ
        DynamicVertexBuffer backingBuffer;

        // 現在の書き込み位置
        int currentPosition;

        // 最大頂点数
        int maxElementCount;

        /// <summary>
        /// バッファを取得する
        /// </summary>
        public DynamicVertexBuffer VertexBuffer
        {
            get { return backingBuffer; }
        }

        /// <summary>
        /// WritableVertexBufferの生成
        /// </summary>
        /// <param name="device">この頂点バッファに関連付けるグラフィックデバイス</param>
        /// <param name="elementCount">この頂点バッファに格納できる最大要素数</param>
        public WritableVertexBuffer(GraphicsDevice device, int maxElementCount)
        {
            this.backingBuffer = new DynamicVertexBuffer(device, typeof(T), maxElementCount, 0);
            this.maxElementCount = maxElementCount;
        }

        /// <summary>
        /// 頂点データの書き込み
        /// </summary>
        /// <param name="vertices">書き込む頂点データ配列</param>
        /// <returns>書き込んだデータの先頭オフセット</returns>
        public int WriteData(T[] vertices)
        {
            return SetData(vertices, 0, vertices.Length);
        }

        /// <summary>
        /// 頂点データの書き込み
        /// </summary>
        /// <param name="vertices">書き込む頂点データ配列</param>
        /// <param name="startIndex">書き込むデータの先頭インデックス</param>
        /// <param name="elementCount">書き込む要素数</param>
        /// <returns>書き込んだデータの先頭オフセット</returns>
        public int SetData(T[] vertices, int startIndex, int elementCount)
        {
            // 頂点データの書き込み
            int position = currentPosition;
            SetDataOptions options = SetDataOptions.NoOverwrite;

            // 最大要素数を超えるようであれば、頂点バッファの先頭に移動し、Dicardオプションを使う。
            if (maxElementCount < position + elementCount)
            {
                position = 0;
                options = SetDataOptions.Discard;
            }

            // 頂点データを頂点バッファへ書き込む
            int vertexStride = backingBuffer.VertexDeclaration.VertexStride;
            backingBuffer.SetData(position * vertexStride, vertices, startIndex, elementCount, vertexStride, options);
            currentPosition = position + elementCount;
            return position;
        }
    }
}
