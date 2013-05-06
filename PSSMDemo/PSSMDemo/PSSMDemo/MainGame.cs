#region Using

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace PSSMDemo
{
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        #region DrawModelEffect

        /// <summary>
        /// モデル描画シェーダの簡易管理クラスです。
        /// </summary>
        sealed class DrawModelEffect
        {
            Effect sourceEffect;

            Matrix world;

            Matrix view;

            Matrix projection;

            Vector4 ambientColor;

            float depthBias;

            int splitCount;

            Vector3 lightDirection;

            Vector3 shadowColor;

            float[] splitDistances;

            Matrix[] lightViewProjections;

            Texture2D texture;

            ShadowMap[] shadowMaps;

            EffectParameter worldParameter;

            EffectParameter viewParameter;

            EffectParameter projectionParameter;

            EffectParameter ambientColorParameter;

            EffectParameter depthBiasParameter;

            EffectParameter splitCountParameter;

            EffectParameter lightDirectionParameter;

            EffectParameter shadowColorParameter;

            EffectParameter splitDistancesParameter;
            
            EffectParameter lightViewProjectionsParameter;

            EffectParameter textureParameter;

            EffectParameter shadowMap0Parameter;

            EffectParameter shadowMap1Parameter;

            EffectParameter shadowMap2Parameter;

            EffectTechnique basicTechnique;

            EffectTechnique varianceTechnique;

            /// <summary>
            /// ワールド行列。
            /// </summary>
            public Matrix World
            {
                get { return world; }
                set
                {
                    world = value;
                    worldParameter.SetValue(World);
                }
            }

            /// <summary>
            /// ビュー行列。
            /// </summary>
            public Matrix View
            {
                get { return view; }
                set
                {
                    view = value;
                    viewParameter.SetValue(view);
                }
            }

            /// <summary>
            /// 射影行列。
            /// </summary>
            public Matrix Projection
            {
                get { return projection; }
                set
                {
                    projection = value;
                    projectionParameter.SetValue(projection);
                }
            }

            /// <summary>
            /// 環境光の色。
            /// </summary>
            public Vector4 AmbientColor
            {
                get { return ambientColor; }
                set
                {
                    ambientColor = value;
                    ambientColorParameter.SetValue(ambientColor);
                }
            }

            /// <summary>
            /// 深度バイアス。
            /// </summary>
            public float DepthBias
            {
                get { return depthBias; }
                set
                {
                    depthBias = value;
                    depthBiasParameter.SetValue(depthBias);
                }
            }

            /// <summary>
            /// 分割数。
            /// </summary>
            public int SplitCount
            {
                get { return splitCount; }
                set
                {
                    splitCount = value;
                    splitCountParameter.SetValue(splitCount);
                }
            }

            /// <summary>
            /// ライトの進行方向。
            /// </summary>
            public Vector3 LightDirection
            {
                get { return lightDirection; }
                set
                {
                    lightDirection = value;
                    lightDirectionParameter.SetValue(lightDirection);
                }
            }

            /// <summary>
            /// 影の色。
            /// </summary>
            public Vector3 ShadowColor
            {
                get { return shadowColor; }
                set
                {
                    shadowColor = value;
                    shadowColorParameter.SetValue(shadowColor);
                }
            }

            /// <summary>
            /// 分割距離の配列。
            /// </summary>
            public float[] SplitDistances
            {
                get { return splitDistances; }
                set
                {
                    splitDistances = value;
                    splitDistancesParameter.SetValue(splitDistances);
                }
            }

            /// <summary>
            /// ライト空間行列の配列。
            /// </summary>
            public Matrix[] LightViewProjections
            {
                get { return lightViewProjections; }
                set
                {
                    lightViewProjections = value;
                    lightViewProjectionsParameter.SetValue(lightViewProjections);
                }
            }

            /// <summary>
            /// モデルのテクスチャ。
            /// </summary>
            public Texture2D Texture
            {
                get { return texture; }
                set
                {
                    texture = value;
                    textureParameter.SetValue(texture);
                }
            }

            /// <summary>
            /// シャドウ マップの配列。
            /// </summary>
            public ShadowMap[] ShadowMaps
            {
                get { return shadowMaps; }
                set
                {
                    shadowMaps = value;
                    shadowMap0Parameter.SetValue(shadowMaps[0].RenderTarget);
                    shadowMap1Parameter.SetValue(shadowMaps[1].RenderTarget);
                    shadowMap2Parameter.SetValue(shadowMaps[2].RenderTarget);
                }
            }

            /// <summary>
            /// 使用するシャドウ マップの種類。
            /// </summary>
            public ShadowMapForm ShadowMapForm { get; set; }

            /// <summary>
            /// エフェクト (DrawModel.fx) を指定してインスタンスを生成します。
            /// </summary>
            /// <param name="sourceEffect">エフェクト。</param>
            public DrawModelEffect(Effect sourceEffect)
            {
                this.sourceEffect = sourceEffect;

                worldParameter = sourceEffect.Parameters["World"];
                viewParameter = sourceEffect.Parameters["View"];
                projectionParameter = sourceEffect.Parameters["Projection"];
                ambientColorParameter = sourceEffect.Parameters["AmbientColor"];
                depthBiasParameter = sourceEffect.Parameters["DepthBias"];
                splitCountParameter = sourceEffect.Parameters["SplitCount"];
                lightDirectionParameter = sourceEffect.Parameters["LightDirection"];
                shadowColorParameter = sourceEffect.Parameters["ShadowColor"];
                splitDistancesParameter = sourceEffect.Parameters["SplitDistances"];
                lightViewProjectionsParameter = sourceEffect.Parameters["LightViewProjections"];
                textureParameter = sourceEffect.Parameters["Texture"];
                shadowMap0Parameter = sourceEffect.Parameters["ShadowMap0"];
                shadowMap1Parameter = sourceEffect.Parameters["ShadowMap1"];
                shadowMap2Parameter = sourceEffect.Parameters["ShadowMap2"];

                basicTechnique = sourceEffect.Techniques["Basic"];
                varianceTechnique = sourceEffect.Techniques["Variance"];
            }

            /// <summary>
            /// エフェクトの状態をグラフィックス デバイスへ適用します。
            /// </summary>
            public void Apply()
            {
                if (ShadowMapForm == ShadowMapForm.Variance)
                {
                    sourceEffect.CurrentTechnique = varianceTechnique;
                }
                else
                {
                    sourceEffect.CurrentTechnique = basicTechnique;
                }

                sourceEffect.CurrentTechnique.Passes[0].Apply();
            }
        }

        #endregion

        #region LightCameraType

        /// <summary>
        /// ライト カメラの種類を表します。
        /// </summary>
        enum LightCameraType
        {
            /// <summary>
            /// LiSPSMLightCamera を用いる。
            /// </summary>
            LiSPSM = 0,

            /// <summary>
            /// UniformLightCamera を用いる。
            /// </summary>
            Uniform = 1,

            /// <summary>
            /// BasicLightCamera を用いる。
            /// </summary>
            Basic = 2,
        }

        #endregion

        /// <summary>
        /// 最大分割数。
        /// </summary>
        const int MaxSplitCount = 3;

        /// <summary>
        /// 最大分割距離数。
        /// </summary>
        const int MaxSplitDistanceCount = MaxSplitCount + 1;

        /// <summary>
        /// ウィンドウの幅。
        /// </summary>
        const int WindowWidth = 800;

        /// <summary>
        /// ウィンドウの高さ。
        /// </summary>
        const int WindowHeight = 480;

        /// <summary>
        /// シャドウ マップのサイズ。
        /// </summary>
        static readonly int[] ShadowMapSizes = { 512, 1024, 2048 };

        /// <summary>
        /// グラフィックス デバイス マネージャ。
        /// </summary>
        GraphicsDeviceManager graphics;

        /// <summary>
        /// スプライト バッチ。
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// フォント。
        /// </summary>
        SpriteFont spriteFont;

        /// <summary>
        /// 表示カメラ。
        /// </summary>
        BasicCamera camera;

        /// <summary>
        /// 表示カメラの視錐台。
        /// </summary>
        BoundingFrustum cameraFrustum;

        /// <summary>
        /// 前回の更新処理におけるキーボード状態。
        /// </summary>
        KeyboardState lastKeyboardState;

        /// <summary>
        /// 前回の更新処理におけるジョイスティック状態。
        /// </summary>
        GamePadState lastGamePadState;

        /// <summary>
        /// 現在の更新処理におけるキーボード状態。
        /// </summary>
        KeyboardState currentKeyboardState;

        /// <summary>
        /// 現在の更新処理におけるジョイスティック状態。
        /// </summary>
        GamePadState currentGamePadState;

        /// <summary>
        /// モデル描画エフェクト。
        /// </summary>
        DrawModelEffect drawModelEffect;

        /// <summary>
        /// グリッド モデル (格子状の床)。
        /// </summary>
        Model gridModel;

        /// <summary>
        /// デュード モデル (人)。
        /// </summary>
        Model dudeModel;

        /// <summary>
        /// デュード モデルのローカル空間境界ボックス。
        /// </summary>
        BoundingBox dudeBoxLocal;

        /// <summary>
        /// デュード モデルのワールド空間境界ボックス。
        /// </summary>
        BoundingBox dudeBoxWorld;

        /// <summary>
        /// デュード モデルに適用するワールド行列。
        /// </summary>
        Matrix world;

        /// <summary>
        /// 明示するシーン領域。
        /// </summary>
        BoundingBox sceneBox;

        /// <summary>
        /// 表示カメラの視錐台をシーン領域として用いるか否かを示す値。
        /// true (表示カメラの視錐台をシーン領域として用いる場合)、
        /// false (sceneBox で明示した領域をシーン領域として用いる場合)。
        /// </summary>
        bool useCameraFrustumSceneBox;

        /// <summary>
        /// 視錐台や境界ボックスの頂点を得るための一時作業配列。
        /// </summary>
        Vector3[] corners;

        /// <summary>
        /// デュード モデルの回転量。
        /// </summary>
        float rotateDude;

        /// <summary>
        /// 現在選択されているライト カメラの種類。
        /// </summary>
        LightCameraType currentLightCameraType;

        /// <summary>
        /// 簡易ライト カメラ ビルダ。
        /// </summary>
        BasicLightCameraBuilder basicLightCameraBuilder;

        /// <summary>
        /// USM ライト カメラ ビルダ。
        /// </summary>
        UniformLightCameraBuilder uniformLightCameraBuilder;

        /// <summary>
        /// LiSPSM ライト カメラ ビルダ。
        /// </summary>
        LiSPSMLightCameraBuilder liSPSMLightCameraBuilder;

        /// <summary>
        /// 分割数。
        /// </summary>
        int splitCount;

        /// <summary>
        /// PSSM 分割機能。
        /// </summary>
        PSSM pssm;

        /// <summary>
        /// 分割された距離の配列。
        /// </summary>
        float[] splitDistances;

        /// <summary>
        /// 分割された射影行列の配列。
        /// </summary>
        Matrix[] splitProjections;

        /// <summary>
        /// 分割されたシャドウ マップの配列。
        /// </summary>
        ShadowMap[] shadowMaps;

        /// <summary>
        /// 分割されたライト カメラ空間行列の配列。
        /// </summary>
        Matrix[] lightViewProjections;

        /// <summary>
        /// シャドウ マップ形式。
        /// </summary>
        ShadowMapForm shadowMapForm;

        /// <summary>
        /// シャドウ マップ エフェクト。
        /// </summary>
        ShadowMapEffect shadowMapEffect;

        /// <summary>
        /// ガウシアン ブラー エフェクト。
        /// </summary>
        Effect gaussianBlurEffect;

        /// <summary>
        /// ガウシアン ブラー。
        /// </summary>
        GaussianBlur gaussianBlur;

        /// <summary>
        /// ライトの進行方向。
        /// </summary>
        /// <remarks>
        /// XNA Shadow Mapping では原点から見たライトの方向であり、
        /// ここでの方向の定義が異なる点に注意が必要です。
        /// </remarks>
        Vector3 lightDirection = new Vector3(0.3333333f, -0.6666667f, -0.6666667f);

        /// <summary>
        /// ライトによる投影を処理する距離。
        /// </summary>
        float lightFar = 500.0f;

        /// <summary>
        /// 現在の表示カメラの境界錐台。
        /// </summary>
        BoundingFrustum currentFrustum;

        /// <summary>
        /// 現在のシャドウ マップ サイズのインデックス。
        /// </summary>
        int currentShadowMapSizeIndex = 1;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;

            camera = new BasicCamera
            {
                Position = new Vector3(0, 70, 100),
                Direction = new Vector3(0, -0.4472136f, -0.8944272f),
                Fov = MathHelper.PiOver4,
                AspectRatio = (float) WindowWidth / (float) WindowHeight,
                NearClipDistance = 1.0f,
                FarClipDistance = 1000.0f
            };

            cameraFrustum = new BoundingFrustum(Matrix.Identity);

            corners = new Vector3[8];

            // gridModel が半径約 183 であるため、
            // これを含むように簡易シーン AABB を決定。
            // なお、広大な世界を扱う場合には、表示カメラの視錐台に含まれるオブジェクト、
            // および、それらに投影しうるオブジェクトを動的に選択および決定し、
            // 適切な最小シーン領域を算出して利用する。
            sceneBox = new BoundingBox(new Vector3(-200), new Vector3(200));

            useCameraFrustumSceneBox = true;

            currentLightCameraType = LightCameraType.LiSPSM;

            basicLightCameraBuilder = new BasicLightCameraBuilder();
            uniformLightCameraBuilder = new UniformLightCameraBuilder();
            uniformLightCameraBuilder.LightFarClipDistance = lightFar;
            liSPSMLightCameraBuilder = new LiSPSMLightCameraBuilder();
            liSPSMLightCameraBuilder.LightFarClipDistance = lightFar;

            splitCount = MaxSplitCount;

            pssm = new PSSM();
            pssm.Fov = camera.Fov;
            pssm.AspectRatio = camera.AspectRatio;
            pssm.NearClipDistance = camera.NearClipDistance;
            pssm.FarClipDistance = camera.FarClipDistance;

            splitDistances = new float[MaxSplitCount + 1];
            splitProjections = new Matrix[MaxSplitCount];
            shadowMaps = new ShadowMap[MaxSplitCount];
            lightViewProjections = new Matrix[MaxSplitCount];

            shadowMapForm = ShadowMapForm.Basic;

            // 単位ベクトル。
            lightDirection = new Vector3(0.3333333f, -0.6666667f, -0.6666667f);

            currentFrustum = new BoundingFrustum(Matrix.Identity);
        }

        protected override void LoadContent()
        {
            shadowMapEffect = new ShadowMapEffect(Content.Load<Effect>("ShadowMap"));
            gaussianBlurEffect = Content.Load<Effect>("GaussianBlur");

            drawModelEffect = new DrawModelEffect(Content.Load<Effect>("DrawModel"));
            drawModelEffect.AmbientColor = new Vector4(0.15f, 0.15f, 0.15f, 1.0f);
            drawModelEffect.DepthBias = 0.001f;
            drawModelEffect.LightDirection = lightDirection;
            drawModelEffect.ShadowColor = new Vector3(0.5f, 0.5f, 0.5f);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");

            gridModel = Content.Load<Model>("grid");
            dudeModel = Content.Load<Model>("dude");

            dudeBoxLocal = BoundingBoxHelper.Empty;
            foreach (var mesh in dudeModel.Meshes)
            {
                BoundingBoxHelper.Merge(ref dudeBoxLocal, BoundingBox.CreateFromSphere(mesh.BoundingSphere));
            }

            for (int i = 0; i < shadowMaps.Length; i++)
            {
                shadowMaps[i] = new ShadowMap(GraphicsDevice, shadowMapEffect);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // キーボード状態およびジョイスティック状態のハンドリング。
            HandleInput(gameTime);

            // 表示カメラの更新。
            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // 念のため状態を初期状態へ。
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // シャドウ マップの描画。
            CreateShadowMap();

            // シャドウ マップを用いたシーンの描画。
            DrawWithShadowMap();

            // シャドウ マップを画面左上に表示。
            DrawShadowMapToScreen();

            // HUD のテキストを描画。
            DrawOverlayText();

            base.Draw(gameTime);
        }

        void CreateShadowMap()
        {
            // デュード モデルのワールド行列。
            world = Matrix.CreateRotationY(MathHelper.ToRadians(rotateDude));

            dudeBoxLocal.GetCorners(corners);
            dudeBoxWorld = BoundingBoxHelper.Empty;
            foreach (var corner in corners)
            {
                Vector3 cornerLocal = corner;
                Vector3 cornerWorld;
                Vector3.Transform(ref cornerLocal, ref world, out cornerWorld);

                BoundingBoxHelper.Merge(ref dudeBoxWorld, ref cornerWorld);
            }

            // ライト カメラへ指定するシーン領域。
            BoundingBox actualSceneBox;
            if (useCameraFrustumSceneBox)
            {
                // 視錐台全体とする場合。
                cameraFrustum.Matrix = camera.View * camera.Projection;
                cameraFrustum.GetCorners(corners);
                actualSceneBox = BoundingBox.CreateFromPoints(corners);

            }
            else
            {
                // 明示する場合。
                actualSceneBox = sceneBox;
                BoundingBoxHelper.Merge(ref actualSceneBox, camera.Position);
            }

            // 表示カメラの分割。
            // デフォルトのラムダ値 0.5f ではカメラ手前が少し狭すぎるか？
            // ここは表示カメラの far の値に応じて調整する。
            pssm.Count = splitCount;
            pssm.Lambda = 0.4f;
            pssm.View = camera.View;
            pssm.SceneBox = actualSceneBox;
            pssm.Split(splitDistances, splitProjections);

            // 使用するライト カメラ ビルダの選択。
            LightCameraBuilder currentLightCameraBuilder;
            switch (currentLightCameraType)
            {
                case LightCameraType.LiSPSM:
                    currentLightCameraBuilder = liSPSMLightCameraBuilder;
                    break;
                case LightCameraType.Uniform:
                    currentLightCameraBuilder = uniformLightCameraBuilder;
                    break;
                default:
                    currentLightCameraBuilder = basicLightCameraBuilder;
                    break;
            }

            // 各分割で共通のビルダ プロパティを設定。
            currentLightCameraBuilder.EyeView = camera.View;
            currentLightCameraBuilder.LightDirection = lightDirection;
            currentLightCameraBuilder.SceneBox = sceneBox;

            for (int i = 0; i < splitCount; i++)
            {
                // 射影行列は分割毎に異なる。
                currentLightCameraBuilder.EyeProjection = splitProjections[i];

                // ライトのビューおよび射影行列の算出。
                Matrix lightView;
                Matrix lightProjection;
                currentLightCameraBuilder.Build(out lightView, out lightProjection);

                // 後のモデル描画用にライト空間行列を算出。
                Matrix.Multiply(ref lightView, ref lightProjection, out lightViewProjections[i]);

                // シャドウ マップを描画。
                shadowMaps[i].Form = shadowMapForm;
                shadowMaps[i].Size = ShadowMapSizes[currentShadowMapSizeIndex];
                shadowMaps[i].Draw(camera.View, splitProjections[i], lightView, lightProjection, DrawShadowCasters);

                // VSM の場合は生成したシャドウ マップへブラーを適用。
                if (shadowMapForm == ShadowMapForm.Variance)
                {
                    if (gaussianBlur == null)
                    {
                        var shadowMapSize = ShadowMapSizes[currentShadowMapSizeIndex];
                        gaussianBlur = new GaussianBlur(
                            GraphicsDevice, shadowMapSize, shadowMapSize, SurfaceFormat.Vector2, gaussianBlurEffect);
                        gaussianBlur.Radius = 7;
                        gaussianBlur.Amount = 7;
                    }

                    gaussianBlur.Filter(shadowMaps[i].RenderTarget, shadowMaps[i].RenderTarget);
                }
            }
        }

        void DrawWithShadowMap()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // シャドウ マップに対するサンプラ。
            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            // シャドウ マップと共にグリッド モデルを描画。
            world = Matrix.Identity;
            DrawModelWithShadowMap(gridModel);

            // シャドウ マップと共にデュード モデルを描画。
            world = Matrix.CreateRotationY(MathHelper.ToRadians(rotateDude));
            DrawModelWithShadowMap(dudeModel);
        }

        // コールバック。
        void DrawShadowCasters(Matrix eyeView, Matrix eyeProjection, ShadowMapEffect effect)
        {
            Matrix viewProjection;
            Matrix.Multiply(ref eyeView, ref eyeProjection, out viewProjection);

            currentFrustum.Matrix = viewProjection;

            ContainmentType containment;
            currentFrustum.Contains(ref dudeBoxWorld, out containment);
            if (containment != ContainmentType.Disjoint)
            {
                DrawShadowCaster(effect, dudeModel);
            }
        }

        void DrawShadowCaster(ShadowMapEffect effect, Model model)
        {
            // シャドウ マップ エフェクトの準備。
            effect.World = world;
            effect.Apply();

            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    GraphicsDevice.Indices = meshPart.IndexBuffer;
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        meshPart.VertexOffset, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }

        void DrawModelWithShadowMap(Model model)
        {
            drawModelEffect.World = world;
            drawModelEffect.View = camera.View;
            drawModelEffect.Projection = camera.Projection;
            drawModelEffect.SplitCount = splitCount;
            drawModelEffect.SplitDistances = splitDistances;
            drawModelEffect.ShadowMaps = shadowMaps;
            drawModelEffect.ShadowMapForm = shadowMapForm;
            drawModelEffect.LightViewProjections = lightViewProjections;

            // XNA 制限:
            // Surface.Single や Surface.Vector2 を用いる場合、
            // サンプラのテクスチャ フィルタは Point でなければならない。
            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
            GraphicsDevice.SamplerStates[3] = SamplerState.PointClamp;
            
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    GraphicsDevice.Indices = meshPart.IndexBuffer;

                    // BasicEffect に設定されているモデルのテクスチャを取得して設定。
                    drawModelEffect.Texture = (meshPart.Effect as BasicEffect).Texture;

                    // 状態の変更を適用。
                    drawModelEffect.Apply();

                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        meshPart.VertexOffset, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }

        void DrawShadowMapToScreen()
        {
            const int mapSize = 96;

            // 現在のフレームで生成したシャドウ マップを画面左上に表示。
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
            for (int i = 0; i < splitCount; i++)
            {
                var x = i * mapSize;
                spriteBatch.Draw(shadowMaps[i].RenderTarget, new Rectangle(x, 0, mapSize, mapSize), Color.White);
            }
            spriteBatch.End();
        }

        void DrawOverlayText()
        {
            var currentShadowMapSize = ShadowMapSizes[currentShadowMapSizeIndex];

            // HUD のテキストを表示。
            var text = "B = Light camera type (" + currentLightCameraType + ")\n" +
                "X = Shadow map form (" + shadowMapForm + ")\n" +
                "Y = Camera frustum as scene box (" + useCameraFrustumSceneBox + ")\n" +
                "K = Split count (" + splitCount + ")\n" +
                "L = Shadow map size (" + currentShadowMapSize + "x" + currentShadowMapSize + ")";

            spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont, text, new Vector2(65, 350), Color.Black);
            spriteBatch.DrawString(spriteFont, text, new Vector2(64, 350 - 1), Color.Yellow);

            spriteBatch.End();
        }

        void HandleInput(GameTime gameTime)
        {
            float time = (float) gameTime.ElapsedGameTime.TotalMilliseconds;

            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            rotateDude += currentGamePadState.Triggers.Right * time * 0.2f;
            rotateDude -= currentGamePadState.Triggers.Left * time * 0.2f;

            if (currentKeyboardState.IsKeyDown(Keys.Q))
                rotateDude -= time * 0.2f;
            if (currentKeyboardState.IsKeyDown(Keys.E))
                rotateDude += time * 0.2f;

            if (currentKeyboardState.IsKeyUp(Keys.B) && lastKeyboardState.IsKeyDown(Keys.B) ||
                currentGamePadState.IsButtonUp(Buttons.B) && lastGamePadState.IsButtonDown(Buttons.B))
            {
                currentLightCameraType++;

                if (LightCameraType.Basic < currentLightCameraType)
                    currentLightCameraType = LightCameraType.LiSPSM;
            }

            if (currentKeyboardState.IsKeyUp(Keys.Y) && lastKeyboardState.IsKeyDown(Keys.Y) ||
                currentGamePadState.IsButtonUp(Buttons.Y) && lastGamePadState.IsButtonDown(Buttons.Y))
            {
                useCameraFrustumSceneBox = !useCameraFrustumSceneBox;
            }

            if (currentKeyboardState.IsKeyUp(Keys.X) && lastKeyboardState.IsKeyDown(Keys.X) ||
                currentGamePadState.IsButtonUp(Buttons.X) && lastGamePadState.IsButtonDown(Buttons.X))
            {
                if (shadowMapForm == ShadowMapForm.Basic)
                {
                    shadowMapForm = ShadowMapForm.Variance;
                }
                else
                {
                    shadowMapForm = ShadowMapForm.Basic;
                }
            }

            if (currentKeyboardState.IsKeyUp(Keys.K) && lastKeyboardState.IsKeyDown(Keys.K) ||
                currentGamePadState.IsButtonUp(Buttons.RightShoulder) && lastGamePadState.IsButtonDown(Buttons.RightShoulder))
            {
                splitCount++;
                if (MaxSplitCount < splitCount)
                    splitCount = 1;
            }

            if (currentKeyboardState.IsKeyUp(Keys.L) && lastKeyboardState.IsKeyDown(Keys.L) ||
                currentGamePadState.IsButtonUp(Buttons.LeftShoulder) && lastGamePadState.IsButtonDown(Buttons.LeftShoulder))
            {
                currentShadowMapSizeIndex++;
                if (ShadowMapSizes.Length <= currentShadowMapSizeIndex)
                    currentShadowMapSizeIndex = 0;

                if (gaussianBlur != null)
                {
                    gaussianBlur.Dispose();
                    gaussianBlur = null;
                }
            }

            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
        }

        void UpdateCamera(GameTime gameTime)
        {
            float time = (float) gameTime.ElapsedGameTime.TotalMilliseconds;

            float pitch = -currentGamePadState.ThumbSticks.Right.Y * time * 0.001f;
            float yaw = -currentGamePadState.ThumbSticks.Right.X * time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Up))
                pitch += time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Down))
                pitch -= time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Left))
                yaw += time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Right))
                yaw -= time * 0.001f;

            camera.Pitch(pitch);
            camera.Yaw(yaw);

            var movement = new Vector3();

            if (currentKeyboardState.IsKeyDown(Keys.W))
                movement.Z -= time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.S))
                movement.Z += time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.A))
                movement.X -= time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.D))
                movement.X += time * 0.1f;

            movement.Z -= currentGamePadState.ThumbSticks.Left.Y * time * 0.1f;
            movement.X += currentGamePadState.ThumbSticks.Left.X * time * 0.1f;

            camera.MoveRelative(ref movement);

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                camera.Position = new Vector3(0, 50, 50);
                camera.Direction = Vector3.Forward;
            }

            camera.Update();
        }
    }

    #region Program

    static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new MainGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
