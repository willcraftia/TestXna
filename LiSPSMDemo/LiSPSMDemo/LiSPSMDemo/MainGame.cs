#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace LiSPSMDemo
{
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        #region DrawModelEffect

        sealed class DrawModelEffect
        {
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

            public Matrix World
            {
                get { return world; }
                set
                {
                    world = value;

                    dirtyFlags |= DirtyFlags.World;
                }
            }

            public Matrix View
            {
                get { return view; }
                set
                {
                    view = value;

                    dirtyFlags |= DirtyFlags.View;
                }
            }

            public Matrix Projection
            {
                get { return projection; }
                set
                {
                    projection = value;

                    dirtyFlags |= DirtyFlags.Projection;
                }
            }

            public Matrix LightViewProjection
            {
                get { return lightViewProjection; }
                set
                {
                    lightViewProjection = value;

                    dirtyFlags |= DirtyFlags.LightViewProjection;
                }
            }

            public Vector3 LightDirection
            {
                get { return lightDirection; }
                set
                {
                    lightDirection = value;

                    dirtyFlags |= DirtyFlags.LightDirection;
                }
            }

            public float DepthBias
            {
                get { return depthBias; }
                set
                {
                    depthBias = value;

                    dirtyFlags |= DirtyFlags.DepthBias;
                }
            }

            public Vector4 AmbientColor
            {
                get { return ambientColor; }
                set
                {
                    ambientColor = value;

                    dirtyFlags |= DirtyFlags.AmbientColor;
                }
            }

            public Texture2D Texture
            {
                get { return texture; }
                set
                {
                    texture = value;

                    dirtyFlags |= DirtyFlags.Texture;
                }
            }

            public Texture2D ShadowMap
            {
                get { return shadowMap; }
                set
                {
                    shadowMap = value;

                    dirtyFlags |= DirtyFlags.ShadowMap;
                }
            }

            public ShadowMapEffectForm ShadowMapEffectForm { get; set; }

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

        enum LightCameraType
        {
            LiSPSM = 0,
            Focused = 1,
            Basic = 2,
        }

        #endregion

        const int shadowMapSize = 2048;

        const int windowWidth = 800;

        const int windowHeight = 480;

        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;

        SpriteFont spriteFont;

        Vector3 cameraPosition = new Vector3(0, 70, 100);

        Vector3 cameraForward = new Vector3(0, -0.4472136f, -0.8944272f);

        BoundingFrustum cameraFrustum = new BoundingFrustum(Matrix.Identity);

        float cameraNear = 1.0f;

        float cameraFar = 1000.0f;

        Vector3 lightDir = new Vector3(-0.3333333f, 0.6666667f, 0.6666667f);

        float lightFar = 500.0f;

        KeyboardState lastKeyboardState = new KeyboardState();

        GamePadState lastGamePadState = new GamePadState();

        KeyboardState currentKeyboardState;

        GamePadState currentGamePadState;

        ShadowMapEffect shadowMapEffect;

        DrawModelEffect drawModelEffect;

        Model gridModel;

        Model dudeModel;

        BoundingBox sceneBox;

        bool useCameraFrustumSceneBox;

        Vector3[] corners;

        float rotateDude = 0.0f;

        RenderTarget2D bsmRenderTarget;

        RenderTarget2D vsmRenderTarget;

        RenderTarget2D currentShadowRenderTarget;

        Matrix world;

        Matrix view;

        Matrix projection;

        BasicLightCamera basicLightCamera;

        FocusedLightCamera focusedLightCamera;

        LiSPSMLightCamera lispsmLightCamera;

        LightCameraType currentLightCameraType;

        Matrix lightViewProjection;

        GaussianBlur gaussianBlur;

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
            sceneBox = new BoundingBox(new Vector3(-200), new Vector3(200));

            useCameraFrustumSceneBox = true;

            // lightDir はライトの進行方向ではなく、原点からのライトの位置方向。
            var L = -lightDir;
            L.Normalize();

            basicLightCamera = new BasicLightCamera();
            basicLightCamera.LightDirection = L;

            focusedLightCamera = new FocusedLightCamera();
            focusedLightCamera.EyeNearDistance = cameraNear;
            focusedLightCamera.EyeFarDistance = cameraFar;
            focusedLightCamera.LightDirection = L;
            focusedLightCamera.LightFarDistance = lightFar;

            lispsmLightCamera = new LiSPSMLightCamera();
            lispsmLightCamera.EyeNearDistance = cameraNear;
            lispsmLightCamera.EyeFarDistance = cameraFar;
            lispsmLightCamera.LightDirection = L;
            lispsmLightCamera.LightFarDistance = lightFar;

            currentLightCameraType = LightCameraType.LiSPSM;

            shadowMapEffectForm = ShadowMapEffectForm.Variance;
        }

        protected override void LoadContent()
        {
            shadowMapEffect = new ShadowMapEffect(Content.Load<Effect>("ShadowMap"));

            drawModelEffect = new DrawModelEffect(Content.Load<Effect>("DrawModel"));
            drawModelEffect.LightDirection = lightDir;
            drawModelEffect.DepthBias = 0.001f;
            drawModelEffect.AmbientColor = new Vector4(0.15f, 0.15f, 0.15f, 1.0f);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");

            gridModel = Content.Load<Model>("grid");
            dudeModel = Content.Load<Model>("dude");

            bsmRenderTarget = new RenderTarget2D(
                GraphicsDevice, shadowMapSize, shadowMapSize,
                false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

            vsmRenderTarget = new RenderTarget2D(
                GraphicsDevice, shadowMapSize, shadowMapSize,
                false, SurfaceFormat.Vector2, DepthFormat.Depth24Stencil8);

            gaussianBlur = new GaussianBlur(GraphicsDevice,
                vsmRenderTarget.Width, vsmRenderTarget.Height,
                SurfaceFormat.Vector2, Content.Load<Effect>("GaussianBlur"));
            gaussianBlur.Radius = 4;
            gaussianBlur.Amount = 16;
        }

        protected override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);

            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            lightViewProjection = CreateLightViewProjectionMatrix();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            switch (shadowMapEffectForm)
            {
                case ShadowMapEffectForm.Basic:
                    currentShadowRenderTarget = bsmRenderTarget;
                    break;
                case ShadowMapEffectForm.Variance:
                    currentShadowRenderTarget = vsmRenderTarget;
                    break;
            }

            CreateShadowMap();

            DrawWithShadowMap();

            DrawShadowMapToScreen();

            DrawOverlayText();

            base.Draw(gameTime);
        }

        Matrix CreateLightViewProjectionMatrix()
        {
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

            // ライトのビュー×射影行列。
            Matrix lightViewProjection;
            Matrix.Multiply(ref lightCamera.LightView, ref lightCamera.LightProjection, out lightViewProjection);

            return lightViewProjection;
        }

        void CreateShadowMap()
        {
            GraphicsDevice.SetRenderTarget(currentShadowRenderTarget);

            GraphicsDevice.Clear(Color.White);

            world = Matrix.CreateRotationY(MathHelper.ToRadians(rotateDude));
            DrawModel(dudeModel, true);

            GraphicsDevice.SetRenderTarget(null);

            if (shadowMapEffectForm == ShadowMapEffectForm.Variance)
            {
                gaussianBlur.Filter(currentShadowRenderTarget, currentShadowRenderTarget);
            }
        }

        void DrawWithShadowMap()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            world = Matrix.Identity;
            DrawModel(gridModel, false);

            world = Matrix.CreateRotationY(MathHelper.ToRadians(rotateDude));
            DrawModel(dudeModel, false);
        }

        void DrawModel(Model model, bool createShadowMap)
        {
            if (createShadowMap)
            {
                shadowMapEffect.World = world;
                shadowMapEffect.LightViewProjection = lightViewProjection;
                shadowMapEffect.Form = shadowMapEffectForm;
                shadowMapEffect.Apply();
            }
            else
            {
                drawModelEffect.World = world;
                drawModelEffect.View = view;
                drawModelEffect.Projection = projection;
                drawModelEffect.LightViewProjection = lightViewProjection;
                drawModelEffect.ShadowMap = currentShadowRenderTarget;
                drawModelEffect.ShadowMapEffectForm = shadowMapEffectForm;
            }

            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    GraphicsDevice.Indices = meshPart.IndexBuffer;

                    if (!createShadowMap)
                    {
                        drawModelEffect.Texture = (meshPart.Effect as BasicEffect).Texture;
                        drawModelEffect.Apply();
                    }

                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        meshPart.VertexOffset, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }

        void DrawShadowMapToScreen()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(currentShadowRenderTarget, new Rectangle(0, 0, 128, 128), Color.White);
            spriteBatch.End();
        }

        void DrawOverlayText()
        {
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
