#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class QuadTree
    {
        Node root;

        public void Build(IHeightMapSource heightMap)
        {
            var rootSize = heightMap.Size - 1;
            root = new Node(null, 0, 0, rootSize, heightMap);

        }

        public bool Select(Selection selection)
        {
            // 複数の height map を利用することを想定し、
            // selection は呼出前に初期化されているものとする。

            return root.Select(selection);
        }
    }
}
