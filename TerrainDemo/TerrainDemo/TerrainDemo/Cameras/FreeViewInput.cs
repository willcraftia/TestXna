#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace TerrainDemo.Cameras
{
    public sealed class FreeViewInput
    {
        const float twoPi = 2 * MathHelper.Pi;

        public int InitialMousePositionX { get; set; }

        public int InitialMousePositionY { get; set; }

        public float RotationVelocity { get; set; }

        public float MoveVelocity { get; set; }

        public FreeView FreeView { get; set; }

        public FreeViewInput()
        {
            RotationVelocity = 0.3f;
            MoveVelocity = 30;
        }

        public void Initialize(int initialMousePositionX, int initialMousePositionY)
        {
            InitialMousePositionX = initialMousePositionX;
            InitialMousePositionY = initialMousePositionY;
            ResetMousePosition();
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            // 姿勢はマウス操作で変更する。
            var mouseState = Mouse.GetState();
            if (InitialMousePositionX != mouseState.X ||
                InitialMousePositionY != mouseState.Y)
            {
                // yaw
                var yaw = -(mouseState.X - InitialMousePositionX);
                FreeView.Yaw(yaw * RotationVelocity * deltaTime);
                // pitch
                var pitch = -(mouseState.Y - InitialMousePositionY);
                FreeView.Pitch(pitch * RotationVelocity * deltaTime);

                ResetMousePosition();
            }

            // 移動はキーボード操作で行う。
            var moveDirection = Vector3.Zero;
            var keyboardState = Keyboard.GetState();
            var distance = MoveVelocity * deltaTime;
            
            // Shift で加速。
            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                distance *= 4;

            if (keyboardState.IsKeyDown(Keys.W)) FreeView.Move(distance);
            if (keyboardState.IsKeyDown(Keys.S)) FreeView.Move(-distance);
            if (keyboardState.IsKeyDown(Keys.A)) FreeView.Strafe(distance);
            if (keyboardState.IsKeyDown(Keys.D)) FreeView.Strafe(-distance);
            if (keyboardState.IsKeyDown(Keys.Q)) FreeView.Up(distance);
            if (keyboardState.IsKeyDown(Keys.Z)) FreeView.Up(-distance);
        }

        void ResetMousePosition()
        {
            Mouse.SetPosition(InitialMousePositionX, InitialMousePositionY);
        }
    }
}
