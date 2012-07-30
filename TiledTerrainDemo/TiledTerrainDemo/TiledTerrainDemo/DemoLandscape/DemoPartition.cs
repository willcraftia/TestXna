#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Framework.Terrain.CDLOD;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoPartition : Partition
    {
        DemoPartitionContext context;

        CDLODSettings settings;

        NoiseHeightMap heightMap;

        NormalMap normalMap;

        CDLODTerrain terrain;

        public DemoPartition(DemoPartitionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;

            settings = context.Settings;

            heightMap = new NoiseHeightMap(context.GraphicsDevice, settings.HeightMapWidth, settings.HeightMapHeight);
            heightMap.NoiseSource = context.Noise;

            normalMap = new NormalMap(context.GraphicsDevice, settings.HeightMapWidth, settings.HeightMapHeight);
            normalMap.HeightMap = heightMap;
            normalMap.Amplitude = settings.HeightScale;

            terrain = new CDLODTerrain(context.Settings);
            terrain.HeightMap = heightMap;
        }

        public override void NeighborLoaded(Partition neighbor)
        {
            var n = neighbor as DemoPartition;

            if (n.Y == Y)
            {
                if (n.X == X - 1)
                {
                    heightMap.MergeLeftNeighbor(n.heightMap);
                    normalMap.MergeLeftNeighbor(n.normalMap);
                }
                else if (n.X == X + 1)
                {
                    heightMap.MergeRightNeighbor(n.heightMap);
                    normalMap.MergeRightNeighbor(n.normalMap);
                }
                else
                {
                    throw new InvalidOperationException("The specified partition is not a neighbor.");
                }
            }
            else if (n.X == X)
            {
                if (n.Y == Y - 1)
                {
                    heightMap.MergeTopNeighbor(n.heightMap);
                    normalMap.MergeTopNeighbor(n.normalMap);
                }
                else if (n.Y == Y + 1)
                {
                    heightMap.MergeBottomNeighbor(n.heightMap);
                    normalMap.MergeBottomNeighbor(n.normalMap);
                }
                else
                {
                    throw new InvalidOperationException("The specified partition is not a neighbor.");
                }
            }
            else
            {
                throw new InvalidOperationException("The specified partition is not a neighbor.");
            }

            base.NeighborLoaded(neighbor);
        }

        public override void Draw(GameTime gameTime)
        {
            var partitionPosition = Position;
            context.Selection.TerrainOffset = new Vector3(partitionPosition.X, 0, partitionPosition.Y);

            // select nodes.
            terrain.Select(context.Selection);

            // set the current height map texture.
            context.Selection.HeightMapTexture = heightMap.Texture;
            // set the current normal map texture.
            context.Selection.NormalMapTexture = normalMap.Texture;

            #region Debug

            context.TotalSelectedNodeCount += context.Selection.SelectedNodeCount;
            context.DrawPartitionCount += 1;

            #endregion

            context.TerrainRenderer.Draw(gameTime, context.Selection);

            // unbind.
            context.Selection.HeightMapTexture = null;
            context.Selection.NormalMapTexture = null;
        }

        protected override void LoadContentOverride()
        {
            LoadHeightMap();
            LoadNormalMap();
            LoadTerrain();

            base.LoadContentOverride();
        }

        void LoadHeightMap()
        {
            // Build noise values in the noise bounds of this partition.
            heightMap.Bounds = new Bounds
            {
                X = context.NoiseMinX + X * context.NoiseWidth,
                Y = context.NoiseMinY + Y * context.NoiseHeight,
                Width = context.NoiseWidth,
                Height = context.NoiseHeight
            };
            heightMap.Build();
        }

        void LoadNormalMap()
        {
            normalMap.Build();
        }

        void LoadTerrain()
        {
            terrain.Build();
        }

        protected override void DisposeOverride(bool disposing)
        {
            if (disposing)
            {
                heightMap.Dispose();
                normalMap.Dispose();
            }

            base.DisposeOverride(disposing);
        }
    }
}
