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

            heightMap = new NoiseHeightMap(context.GraphicsDevice, settings);
            heightMap.NoiseSource = context.Noise;

            normalMap = new NormalMap(context.GraphicsDevice, settings.HeightMapWidth, settings.HeightMapHeight);

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
                    MergeLeftNeighbor(n);
                }
                else if (n.X == X + 1)
                {
                    // Right
                    MergeRightNeighbor(n);
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
                    MergeTopNeighbor(n);
                }
                else if (n.Y == Y + 1)
                {
                    // Bottom
                    MergeBottomNeighbor(n);
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

            heightMap.textureDirty = true;
            normalMap.textureDirty = true;

            //heightMap.RefreshTexture();
            //normalMap.RefreshTexture();

            base.NeighborLoaded(neighbor);
        }

        void MergeHeight(IMap<float> map0, int x0, int y0, IMap<float> map1, int x1, int y1)
        {
            var h0 = map0[x0, y0];
            var h1 = map1[x1, y1];
            var h = (h0 + h1) * 0.5f;
            map0[x0, y0] = h;
            map1[x1, y1] = h;
            //map1[x1, y1] = map0[x0, y0];
        }

        void MergeLeftNeighbor(DemoPartition partition)
        {
            var amplitude = settings.HeightScale;
            var rightIndex = settings.HeightMapWidth - 1;

            for (int y = 0; y < settings.HeightMapHeight; y++)
            {
                MergeHeight(heightMap, 0, y, partition.heightMap, rightIndex, y);
            }
            for (int y = 0; y < settings.HeightMapHeight; y++)
            {
                var h0 = heightMap[0, y - 1] * amplitude;
                var h1 = heightMap[0, y + 1] * amplitude;
                var h2 = partition.heightMap[rightIndex - 1, y] * amplitude;
                var h3 = heightMap[0 + 1, y] * amplitude;

                var d0 = new Vector3(0, 1, h1 - h0);
                var d1 = new Vector3(1, 0, h3 - h2);
                d0.Normalize();
                d1.Normalize();

                Vector3 normal;
                Vector3.Cross(ref d0, ref d1, out normal);
                normal.Normalize();

                // [-1, 1] -> [0, 1]
                normal.X = normal.X * 0.5f + 0.5f;
                normal.Y = normal.Y * 0.5f + 0.5f;
                normal.Z = normal.Z * 0.5f + 0.5f;

                var n = new Rgba64(normal.X, normal.Y, normal.Z, 0);
                normalMap[0, y] = n;
                partition.normalMap[rightIndex, y] = n;

                //normalMap[0, y] = new Rgba64(1, 0, 0, 0);
                //partition.normalMap[rightIndex, y] = new Rgba64(1, 1, 1, 0);
            }
        }

        void MergeRightNeighbor(DemoPartition partition)
        {
            var amplitude = settings.HeightScale;
            var rightIndex = settings.HeightMapWidth - 1;

            for (int y = 0; y < settings.HeightMapHeight; y++)
            {
                MergeHeight(heightMap, rightIndex, y, partition.heightMap, 0, y);
            }
            for (int y = 0; y < settings.HeightMapHeight; y++)
            {
                var h0 = heightMap[rightIndex, y - 1] * amplitude;
                var h1 = heightMap[rightIndex, y + 1] * amplitude;
                var h2 = heightMap[rightIndex - 1, y] * amplitude;
                var h3 = partition.heightMap[0 + 1, y] * amplitude;

                var d0 = new Vector3(0, 1, h1 - h0);
                var d1 = new Vector3(1, 0, h3 - h2);
                d0.Normalize();
                d1.Normalize();

                Vector3 normal;
                Vector3.Cross(ref d0, ref d1, out normal);
                normal.Normalize();

                // [-1, 1] -> [0, 1]
                normal.X = normal.X * 0.5f + 0.5f;
                normal.Y = normal.Y * 0.5f + 0.5f;
                normal.Z = normal.Z * 0.5f + 0.5f;

                var n = new Rgba64(normal.X, normal.Y, normal.Z, 0);
                normalMap[rightIndex, y] = n;
                partition.normalMap[0, y] = n;

                //normalMap[rightIndex, y] = new Rgba64(0, 1, 0, 0);
                //partition.normalMap[0, y] = new Rgba64(1, 1, 1, 0);
            }
        }

        void MergeTopNeighbor(DemoPartition partition)
        {
            var amplitude = settings.HeightScale;
            var bottomIndex = settings.HeightMapHeight - 1;

            for (int x = 0; x < settings.HeightMapWidth; x++)
            {
                MergeHeight(heightMap, x, 0, partition.heightMap, x, bottomIndex);
            }
            for (int x = 0; x < settings.HeightMapWidth; x++)
            {
                var h0 = partition.heightMap[x, bottomIndex - 1] * amplitude;
                var h1 = heightMap[x, 0 + 1] * amplitude;
                var h2 = heightMap[x - 1, 0] * amplitude;
                var h3 = heightMap[x + 1, 0] * amplitude;

                var d0 = new Vector3(0, 1, h1 - h0);
                var d1 = new Vector3(1, 0, h3 - h2);
                d0.Normalize();
                d1.Normalize();

                Vector3 normal;
                Vector3.Cross(ref d0, ref d1, out normal);
                normal.Normalize();

                // [-1, 1] -> [0, 1]
                normal.X = normal.X * 0.5f + 0.5f;
                normal.Y = normal.Y * 0.5f + 0.5f;
                normal.Z = normal.Z * 0.5f + 0.5f;

                var n = new Rgba64(normal.X, normal.Y, normal.Z, 0);
                normalMap[x, 0] = n;
                partition.normalMap[x, bottomIndex] = n;

                //normalMap[x, 0] = new Rgba64(0, 0, 1, 0);
                //partition.normalMap[x, bottomIndex] = new Rgba64(1, 1, 1, 0);
            }
        }

        void MergeBottomNeighbor(DemoPartition partition)
        {
            var amplitude = settings.HeightScale;
            var bottomIndex = settings.HeightMapHeight - 1;

            for (int x = 0; x < settings.HeightMapWidth; x++)
            {
                MergeHeight(heightMap, x, bottomIndex, partition.heightMap, x, 0);
            }
            for (int x = 0; x < settings.HeightMapWidth; x++)
            {
                var h0 = heightMap[x, bottomIndex - 1] * amplitude;
                var h1 = partition.heightMap[x, 0 + 1] * amplitude;
                var h2 = heightMap[x - 1, bottomIndex] * amplitude;
                var h3 = heightMap[x + 1, bottomIndex] * amplitude;

                var d0 = new Vector3(0, 1, h1 - h0);
                var d1 = new Vector3(1, 0, h3 - h2);
                d0.Normalize();
                d1.Normalize();

                Vector3 normal;
                Vector3.Cross(ref d0, ref d1, out normal);
                normal.Normalize();

                // [-1, 1] -> [0, 1]
                normal.X = normal.X * 0.5f + 0.5f;
                normal.Y = normal.Y * 0.5f + 0.5f;
                normal.Z = normal.Z * 0.5f + 0.5f;

                var n = new Rgba64(normal.X, normal.Y, normal.Z, 0);
                normalMap[x, bottomIndex] = n;
                partition.normalMap[x, 0] = n;

                //normalMap[x, bottomIndex] = new Rgba64(1, 1, 0, 0);
                //partition.normalMap[x, 0] = new Rgba64(1, 1, 1, 0);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var partitionPosition = Position;
            context.Selection.TerrainOffset = new Vector3(partitionPosition.X, 0, partitionPosition.Y);

            // select.
            terrain.Select(context.Selection);

            // set height map texture.
            context.Selection.HeightMapTexture = heightMap.Texture;
            // set normal map texture.
            context.Selection.NormalMapTexture = normalMap.Texture;

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

            // Build the normal map.
            normalMap.Build(heightMap, settings.HeightScale);

            // Build the terrain.
            terrain.Build();

            base.LoadContentOverride();
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
