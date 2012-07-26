#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledTerrainDemo.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class Terrain
    {
        Settings settings;

        QuadTree quadTree;

        public IHeightMapSource HeightMap { get; set; }

        public Terrain(Settings settings)
        {
            this.settings = settings;

            quadTree = new QuadTree(settings);
        }

        public void Build()
        {
            if (HeightMap == null)
                throw new InvalidOperationException("HeightMap is null.");

            // Build the quadtree.
            quadTree.Build(HeightMap);
        }

        public void Select(Selection selection)
        {
            // Prepare selection's state per a terrain.
            selection.ClearSelectedNodes();

            // Select visible nodes.
            quadTree.Select(selection);
        }
    }
}
