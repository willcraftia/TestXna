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

        Rgba64[] normalMap;

        CDLODTerrain terrain;

        Texture2D normalMapTexture;

        bool normalMapDirty;

        public DemoPartition(DemoPartitionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;

            settings = context.Settings;

            heightMap = new NoiseHeightMap(context.GraphicsDevice, settings);
            heightMap.NoiseSource = context.Noise;

            normalMap = new Rgba64[settings.HeightMapWidth * settings.HeightMapHeight];
            normalMapTexture = new Texture2D(
                context.GraphicsDevice, settings.HeightMapWidth, settings.HeightMapHeight, false, SurfaceFormat.Rgba64);

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
                    // Left
                    for (int y = 0; y < settings.HeightMapHeight; y++)
                        MergeHeight(heightMap, 0, y, n.heightMap, settings.HeightMapWidth - 1, y);
                }
                else if (n.X == X + 1)
                {
                    // Right
                    for (int y = 0; y < settings.HeightMapHeight; y++)
                        MergeHeight(heightMap, settings.HeightMapWidth - 1, y, n.heightMap, 0, y);
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
                    // Top
                    for (int x = 0; x < settings.HeightMapWidth; x++)
                        MergeHeight(heightMap, x, 0, n.heightMap, x, settings.HeightMapHeight - 1);
                }
                else if (n.Y == Y + 1)
                {
                    // Bottom
                    for (int x = 0; x < settings.HeightMapWidth; x++)
                        MergeHeight(heightMap, x, settings.HeightMapHeight - 1, n.heightMap, x, 0);
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

        void MergeHeight(IMap map0, int x0, int y0, IMap map1, int x1, int y1)
        {
            var h0 = map0[x0, y0];
            var h1 = map1[x1, y1];
            var h = (h0 + h1) * 0.5f;
            map0[x0, y0] = h;
            map1[x1, y1] = h;
        }

        public override void Draw(GameTime gameTime)
        {
            var partitionPosition = Position;
            context.Selection.TerrainOffset = new Vector3(partitionPosition.X, 0, partitionPosition.Y);

            // select.
            terrain.Select(context.Selection);

            // set the height map texture.
            context.Selection.HeightMapTexture = heightMap.Texture;

            if (normalMapDirty)
            {
                normalMapTexture.SetData(normalMap);
                normalMapDirty = false;
            }
            context.Selection.NormalMapTexture = normalMapTexture;

            #region Debug

            context.TotalSelectedNodeCount += context.Selection.SelectedNodeCount;
            context.DrawPartitionCount += 1;

            #endregion

            context.TerrainRenderer.Draw(gameTime, context.Selection);
        }

        protected override void LoadContentOverride()
        {
            // Build noise values with the noise bounds for this partition.
            heightMap.Bounds = new Bounds
            {
                X = context.NoiseMinX + X * context.NoiseWidth,
                Y = context.NoiseMinY + Y * context.NoiseHeight,
                Width = context.NoiseWidth,
                Height = context.NoiseHeight
            };
            heightMap.Build();

            BuildNormalMap();

            // Build the terrain.
            terrain.Build();

            base.LoadContentOverride();
        }

        void BuildNormalMap()
        {
            var heightScale = settings.HeightScale;
            // todo: 名前が悪い。Patch scale を意味した値ではない。
            var patchScale = settings.PatchScale;

            for (int y = 0; y < heightMap.Height; y++)
            {
                for (int x = 0; x < heightMap.Width; x++)
                {
                    var n = heightMap[x, y - 1] * heightScale;
                    var s = heightMap[x, y + 1] * heightScale;
                    var e = heightMap[x - 1, y] * heightScale;
                    var w = heightMap[x + 1, y] * heightScale;

                    var sn = new Vector3(0, 2 * patchScale, s - n);
                    var ew = new Vector3(2 * patchScale, 0, e - w);
                    sn.Normalize();
                    ew.Normalize();

                    Vector3 normal;
                    Vector3.Cross(ref sn, ref ew, out normal);
                    normal.Normalize();

                    // [-1, 1] -> [0, 1]
                    normal.X = normal.X * 0.5f + 0.5f;
                    normal.Y = normal.Y * 0.5f + 0.5f;
                    normal.Z = normal.Z * 0.5f + 0.5f;

                    normalMap[x + y * heightMap.Width] = new Rgba64(normal.X, normal.Y, normal.Z, 0);
                }
            }

            normalMapDirty = true;
        }

        protected override void DisposeOverride(bool disposing)
        {
            if (disposing)
            {
                heightMap.Dispose();
            }

            base.DisposeOverride(disposing);
        }
    }
}
