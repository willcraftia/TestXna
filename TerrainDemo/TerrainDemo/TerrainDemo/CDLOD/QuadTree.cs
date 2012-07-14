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

            topNodeCountX = (createDescription.HeightMap.Width - 1) / topNodeSize;
            topNodeCountY = (createDescription.HeightMap.Height - 1) / topNodeSize;
            
            // adjust top node counts when the top node size is over the height map size.
            if (createDescription.HeightMap.Width - 1 < topNodeSize)
                topNodeCountX++;
            if (createDescription.HeightMap.Height - 1 < topNodeSize)
                topNodeCountY++;

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
