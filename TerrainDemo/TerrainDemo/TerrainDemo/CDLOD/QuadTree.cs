#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class QuadTree
    {
        int topNodeCountX;

        int topNodeCountY;

        Node[,] topNodes;

        public void Build(ref CreateDescription createDescription)
        {
            var topNodeSize = createDescription.LeafNodeSize;
            for (int i = 1; i < createDescription.LevelCount; i++)
                topNodeSize *= 2;

            // オリジナルでは + 1 としているが、
            // + 1 では height map の領域をはみ出すので削除してみた。
            topNodeCountX = (createDescription.HeightMap.Width - 1) / topNodeSize /*+ 1*/;
            topNodeCountY = (createDescription.HeightMap.Height - 1) / topNodeSize /*+ 1*/;
            topNodes = new Node[topNodeCountX, topNodeCountY];
            for (int y = 0; y < topNodeCountY; y++)
            {
                for (int x = 0; x < topNodeCountX; x++)
                {
                    topNodes[x, y] = new Node(x * topNodeSize, y * topNodeSize, topNodeSize, ref createDescription);
                }
            }
        }

        public void Select(Selection selection)
        {
            for (int y = 0; y < topNodeCountY; y++)
            {
                for (int x = 0; x < topNodeCountX; x++)
                {
                    topNodes[x, y].Select(selection, false);
                }
            }
        }
    }
}
