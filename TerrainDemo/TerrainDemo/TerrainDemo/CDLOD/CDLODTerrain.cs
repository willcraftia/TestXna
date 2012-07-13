#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TerrainDemo.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class CDLODTerrain : IDisposable
    {
        const int maxSelectedNodeCount = 4000;

        const float visibilityDistance = 20000;

        const float morphStartRatio = 0.66f;

        public const int DefaultLeafNodeSize = 8;

        public const int DefaultPatchResolution = 2;

        public const int DefaultLevelCount = 7;

        public const float DefaultPatchScale = 2;

        public const float DefaultHeightScale = 50;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content { get; private set; }

        int leafNodeSize;

        // patchGridSize = leafNodeSize * patchResolution;
        int patchResolution;

        int levelCount;

        float patchScale;

        float heightScale;

        Vector3 terrainOffset;

        // 後で複数マップへの対応をする。
        QuadTree quadTree = new QuadTree();

        PatchInstanceVertex[] instances = new PatchInstanceVertex[maxSelectedNodeCount];

        /// <summary>
        /// HW インスタンスで使うインスタンス情報のための頂点バッファ。
        /// </summary>
        WritableVertexBuffer<PatchInstanceVertex> instanceVertexBuffer;

        /// <summary>
        /// 頂点バインディング情報。
        /// </summary>
        VertexBufferBinding[] vertexBufferBindings = new VertexBufferBinding[2];

        PatchMesh patchMesh;

        Selection selection = new Selection();

        Effect sourceEffect;

        CDLODTerrainEffect effect;

        Matrix view;

        Matrix projection;

        Vector3 ambientLightColor = Vector3.Zero;

        Vector3 lightDirection = Vector3.One;

        Vector3 diffuseLightColor = Vector3.One;

        Texture2D heightMapTexture;

        Color[] debugLevelColors = new Color[]
        {
            Color.White,
            new Color(1, 0.2f, 0.2f, 1),
            new Color(0.2f, 1, 0.2f, 1),
            new Color(0.2f, 0.2f, 1, 1)
        };

        BoundingBoxDrawer boundingBoxDrawer;

        BasicEffect debugEffect;

        public Vector3 TerrainOffset
        {
            get { return terrainOffset; }
            set { terrainOffset = value; }
        }

        public float PatchScale
        {
            get { return patchScale; }
            set { patchScale = value; }
        }

        public float HeightScale
        {
            get { return heightScale; }
            set { heightScale = value; }
        }

        public Morph Morph
        {
            get { return selection.Morph; }
            set { selection.Morph = value; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public int SelectedNodeCount
        {
            get { return selection.SelectedNodeCount; }
        }

        public bool WhiteSolidVisible { get; set; }

        public bool HeightColorVisible { get; set; }

        public bool WireframeVisible { get; set; }

        public bool NodeBoundingBoxVisible { get; set; }

        public bool LightEnabled { get; set; }

        public CDLODTerrain(GraphicsDevice graphicsDevice, ContentManager content)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;

            leafNodeSize = DefaultLeafNodeSize;
            patchResolution = DefaultPatchResolution;
            levelCount = DefaultLevelCount;
            patchScale = DefaultPatchScale;
            heightScale = DefaultHeightScale;

            lightDirection = new Vector3(0, -1, -1);
            lightDirection.Normalize();

            HeightColorVisible = true;
            LightEnabled = true;
        }

        public void Initialize(IHeightMapSource heightMap)
        {
            var createDescription = new CreateDescription
            {
                LeafNodeSize = leafNodeSize,
                LevelCount = levelCount,
                HeightMap = heightMap
            };

            quadTree.Build(ref createDescription);

            instanceVertexBuffer = new WritableVertexBuffer<PatchInstanceVertex>(GraphicsDevice, maxSelectedNodeCount * 2);

            selection.PatchScale = patchScale;
            selection.HeightScale = heightScale;
            selection.MaxSelectedNodeCount = maxSelectedNodeCount;
            selection.TerrainOffset = terrainOffset;
            selection.SelectedNodes = new SelectedNode[maxSelectedNodeCount];

            if (selection.Morph == null)
            {
                var defaultMorph = new DefaultMorph(levelCount);
                defaultMorph.VisibilityDistance = visibilityDistance;
                defaultMorph.MorphStartRatio = morphStartRatio;
                selection.Morph = defaultMorph;
            }

            if (!selection.Morph.Initialized)
                selection.Morph.Initialize();

            InitializeTextures(heightMap);

            patchMesh = new PatchMesh(GraphicsDevice, leafNodeSize * patchResolution);

            sourceEffect = Content.Load<Effect>("CDLODTerrainEffect");
            effect = new CDLODTerrainEffect(sourceEffect);
            effect.TerrainOffset = terrainOffset;
            effect.TerrainScale = new Vector3
            {
                X = (heightMap.Width - 1) * patchScale,
                Y = heightScale,
                Z = (heightMap.Height - 1) * patchScale
            };
            effect.HeightMap = heightMapTexture;
            effect.PatchGridSize = patchMesh.GridSize;

            boundingBoxDrawer = new BoundingBoxDrawer(GraphicsDevice);
            debugEffect = new BasicEffect(GraphicsDevice);
            debugEffect.AmbientLightColor = Vector3.One;
            debugEffect.VertexColorEnabled = true;
        }

        void InitializeTextures(IHeightMapSource heightMap)
        {
            heightMapTexture = new Texture2D(GraphicsDevice, heightMap.Width, heightMap.Height, false, SurfaceFormat.Single);

            var heights = new float[heightMap.Width * heightMap.Height];
            for (int y = 0; y < heightMap.Height; y++)
            {
                for (int x = 0; x < heightMap.Width; x++)
                {
                    var height = heightMap.GetHeight(x, y);
                    // [-1, 1] -> [0, 1]
                    //height = (height + 1) * 0.5f;
                    heights[x + y * heightMap.Width] = height;
                }
            }
            heightMapTexture.SetData(heights);
        }

        public void Update(GameTime gameTime)
        {
            // 選択状態を初期化。
            selection.SelectedNodeCount = 0;
            Matrix inverse;
            Matrix.Invert(ref view, out inverse);
            selection.EyePosition = inverse.Translation;
            selection.Frustum.Matrix = view * projection;

            quadTree.Select(selection);

            if (selection.SelectedNodeCount == 0) return;

            for (int i = 0; i < selection.SelectedNodeCount; i++)
            {
                instances[i].Offset.X = selection.SelectedNodes[i].X * selection.PatchScale + selection.TerrainOffset.X;
                instances[i].Offset.Y = selection.SelectedNodes[i].Y * selection.PatchScale + selection.TerrainOffset.Z;
                instances[i].Scale = selection.SelectedNodes[i].Size * selection.PatchScale;
                instances[i].Level = selection.SelectedNodes[i].Level;
                selection.Morph.GetMorphConsts(selection.SelectedNodes[i].Level, out instances[i].MorphConsts);
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (selection.SelectedNodeCount == 0) return;

            var offset = instanceVertexBuffer.SetData(instances, 0, selection.SelectedNodeCount);

            vertexBufferBindings[0] = new VertexBufferBinding(patchMesh.VertexBuffer, 0);
            vertexBufferBindings[1] = new VertexBufferBinding(instanceVertexBuffer.VertexBuffer, offset, 1);

            GraphicsDevice.SetVertexBuffers(vertexBufferBindings);
            GraphicsDevice.Indices = patchMesh.IndexBuffer;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Effect
            effect.View = view;
            effect.Projection = projection;
            effect.AmbientLightColor = ambientLightColor;
            effect.LightDirection = lightDirection;
            effect.DiffuseLightColor = diffuseLightColor;
            effect.LightEnabled = LightEnabled;

            // WhiteSolid tequnique
            if (WhiteSolidVisible)
                DrawPatchInstances(effect.WhiteSolidTequnique);

            // HeightColor tequnique
            if (HeightColorVisible)
                DrawPatchInstances(effect.HeightColorTequnique);

            // Wireframe tequnique
            if (WireframeVisible)
            {
                var wireframeTerrainOffset = terrainOffset;
                wireframeTerrainOffset.Y += 0.05f;
                effect.TerrainOffset = wireframeTerrainOffset;
                DrawPatchInstances(effect.WireframeTequnique);
                effect.TerrainOffset = terrainOffset;
            }

            if (NodeBoundingBoxVisible)
            {
                debugEffect.View = view;
                debugEffect.Projection = projection;

                BoundingBox box;
                for (int i = 0; i < selection.SelectedNodeCount; i++)
                {
                    selection.SelectedNodes[i].GetBoundingBox(
                        ref selection.TerrainOffset, selection.PatchScale, selection.HeightScale, out box);
                    var level = selection.SelectedNodes[i].Level;
                    level %= 4;
                    boundingBoxDrawer.Draw(ref box, debugEffect, ref debugLevelColors[level]);
                }
            }
        }

        void DrawPatchInstances(EffectTechnique technique)
        {
            effect.CurrentTechnique = technique;
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawInstancedPrimitives(
                    PrimitiveType.TriangleList, 0, 0,
                    patchMesh.NumVertices, 0, patchMesh.PrimitiveCount, selection.SelectedNodeCount);
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~CDLODTerrain()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (sourceEffect != null) sourceEffect.Dispose();
                if (heightMapTexture != null) heightMapTexture.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
