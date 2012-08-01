#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Framework.Terrain;
using Willcraftia.Xna.Framework.Terrain.CDLOD;
using Willcraftia.Xna.Framework.Terrain.Erosion;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoPartition : Partition
    {
        DemoPartitionContext context;

        CDLODSettings settings;

        NoiseHeightMap heightMap;

        NormalMap normalMap;

        NoiseRainMap noiseRainMap;

        Map<float> uniformRainMap;

        Map<float> terrainRainMap;

        Perlin rainNoise = new Perlin();

        HydraulicErosion hydraulicErosion = new HydraulicErosion();

        FastHydraulicErosion fastHydraulicErosion = new FastHydraulicErosion();

        MusgraveHydraulicErosion musgraveHydraulicErosion = new MusgraveHydraulicErosion();

        ThermalErosion thermalErosion = new ThermalErosion();

        FastThermalErosion fastThermalErosion = new FastThermalErosion();

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

            noiseRainMap = new NoiseRainMap(settings.HeightMapWidth, settings.HeightMapHeight);
            noiseRainMap.NoiseSource = rainNoise.Sample;
            noiseRainMap.RainAmount = 0.5f;

            uniformRainMap = new Map<float>(settings.HeightMapWidth, settings.HeightMapHeight);
            uniformRainMap.Fill(0.8f);

            terrainRainMap = new Map<float>(settings.HeightMapWidth, settings.HeightMapHeight);

            hydraulicErosion.HeightMap = heightMap;
            //hydraulicErosion.RainMap = noiseRainMap;
            //hydraulicErosion.RainMap = uniformRainMap;
            hydraulicErosion.RainMap = terrainRainMap;

            fastHydraulicErosion.HeightMap = heightMap;
            //fastHydraulicErosion.RainMap = noiseRainMap;
            //fastHydraulicErosion.RainMap = uniformRainMap;
            fastHydraulicErosion.RainMap = terrainRainMap;

            musgraveHydraulicErosion.HeightMap = heightMap;
            //musgraveHydraulicErosion.RainMap = noiseRainMap;
            //musgraveHydraulicErosion.RainMap = uniformRainMap;
            musgraveHydraulicErosion.RainMap = terrainRainMap;

            thermalErosion.HeightMap = heightMap;

            fastThermalErosion.HeightMap = heightMap;

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

            // Erode.
            noiseRainMap.Bounds = new Bounds
            {
                X = context.NoiseMinX + X * context.NoiseWidth,
                Y = context.NoiseMinY + Y * context.NoiseHeight,
                Width = context.NoiseWidth * 0.5f,
                Height = context.NoiseHeight * 0.5f
            };
            noiseRainMap.Build();

            heightMap.CopyTo(terrainRainMap);
            terrainRainMap.Normalize().Clamp();

            //hydraulicErosion.Build();
            //fastHydraulicErosion.Build();
            //musgraveHydraulicErosion.Build();

            thermalErosion.Build();
            //fastThermalErosion.Build();
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
