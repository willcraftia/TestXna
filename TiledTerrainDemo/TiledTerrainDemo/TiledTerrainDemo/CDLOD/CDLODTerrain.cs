#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledTerrainDemo.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class CDLODTerrain
    {
        CDLODSettings settings;

        QuadTree quadTree;

        public ICDLODHeightMap HeightMap { get; set; }

        public CDLODTerrain(CDLODSettings settings)
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

        public void Select(CDLODSelection selection)
        {
            // Prepare selection's state per a terrain.
            selection.ClearSelectedNodes();

            // Select visible nodes.
            quadTree.Select(selection);
        }
    }
}
