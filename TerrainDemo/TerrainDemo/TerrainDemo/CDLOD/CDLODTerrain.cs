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
        const int maxPatchCount = 20000;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content { get; private set; }

        public int HeightMapSize { get; private set; }

        // 後で複数マップへの対応をする。
        QuadTree quadTree = new QuadTree();

        PatchInstanceVertex[] instances = new PatchInstanceVertex[maxPatchCount];

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

        public Vector3 TerrainOffset
        {
            get { return selection.TerrainOffset; }
            set { selection.TerrainOffset = value; }
        }

        public float PatchScale
        {
            get { return selection.PatchScale; }
            set { selection.PatchScale = value; }
        }

        public float HeightScale
        {
            get { return selection.HeightScale; }
            set { selection.HeightScale = value; }
        }

        public Morph Morph
        {
            get { return selection.Morph; }
            set { selection.Morph = value; }
        }

        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;

                Matrix inverse;
                Matrix.Invert(ref view, out inverse);
                selection.EyePosition = inverse.Translation;
            }
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

        public CDLODTerrain(GraphicsDevice graphicsDevice, ContentManager content, int heightMapSize)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            HeightMapSize = heightMapSize;

            PatchScale = 1;
            HeightScale = 50;

            lightDirection = new Vector3(0, -1, -1);
            lightDirection.Normalize();

            patchMesh = new PatchMesh(graphicsDevice);
        }

        public void Initialize(IHeightMapSource heightMap)
        {
            if (heightMap.Size != HeightMapSize)
                throw new ArgumentException("The size of heightMap is invalid.", "heightMap");

            quadTree = new QuadTree();
            quadTree.Build(heightMap);

            instanceVertexBuffer = new WritableVertexBuffer<PatchInstanceVertex>(GraphicsDevice, maxPatchCount * 2);

            selection.MaxSelectedNodeCount = maxPatchCount;
            selection.SelectedNodes = new SelectedNode[maxPatchCount];

            if (selection.Morph == null)
                selection.Morph = new DefaultMorph(HeightMapSize);

            if (!selection.Morph.Initialized)
                selection.Morph.Initialize();

            float samplerWorldToTextureScale = (HeightMapSize - 1) / (float) HeightMapSize;
            Vector3 terrainScale = new Vector3
            {
                X = (HeightMapSize - 1) * selection.PatchScale,
                Y = HeightScale,
                Z = (HeightMapSize - 1) * selection.PatchScale
            };

            InitializeTextures(heightMap);

            sourceEffect = Content.Load<Effect>("CDLODTerrainEffect");
            effect = new CDLODTerrainEffect(sourceEffect);
            effect.SamplerWorldToTextureScale = new Vector2(samplerWorldToTextureScale, samplerWorldToTextureScale);
            effect.HeightMapTexelSize = 1 / (float) HeightMapSize;
            effect.TerrainOffset = selection.TerrainOffset;
            effect.TerrainScale = terrainScale;
            effect.HeightMap = heightMapTexture;
        }

        void InitializeTextures(IHeightMapSource heightMap)
        {
            heightMapTexture = new Texture2D(GraphicsDevice, HeightMapSize, HeightMapSize, false, SurfaceFormat.Single);

            var heights = new float[HeightMapSize * HeightMapSize];
            for (int y = 0; y < HeightMapSize; y++)
            {
                for (int x = 0; x < HeightMapSize; x++)
                {
                    var height = heightMap.GetHeight(x, y);
                    // [-1, 1] -> [0, 1]
                    //height = (height + 1) * 0.5f;
                    heights[x + y * HeightMapSize] = height;
                }
            }
            heightMapTexture.SetData(heights);
        }

        public void Update(GameTime gameTime)
        {
            // 選択状態を初期化。
            selection.SelectedNodeCount = 0;

            if (!quadTree.Select(selection))
                return;

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

            effect.Apply();
            GraphicsDevice.DrawInstancedPrimitives(
                PrimitiveType.TriangleList, 0, 0,
                patchMesh.NumVertices, 0, patchMesh.PrimitiveCount, selection.SelectedNodeCount);
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
