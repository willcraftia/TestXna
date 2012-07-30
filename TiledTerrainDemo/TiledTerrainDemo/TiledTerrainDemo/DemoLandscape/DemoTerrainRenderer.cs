#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Terrain.CDLOD;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoTerrainRenderer : IDisposable
    {
        CDLODSettings settings;

        CDLODTerrainRenderer renderer;

        Effect sourceEffect;

        TerrainEffect effect;

        Vector3 ambientLightColor = new Vector3(0.6f);

        Vector3 lightDirection = Vector3.One;

        Vector3 diffuseLightColor = Vector3.One;

        Vector3 fogColor = Vector3.One;

        BoundingBoxDrawer boundingBoxDrawer;

        BasicEffect debugEffect;

        Color[] debugLevelColors = new Color[]
        {
            Color.White,
            new Color(1, 0.2f, 0.2f, 1),
            new Color(0.2f, 1, 0.2f, 1),
            new Color(0.2f, 0.2f, 1, 1)
        };

        Vector4[] heightColorBuffer = new Vector4[TerrainEffect.DefinedMaxColorCount];

        float[] heightColorPositionBuffer = new float[TerrainEffect.DefinedMaxColorCount];

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content { get; private set; }

        public CDLODSettings Settings
        {
            get { return settings; }
        }

        public bool FogEnabled { get; set; }

        public float FogStart { get; set; }

        public float FogEnd { get; set; }

        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; }
        }

        public TerrainRenderMode RenderMode { get; set; }

        public bool WireframeVisible { get; set; }

        public bool NodeBoundingBoxVisible { get; set; }

        public bool LightEnabled { get; set; }

        public Vector3 LightDirection
        {
            get { return lightDirection; }
            set
            {
                lightDirection = value;

                if (lightDirection.X != 0 || lightDirection.Y != 0 || lightDirection.Z != 0)
                    lightDirection.Normalize();
            }
        }

        public DemoTerrainRenderer(GraphicsDevice graphicsDevice, ContentManager content, CDLODSettings settings)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            this.settings = settings;

            renderer = new CDLODTerrainRenderer(graphicsDevice, settings);

            sourceEffect = Content.Load<Effect>("TerrainEffect");
            effect = new TerrainEffect(sourceEffect);
            effect.LevelCount = settings.LevelCount;
            effect.TerrainScale = settings.TerrainScale;
            effect.PatchGridSize = renderer.PatchGridSize;
            effect.HeightMapSize = new Vector2(settings.HeightMapWidth, settings.HeightMapHeight);
            effect.DiffuseMap0 = Content.Load<Texture2D>("Textures/DiffuseMap0");
            effect.DiffuseMap1 = Content.Load<Texture2D>("Textures/DiffuseMap1");
            effect.DiffuseMap2 = Content.Load<Texture2D>("Textures/DiffuseMap2");
            effect.DiffuseMap3 = Content.Load<Texture2D>("Textures/DiffuseMap3");
            effect.SetDiffuseRange0(-settings.HeightScale * 1.5f, -settings.HeightScale * 0.5f);
            effect.SetDiffuseRange1(-settings.HeightScale * 0.5f, 0);
            effect.SetDiffuseRange2(0, settings.HeightScale * 0.5f);
            effect.SetDiffuseRange3(settings.HeightScale * 0.5f, settings.HeightScale * 1.5f);
            effect.DiffuseMapScale = settings.MapScale;

            lightDirection = new Vector3(0, -1, -1);
            lightDirection.Normalize();

            boundingBoxDrawer = new BoundingBoxDrawer(GraphicsDevice);
            debugEffect = new BasicEffect(GraphicsDevice);
            debugEffect.AmbientLightColor = Vector3.One;
            debugEffect.VertexColorEnabled = true;

            RenderMode = TerrainRenderMode.HeightColor;
            LightEnabled = true;
        }

        // Invoke this method if the state of a IVisibleRanges instance is changed.
        public void InitializeMorphConsts(ICDLODVisibleRanges visibleRanges)
        {
            Vector2[] morphConsts;
            CDLODMorphConsts.Create(visibleRanges, out morphConsts);
            effect.MorphConsts = morphConsts;
        }

        public void InitializeHeightColors(HeightColorCollection heightColors)
        {
            for (int i = 0; i < heightColors.Count; i++)
            {
                var hc = heightColors[i];
                heightColorBuffer[i] = hc.Color;
                heightColorPositionBuffer[i] = hc.Position;
            }

            effect.HeightColorCount = heightColors.Count;
            effect.HeightColors = heightColorBuffer;
            effect.HeightColorPositions = heightColorPositionBuffer;
        }

        public void Draw(GameTime gameTime, CDLODSelection selection)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Prepare effect parameters.

            // Get the eye position in world space.
            Vector3 eyePosition;
            View.GetEyePosition(ref selection.View, out eyePosition);

            // Calcualte the eye position in terrain space.
            Vector3 terrainEyePosition = eyePosition - selection.TerrainOffset;

            // Calculate the view matrix in terrain space.
            Matrix inverseView;
            Matrix.Invert(ref selection.View, out inverseView);
            inverseView.Translation = terrainEyePosition;
            Matrix terrainView;
            Matrix.Invert(ref inverseView, out terrainView);

            // per a selection (a terrain).
            effect.HeightMap = selection.HeightMapTexture;
            effect.NormalMap = selection.NormalMapTexture;
            effect.Projection = selection.Projection;
            effect.TerrainEyePosition = terrainEyePosition;
            effect.TerrainView = terrainView;

            // render settings.
            effect.AmbientLightColor = ambientLightColor;
            effect.LightDirection = lightDirection;
            effect.DiffuseLightColor = diffuseLightColor;
            effect.LightEnabled = LightEnabled;
            effect.FogEnabled = FogEnabled;
            effect.FogStart = FogStart;
            effect.FogEnd = FogEnd;
            effect.FogColor = fogColor;

            switch (RenderMode)
            {
                case TerrainRenderMode.Texture:
                    sourceEffect.CurrentTechnique = effect.TextureTequnique;
                    break;
                case TerrainRenderMode.WhiteSolid:
                    sourceEffect.CurrentTechnique = effect.WhiteSolidTequnique;
                    break;
                case TerrainRenderMode.Normal:
                    sourceEffect.CurrentTechnique = effect.NormalTequnique;
                    break;
                case TerrainRenderMode.HeightColor:
                default:
                    sourceEffect.CurrentTechnique = effect.HeightColorTequnique;
                    break;
            }
            renderer.Draw(gameTime, sourceEffect, selection);

            // Wireframe tequnique
            if (WireframeVisible)
            {
                sourceEffect.CurrentTechnique = effect.WireframeTequnique;
                renderer.Draw(gameTime, sourceEffect, selection);
            }

            if (NodeBoundingBoxVisible)
            {
                debugEffect.View = selection.View;
                debugEffect.Projection = selection.Projection;

                CDLODSelectedNode selectedNode;
                BoundingBox box;
                for (int i = 0; i < selection.SelectedNodeCount; i++)
                {
                    selection.GetSelectedNode(i, out selectedNode);

                    selectedNode.GetBoundingBox(
                        ref selection.TerrainOffset, settings.MapScale, settings.HeightScale, out box);
                    var level = selectedNode.Level;
                    level %= 4;
                    boundingBoxDrawer.Draw(ref box, debugEffect, ref debugLevelColors[level]);
                }
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~DemoTerrainRenderer()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                renderer.Dispose();
                sourceEffect.Dispose();
                debugEffect.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
