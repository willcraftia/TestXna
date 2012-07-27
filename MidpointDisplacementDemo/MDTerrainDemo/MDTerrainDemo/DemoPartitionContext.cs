#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Terrain.CDLOD;

#endregion

namespace MDTerrainDemo
{
    public sealed class DemoPartitionContext
    {
        CDLODSettings settings;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content { get; private set; }

        public CDLODSettings Settings
        {
            get { return settings; }
        }

        public DemoTerrainRenderer TerrainRenderer { get; private set; }

        public CDLODSelection Selection { get; private set; }

        public int MDSeed { get; set; }

        public float MDHurst { get; set; }

        #region Debug

        public int TotalSelectedNodeCount { get; set;}

        public int DrawPartitionCount { get; set; }

        #endregion

        public DemoPartitionContext(
            GraphicsDevice graphicsDevice, ContentManager content, CDLODSettings settings,
            ICDLODVisibleRanges visibleRanges)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            this.settings = settings;

            TerrainRenderer = new DemoTerrainRenderer(GraphicsDevice, Content, settings);
            TerrainRenderer.InitializeMorphConsts(visibleRanges);

            var heightColors = new HeightColorCollection();
            // default settings.
            heightColors.AddColor(-1.0000f, new Color(  0,   0, 128, 255));
            heightColors.AddColor(-0.2500f, new Color(  0,   0, 255, 255));
            heightColors.AddColor( 0.0000f, new Color(  0, 128, 255, 255));
            heightColors.AddColor( 0.0625f, new Color(240, 240,  64, 255));
            heightColors.AddColor( 0.1250f, new Color( 32, 160,   0, 255));
            heightColors.AddColor( 0.3750f, new Color(224, 224,   0, 255));
            heightColors.AddColor( 0.7500f, new Color(128, 128, 128, 255));
            heightColors.AddColor( 1.0000f, new Color(255, 255, 255, 255));

            TerrainRenderer.InitializeHeightColors(heightColors);

            Selection = new CDLODSelection(settings, visibleRanges);
        }
    }
}
