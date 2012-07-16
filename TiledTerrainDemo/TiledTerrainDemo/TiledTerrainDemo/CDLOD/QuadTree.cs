#region Using

using System;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class QuadTree
    {
        int topNodeCountX;

        int topNodeCountY;

        Node[,] topNodes;

        public void Build(ref CreateDescription createDescription)
        {
            var topNodeSize = createDescription.Settings.LeafNodeSize;
            for (int i = 1; i < createDescription.Settings.LevelCount; i++)
                topNodeSize *= 2;

            topNodeCountX = (int) Math.Ceiling((createDescription.HeightMap.Width - 1) / (float) topNodeSize);
            topNodeCountY = (int) Math.Ceiling((createDescription.HeightMap.Height - 1) / (float) topNodeSize);

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
                    topNodes[x, y].Select(selection, false, false);
                }
            }
        }
    }
}
