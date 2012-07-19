#region Using

using System;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class QuadTree
    {
        int topNodeSize;

        int topNodeCountX;

        int topNodeCountY;

        Node[,] topNodes;

        public QuadTree(Settings settings)
        {
            topNodeSize = settings.TopNodeSize;

            topNodeCountX = (int) Math.Ceiling((settings.HeightMapWidth - 1) / (float) topNodeSize);
            topNodeCountY = (int) Math.Ceiling((settings.HeightMapHeight - 1) / (float) topNodeSize);

            topNodes = new Node[topNodeCountX, topNodeCountY];
            for (int y = 0; y < topNodeCountY; y++)
                for (int x = 0; x < topNodeCountX; x++)
                    topNodes[x, y] = new Node(x * topNodeSize, y * topNodeSize, topNodeSize, ref settings);
        }

        public void Build(IHeightMapSource heightMap)
        {
            for (int y = 0; y < topNodeCountY; y++)
                for (int x = 0; x < topNodeCountX; x++)
                    topNodes[x, y].Build(heightMap);
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
