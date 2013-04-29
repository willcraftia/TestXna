#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace LiSPSMDemo
{
    /// <summary>
    /// ゲーム クラス。
    /// 原型は XNA Shadow Mapping サンプル。
    /// ここでは、原型に対して VSM、LiSPSM のロジックをサンプルとして追加している。
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        #region DrawModelEffect

        /// <summary>
        /// モデル描画エフェクト管理クラス。
        /// </summary>
        sealed class DrawModelEffect
        {
            #region DirtyFlags

            /// <summary>
            /// プロパティのダーティ フラグ。
            /// ダーティ フラグでプロパティの変更を管理し、
            /// 変更のあったプロパティのみをエフェクトへ再適用します。
            /// </summary>
            [Flags]
            enum DirtyFlags
            {
                World               = (1 << 0),
                View                = (1 << 1),
                Projection          = (1 << 2),
                LightViewProjection = (1 << 3),
                LightDirection      = (1 << 4),
                DepthBias           = (1 << 5),
                AmbientColor        = (1 << 6),
                Texture             = (1 << 7),
                ShadowMap           = (1 << 8),
            }

            #endregion

            Effect sourceEffect;

            Matrix world;

            Matrix view;

            Matrix projection;

            Matrix lightViewProjection;

            Vector3 lightDirection;

            float depthBias;

            Vector4 ambientColor;

            Texture2D texture;

            Texture2D shadowMap;

            DirtyFlags dirtyFlags;

            EffectParameter worldParameter;

            EffectParameter viewParameter;

            EffectParameter projectionParameter;

            EffectParameter lightViewProjectionParameter;

            EffectParameter lightDirectionParameter;

            EffectParameter depthBiasParameter;

            EffectParameter ambientColorParameter;

            EffectParameter textureParameter;

            EffectParameter shadowMapParameter;

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

                    dirtyFlags |= DirtyFlags.World;
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

                    dirtyFlags |= DirtyFlags.View;
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

                    dirtyFlags |= DirtyFlags.Projection;
                }
            }

            /// <summary>
            /// ライト空間行列 (ライト カメラのビュー×射影行列)。
            /// </summary>
            public Matrix LightViewProjection
            {
                get { return lightViewProjection; }
                set
                {
                    lightViewProjection = value;

                    dirtyFlags |= DirtyFlags.LightViewProjection;
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

                    dirtyFlags |= DirtyFlags.LightDirection;
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

                    dirtyFlags |= DirtyFlags.DepthBias;
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

                    dirtyFlags |= DirtyFlags.AmbientColor;
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

                    dirtyFlags |= DirtyFlags.Texture;
                }
            }

            /// <summary>
            /// シャドウ マップ。
            /// </summary>
            public Texture2D ShadowMap
            {
                get { return shadowMap; }
                set
                {
                    shadowMap = value;

                    dirtyFlags |= DirtyFlags.ShadowMap;
                }
            }

            /// <summary>
            /// 使用するシャドウ マップの種類。
            /// </summary>
            public ShadowMapEffectForm ShadowMapEffectForm { get; set; }

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
                lightViewProjectionParameter = sourceEffect.Parameters["LightViewProjection"];
                lightDirectionParameter = sourceEffect.Parameters["LightDirection"];
                depthBiasParameter = sourceEffect.Parameters["DepthBias"];
                ambientColorParameter = sourceEffect.Parameters["AmbientColor"];
                textureParameter = sourceEffect.Parameters["Texture"];
                shadowMapParameter = sourceEffect.Parameters["ShadowMap"];

                basicTechnique = sourceEffect.Techniques["Basic"];
                varianceTechnique = sourceEffect.Techniques["Variance"];

                dirtyFlags = DirtyFlags.World | DirtyFlags.View | DirtyFlags.Projection |
                    DirtyFlags.LightViewProjection | DirtyFlags.LightDirection |
                    DirtyFlags.DepthBias | DirtyFlags.AmbientColor;
            }

            /// <summary>
            /// エフェクトの状態をグラフィックス デバイスへ適用します。
            /// </summary>
            public void Apply()
            {
                if ((dirtyFlags & DirtyFlags.World) != 0)
                {
                    worldParameter.SetValue(World);
                    dirtyFlags &= ~DirtyFlags.World;
                }
                if ((dirtyFlags & DirtyFlags.View) != 0)
                {
                    viewParameter.SetValue(View);
                    dirtyFlags &= ~DirtyFlags.View;
                }
                if ((dirtyFlags & DirtyFlags.Projection) != 0)
                {
                    projectionParameter.SetValue(Projection);
                    dirtyFlags &= ~DirtyFlags.Projection;
                }
                if ((dirtyFlags & DirtyFlags.LightViewProjection) != 0)
                {
                    lightViewProjectionParameter.SetValue(LightViewProjection);
                    dirtyFlags &= ~DirtyFlags.LightViewProjection;
                }
                if ((dirtyFlags & DirtyFlags.LightDirection) != 0)
                {
                    lightDirectionParameter.SetValue(LightDirection);
                    dirtyFlags &= ~DirtyFlags.LightDirection;
                }
                if ((dirtyFlags & DirtyFlags.DepthBias) != 0)
                {
                    depthBiasParameter.SetValue(DepthBias);
                    dirtyFlags &= ~DirtyFlags.DepthBias;
                }
                if ((dirtyFlags & DirtyFlags.AmbientColor) != 0)
                {
                    ambientColorParameter.SetValue(AmbientColor);
                    dirtyFlags &= ~DirtyFlags.AmbientColor;
                }
                if ((dirtyFlags & DirtyFlags.Texture) != 0)
                {
                    textureParameter.SetValue(Texture);
                    dirtyFlags &= ~DirtyFlags.Texture;
                }
                if ((dirtyFlags & DirtyFlags.ShadowMap) != 0)
                {
                    shadowMapParameter.SetValue(ShadowMap);
                    dirtyFlags &= ~DirtyFlags.ShadowMap;
                }

                if (ShadowMapEffectForm == ShadowMapEffectForm.Variance)
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
        /// ライト カメラの種類。
        /// </summary>
        enum LightCameraType
        {
            /// <summary>
            /// LiSPSMLightCamera を用いる。
            /// </summary>
            LiSPSM = 0,

            /// <summary>
            /// FocusedLightCamera を用いる。
            /// </summary>
            Focused = 1,

            /// <summary>
            /// BasicLightCamera を用いる。
            /// </summary>
            Basic = 2,
        }

        #endregion

        /// <summary>
        /// シャドウ マップのサイズ (正方形)。
        /// </summary>
        const int shadowMapSize = 2048;

        /// <summary>
        /// ウィンドウの幅。
        /// </summary>
        const int windowWidth = 800;

        /// <summary>
        /// ウィンドウの高さ。
        /// </summary>
        const int windowHeight = 480;

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
        /// 表示カメラの位置。
        /// </summary>
        Vector3 cameraPosition = new Vector3(0, 70, 100);

        /// <summary>
        /// 表示カメラの視線方向。
        /// </summary>
        Vector3 cameraForward = new Vector3(0, -0.4472136f, -0.8944272f);

        /// <summary>
        /// 表示カメラの視錐台。
        /// </summary>
        BoundingFrustum cameraFrustum = new BoundingFrustum(Matrix.Identity);

        /// <summary>
        /// 表示カメラの近平面までの距離。
        /// </summary>
        float cameraNear = 1.0f;

        /// <summary>
        /// 表示カメラの遠平面までの距離。
        /// </summary>
        float cameraFar = 1000.0f;

        /// <summary>
        /// ライトの進行方向 (XNA Shadow Mapping では原点から見たライトの方向)。
        /// 単位ベクトル。
        /// </summary>
        Vector3 lightDirection = new Vector3(0.3333333f, -0.6666667f, -0.6666667f);

        /// <summary>
        /// ライトによる投影を処理する距離。
        /// </summary>
        float lightFar = 500.0f;

        /// <summary>
        /// 前回の更新処理におけるキーボード状態。
        /// </summary>
        KeyboardState lastKeyboardState = new KeyboardState();

        /// <summary>
        /// 前回の更新処理におけるゲーム パッド状態。
        /// </summary>
        GamePadState lastGamePadState = new GamePadState();

        /// <summary>
        /// 現在の更新処理におけるキーボード状態。
        /// </summary>
        KeyboardState currentKeyboardState;

        /// <summary>
        /// 現在の更新処理におけるゲーム パッド状態。
        /// </summary>
        GamePadState currentGamePadState;

        /// <summary>
        /// シャドウ マップ エフェクト。
        /// </summary>
        ShadowMapEffect shadowMapEffect;

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
        float rotateDude = 0.0f;

        /// <summary>
        /// 基礎的なシャドウ マップのレンダ ターゲット。
        /// </summary>
        RenderTarget2D bsmRenderTarget;

        /// <summary>
        /// VSM のレンダ ターゲット。
        /// </summary>
        RenderTarget2D vsmRenderTarget;

        /// <summary>
        /// 現在選択されているシャドウ マップのレンダ ターゲット。
        /// </summary>
        RenderTarget2D currentShadowRenderTarget;

        /// <summary>
        /// デュード モデルに適用するワールド行列。
        /// </summary>
        Matrix world;

        /// <summary>
        /// 表示カメラのビュー行列。
        /// </summary>
        Matrix view;

        /// <summary>
        /// 表示カメラの射影行列。
        /// </summary>
        Matrix projection;

        /// <summary>
        /// 基礎的な簡易ライト カメラ。
        /// シーン領域へ焦点を合わせない。
        /// XNA Shadow Mapping のライト カメラ算出と同程度の品質。
        /// </summary>
        BasicLightCamera basicLightCamera;

        /// <summary>
        /// シーン領域へ焦点を合わせるライト カメラ。
        /// 焦点合わせにより、BasicLightCamera よりも高品質となる。
        /// </summary>
        FocusedLightCamera focusedLightCamera;

        /// <summary>
        /// LiSPSM ライト カメラ。
        /// 焦点合わせおよび LiSPSM による補正により、
        /// FocusedLightCamera よりも高品質となる。
        /// </summary>
        LiSPSMLightCamera lispsmLightCamera;

        /// <summary>
        /// 現在選択されているライト カメラの種類。
        /// </summary>
        LightCameraType currentLightCameraType;

        /// <summary>
        /// 現在のライト空間行列 (ビュー×射影行列)。
        /// </summary>
        Matrix lightViewProjection;

        /// <summary>
        /// VSM で用いるガウシアン ブラー。
        /// </summary>
        GaussianBlur gaussianBlur;

        /// <summary>
        /// 現在選択されているシャドウ マップの種類。
        /// </summary>
        ShadowMapEffectForm shadowMapEffectForm;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;

            var aspectRatio = (float) windowWidth / (float) windowHeight;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, cameraNear, cameraFar);

            corners = new Vector3[8];

            // gridModel が半径約 183 であるため、
            // これを含むように簡易シーン AABB を決定。
            // なお、広大な世界を扱う場合には、表示カメラの視錐台に含まれるオブジェクト、
            // および、それらに投影しうるオブジェクトを動的に選択および決定し、
            // 適切な最小シーン領域を算出して利用する。
            sceneBox = new BoundingBox(new Vector3(-200), new Vector3(200));

            useCameraFrustumSceneBox = true;

            // ライト カメラの初期化。

            basicLightCamera = new BasicLightCamera();
            basicLightCamera.LightDirection = lightDirection;

            focusedLightCamera = new FocusedLightCamera();
            focusedLightCamera.EyeNearDistance = cameraNear;
            focusedLightCamera.EyeFarDistance = cameraFar;
            focusedLightCamera.LightDirection = lightDirection;
            focusedLightCamera.LightFarDistance = lightFar;

            lispsmLightCamera = new LiSPSMLightCamera();
            lispsmLightCamera.EyeNearDistance = cameraNear;
            lispsmLightCamera.EyeFarDistance = cameraFar;
            lispsmLightCamera.LightDirection = lightDirection;
            lispsmLightCamera.LightFarDistance = lightFar;

            currentLightCameraType = LightCameraType.LiSPSM;

            shadowMapEffectForm = ShadowMapEffectForm.Variance;
        }

        protected override void LoadContent()
        {
            shadowMapEffect = new ShadowMapEffect(Content.Load<Effect>("ShadowMap"));

            drawModelEffect = new DrawModelEffect(Content.Load<Effect>("DrawModel"));
            drawModelEffect.LightDirection = lightDirection;
            drawModelEffect.DepthBias = 0.001f;
            drawModelEffect.AmbientColor = new Vector4(0.15f, 0.15f, 0.15f, 1.0f);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");

            gridModel = Content.Load<Model>("grid");
            dudeModel = Content.Load<Model>("dude");

            // 基礎的なシャドウ マップは R 値のみを用いる。
            bsmRenderTarget = new RenderTarget2D(
                GraphicsDevice, shadowMapSize, shadowMapSize,
                false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

            // VSM は RG 値の二つを用いる。
            vsmRenderTarget = new RenderTarget2D(
                GraphicsDevice, shadowMapSize, shadowMapSize,
                false, SurfaceFormat.Vector2, DepthFormat.Depth24Stencil8);

            // ガウシアン ブラーは VSM で用いるため、
            // 内部で使用するレンダ ターゲットは VSM に合わせる。
            gaussianBlur = new GaussianBlur(GraphicsDevice,
                vsmRenderTarget.Width, vsmRenderTarget.Height,
                SurfaceFormat.Vector2, Content.Load<Effect>("GaussianBlur"));
            gaussianBlur.Radius = 4;
            gaussianBlur.Amount = 16;
        }

        protected override void Update(GameTime gameTime)
        {
            // キーボード状態およびゲーム パッド状態のハンドリング。
            HandleInput(gameTime);

            // 表示カメラの更新。
            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // ライト カメラによるライト空間行列の算出。
            lightViewProjection = CreateLightViewProjectionMatrix();

            // 念のため状態を初期状態へ。
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // 選択されているシャドウ マップの種類に応じてレンダ ターゲットを切り替え。
            switch (shadowMapEffectForm)
            {
                case ShadowMapEffectForm.Basic:
                    currentShadowRenderTarget = bsmRenderTarget;
                    break;
                case ShadowMapEffectForm.Variance:
                    currentShadowRenderTarget = vsmRenderTarget;
                    break;
            }

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

        Matrix CreateLightViewProjectionMatrix()
        {
            // ライト カメラへ指定するシーン領域。
            BoundingBox actualSceneBox;

            if (useCameraFrustumSceneBox)
            {
                // 視錐台全体とする場合。
                cameraFrustum.GetCorners(corners);
                actualSceneBox = BoundingBox.CreateFromPoints(corners);

            }
            else
            {
                // 明示する場合。
                actualSceneBox = sceneBox;
                Vector3.Min(ref actualSceneBox.Min, ref cameraPosition, out actualSceneBox.Min);
                Vector3.Max(ref actualSceneBox.Max, ref cameraPosition, out actualSceneBox.Max);
            }

            // 利用するライト カメラの選択。
            LightCamera lightCamera;
            switch (currentLightCameraType)
            {
                case LightCameraType.LiSPSM:
                    lightCamera = lispsmLightCamera;
                    break;
                case LightCameraType.Focused:
                    lightCamera = focusedLightCamera;
                    break;
                default:
                    lightCamera = basicLightCamera;
                    break;
            }

            // カメラの行列を更新。
            lightCamera.Update(view, projection, actualSceneBox);

            // ライト空間行列の算出。
            Matrix lightViewProjection;
            Matrix.Multiply(ref lightCamera.LightView, ref lightCamera.LightProjection, out lightViewProjection);

            return lightViewProjection;
        }

        void CreateShadowMap()
        {
            // 選択中のシャドウ マップ レンダ ターゲットを設定。
            GraphicsDevice.SetRenderTarget(currentShadowRenderTarget);

            // レンダ ターゲットの R あるいは RG を 1 で埋める (1 は最遠の深度)。
            // 同時に、深度ステンシルの深度も 1 へ。
            GraphicsDevice.Clear(Color.White);

            // デュード モデルのワールド行列。
            world = Matrix.CreateRotationY(MathHelper.ToRadians(rotateDude));

            // 投影オブジェクトとしてデュード モデルを描画。
            // グリッド モデルは非投影オブジェクト。
            DrawModel(dudeModel, true);

            // レンダ ターゲットをデフォルトへ戻す。
            GraphicsDevice.SetRenderTarget(null);

            // VSM を選択している場合はシャドウ マップへブラーを適用。
            if (shadowMapEffectForm == ShadowMapEffectForm.Variance)
            {
                gaussianBlur.Filter(currentShadowRenderTarget, currentShadowRenderTarget);
            }
        }

        void DrawWithShadowMap()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // シャドウ マップに対するサンプラ。
            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            // シャドウ マップと共にグリッド モデルを描画。
            world = Matrix.Identity;
            DrawModel(gridModel, false);

            // シャドウ マップと共にデュード モデルを描画。
            world = Matrix.CreateRotationY(MathHelper.ToRadians(rotateDude));
            DrawModel(dudeModel, false);
        }

        void DrawModel(Model model, bool createShadowMap)
        {
            if (createShadowMap)
            {
                // シャドウ マップ エフェクトの準備。
                shadowMapEffect.World = world;
                shadowMapEffect.LightViewProjection = lightViewProjection;
                shadowMapEffect.Form = shadowMapEffectForm;

                // 全てのモデルのメッシュ パートに対して同一の状態を用いるため、
                // ここで状態を適用。
                shadowMapEffect.Apply();
            }
            else
            {
                // モデル描画エフェクトの準備。
                drawModelEffect.World = world;
                drawModelEffect.View = view;
                drawModelEffect.Projection = projection;
                drawModelEffect.LightViewProjection = lightViewProjection;
                drawModelEffect.ShadowMap = currentShadowRenderTarget;
                drawModelEffect.ShadowMapEffectForm = shadowMapEffectForm;
            }

            // モデルを描画。
            // モデルは XNB 標準状態で読み込んでいるため、
            // メッシュ パートのエフェクトには BasicEffect が設定されている。

            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    GraphicsDevice.Indices = meshPart.IndexBuffer;

                    if (!createShadowMap)
                    {
                        // BasicEffect に設定されているモデルのテクスチャを取得して設定。
                        drawModelEffect.Texture = (meshPart.Effect as BasicEffect).Texture;

                        // 状態の変更を適用。
                        drawModelEffect.Apply();
                    }

                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        meshPart.VertexOffset, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }

        void DrawShadowMapToScreen()
        {
            // 現在のフレームで生成したシャドウ マップを画面左上に表示。
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(currentShadowRenderTarget, new Rectangle(0, 0, 128, 128), Color.White);
            spriteBatch.End();
        }

        void DrawOverlayText()
        {
            // HUD のテキストを表示。
            var text = "B = Light camera type (" + currentLightCameraType + ")\n" +
                "X = Shadow map form (" + shadowMapEffectForm + ")\n" +
                "Y = Use camera frustum as scene box (" + useCameraFrustumSceneBox + ")\n" +
                "L = Adjust LiSPSM optimal N (" + lispsmLightCamera.AdjustOptimalN + ")";

            spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont, text, new Vector2(65, 300), Color.Black);
            spriteBatch.DrawString(spriteFont, text, new Vector2(64, 299), Color.White);

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
                if (shadowMapEffectForm == ShadowMapEffectForm.Basic)
                {
                    shadowMapEffectForm = ShadowMapEffectForm.Variance;
                }
                else
                {
                    shadowMapEffectForm = ShadowMapEffectForm.Basic;
                }
            }

            if (currentKeyboardState.IsKeyUp(Keys.L) && lastKeyboardState.IsKeyDown(Keys.L) ||
                currentGamePadState.IsButtonUp(Buttons.LeftShoulder) && lastGamePadState.IsButtonDown(Buttons.LeftShoulder))
            {
                lispsmLightCamera.AdjustOptimalN = !lispsmLightCamera.AdjustOptimalN;
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
            float turn = -currentGamePadState.ThumbSticks.Right.X * time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Up))
                pitch += time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Down))
                pitch -= time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Left))
                turn += time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Right))
                turn -= time * 0.001f;

            var cameraRight = Vector3.Cross(Vector3.Up, cameraForward);
            var flatFront = Vector3.Cross(cameraRight, Vector3.Up);

            var pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
            var turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

            var tiltedFront = Vector3.TransformNormal(cameraForward, pitchMatrix * turnMatrix);

            if (Vector3.Dot(tiltedFront, flatFront) > 0.001f)
            {
                cameraForward = Vector3.Normalize(tiltedFront);
            }

            if (currentKeyboardState.IsKeyDown(Keys.W))
                cameraPosition += cameraForward * time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.S))
                cameraPosition -= cameraForward * time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.A))
                cameraPosition += cameraRight * time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.D))
                cameraPosition -= cameraRight * time * 0.1f;

            cameraPosition += cameraForward * currentGamePadState.ThumbSticks.Left.Y * time * 0.1f;
            cameraPosition -= cameraRight * currentGamePadState.ThumbSticks.Left.X * time * 0.1f;

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                cameraPosition = new Vector3(0, 50, 50);
                cameraForward = new Vector3(0, 0, -1);
            }

            cameraForward.Normalize();

            view = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraForward, Vector3.Up);

            cameraFrustum.Matrix = view * projection;
        }
    }

    #region Program

#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリー ポイントです。
        /// </summary>
        static void Main(string[] args)
        {
            using (MainGame game = new MainGame())
            {
                game.Run();
            }
        }
    }
#endif

    #endregion
}
