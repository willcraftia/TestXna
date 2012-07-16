#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledTerrainDemo.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class TerrainRenderer
    {
        public const int DefaultPatchResolution = 2;

        Settings settings;

        PatchInstanceVertex[] instances = new PatchInstanceVertex[Selection.MaxSelectedNodeCount];

        /// <summary>
        /// The vertex buffer to populate instances.
        /// </summary>
        WritableVertexBuffer<PatchInstanceVertex> instanceVertexBuffer;

        /// <summary>
        /// Vertex bindings.
        /// </summary>
        VertexBufferBinding[] vertexBufferBindings = new VertexBufferBinding[2];

        /// <summary>
        /// A patch mesh.
        /// </summary>
        PatchMesh patchMesh;

        Effect sourceEffect;

        TerrainEffect effect;

        Vector3 ambientLightColor = new Vector3(0.6f);

        Vector3 lightDirection = Vector3.One;

        Vector3 diffuseLightColor = Vector3.One;

        BoundingBoxDrawer boundingBoxDrawer;

        BasicEffect debugEffect;

        Color[] debugLevelColors = new Color[]
        {
            Color.White,
            new Color(1, 0.2f, 0.2f, 1),
            new Color(0.2f, 1, 0.2f, 1),
            new Color(0.2f, 0.2f, 1, 1)
        };

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content { get; private set; }

        public Settings Settings
        {
            get { return settings; }
        }

        public bool WhiteSolidVisible { get; set; }

        public bool HeightColorVisible { get; set; }

        public bool WireframeVisible { get; set; }

        public bool NodeBoundingBoxVisible { get; set; }

        public bool LightEnabled { get; set; }

        public float WireframeGap { get; set; }

        public TerrainRenderer(GraphicsDevice graphicsDevice, ContentManager content, Settings settings)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            this.settings = settings;

            instanceVertexBuffer = new WritableVertexBuffer<PatchInstanceVertex>(GraphicsDevice, Selection.MaxSelectedNodeCount * 2);

            // TODO: I want to change a patch resolution at runtime.
            // patchGridSize = leafNodeSize * patchResolution;
            patchMesh = new PatchMesh(GraphicsDevice, settings.LeafNodeSize * DefaultPatchResolution);

            sourceEffect = Content.Load<Effect>("TerrainEffect");
            effect = new TerrainEffect(sourceEffect);
            effect.PatchGridSize = patchMesh.GridSize;
            effect.LevelCount = settings.LevelCount;

            lightDirection = new Vector3(0, -1, -1);
            lightDirection.Normalize();

            boundingBoxDrawer = new BoundingBoxDrawer(GraphicsDevice);
            debugEffect = new BasicEffect(GraphicsDevice);
            debugEffect.AmbientLightColor = Vector3.One;
            debugEffect.VertexColorEnabled = true;

            HeightColorVisible = true;
            LightEnabled = true;

            WireframeGap = 0.01f;
        }

        // Invoke this method if the state of a IVisibleRanges instance is changed.
        public void InitializeMorphConsts(IVisibleRanges visibleRanges)
        {
            Vector2[] morphConsts;
            MorphConsts.Create(visibleRanges, out morphConsts);
            effect.MorphConsts = morphConsts;
        }

        public void Draw(GameTime gameTime, Selection selection)
        {
            if (selection.SelectedNodeCount == 0) return;

            // create instances
            for (int i = 0; i < selection.SelectedNodeCount; i++)
                selection.GetPatchInstanceVertex(i, out instances[i]);

            var offset = instanceVertexBuffer.SetData(instances, 0, selection.SelectedNodeCount);

            vertexBufferBindings[0] = new VertexBufferBinding(patchMesh.VertexBuffer, 0);
            vertexBufferBindings[1] = new VertexBufferBinding(instanceVertexBuffer.VertexBuffer, offset, 1);

            GraphicsDevice.SetVertexBuffers(vertexBufferBindings);
            GraphicsDevice.Indices = patchMesh.IndexBuffer;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Prepare effect parameters.
            // per a selection (a terrain).
            effect.TerrainOffset = selection.TerrainOffset;
            effect.TerrainScale = selection.TerrainScale;
            effect.HeightMap = selection.HeightMapTexture;
            effect.View = selection.View;
            effect.Projection = selection.Projection;
            // render settings.
            effect.AmbientLightColor = ambientLightColor;
            effect.LightDirection = lightDirection;
            effect.DiffuseLightColor = diffuseLightColor;
            effect.LightEnabled = LightEnabled;

            // WhiteSolid tequnique
            if (WhiteSolidVisible)
                DrawPatchInstances(effect.WhiteSolidTequnique, selection.SelectedNodeCount);

            // HeightColor tequnique
            if (HeightColorVisible)
                DrawPatchInstances(effect.HeightColorTequnique, selection.SelectedNodeCount);

            // Wireframe tequnique
            if (WireframeVisible)
            {
                var wireframeTerrainOffset = selection.TerrainOffset;
                wireframeTerrainOffset.Y += WireframeGap;
                effect.TerrainOffset = wireframeTerrainOffset;
                DrawPatchInstances(effect.WireframeTequnique, selection.SelectedNodeCount);
                effect.TerrainOffset = selection.TerrainOffset;
            }

            if (NodeBoundingBoxVisible)
            {
                debugEffect.View = selection.View;
                debugEffect.Projection = selection.Projection;

                SelectedNode selectedNode;
                BoundingBox box;
                for (int i = 0; i < selection.SelectedNodeCount; i++)
                {
                    selection.GetSelectedNode(i, out selectedNode);

                    selectedNode.GetBoundingBox(
                        ref selection.TerrainOffset, settings.PatchScale, settings.HeightScale, out box);
                    var level = selectedNode.Level;
                    level %= 4;
                    boundingBoxDrawer.Draw(ref box, debugEffect, ref debugLevelColors[level]);
                }
            }
        }

        void DrawPatchInstances(EffectTechnique technique, int selectedNodeCount)
        {
            effect.CurrentTechnique = technique;
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawInstancedPrimitives(
                    PrimitiveType.TriangleList, 0, 0,
                    patchMesh.NumVertices, 0, patchMesh.PrimitiveCount, selectedNodeCount);
            }
        }
    }
}
