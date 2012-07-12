#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public interface IHeightMapSource
    {
        int Width { get; }

        int Height { get; }

        // スケール調整やオフセット調整などがされていない素の高さ。
        // [-1, 1] を仮定。
        // ノイズから生成する際には [-1, 1] に収まるように正規化しておく。
        float GetHeight(int x, int y);

        void GetAreaMinMaxHeight(int x, int y, int sizeX, int sizeY, out float minHeight, out float maxHeight);
    }
}
