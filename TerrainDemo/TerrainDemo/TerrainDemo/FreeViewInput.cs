#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace TerrainDemo
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
            var rotationAmount = Vector3.Zero;
            var mouseState = Mouse.GetState();
            if (InitialMousePositionX != mouseState.X ||
                InitialMousePositionY != mouseState.Y)
            {
                // yaw
                rotationAmount.Y = -(mouseState.X - InitialMousePositionX);
                // pitch
                rotationAmount.X = -(mouseState.Y - InitialMousePositionY);
                ResetMousePosition();
            }

            rotationAmount *= RotationVelocity * deltaTime;
            FreeView.RotationAmount = rotationAmount;

            // 移動はキーボード操作で行う。
            var moveDirection = Vector3.Zero;
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W)) moveDirection.Z = -1;
            if (keyboardState.IsKeyDown(Keys.S)) moveDirection.Z = 1;
            if (keyboardState.IsKeyDown(Keys.A)) moveDirection.X = -1;
            if (keyboardState.IsKeyDown(Keys.D)) moveDirection.X = 1;
            if (keyboardState.IsKeyDown(Keys.Z)) moveDirection.Y = -1;
            if (keyboardState.IsKeyDown(Keys.Q)) moveDirection.Y = 1;

            FreeView.MoveDirection = moveDirection;
            FreeView.MoveAmount = MoveVelocity * deltaTime;
            //if (moveDirection.X != 0 && moveDirection.Y != 0 && moveDirection.Z != 0)
            //{
            //    FreeView.MoveDirection = moveDirection;
            //    FreeView.MoveAmount = MoveVelocity * deltaTime;
            //}
        }

        void ResetMousePosition()
        {
            Mouse.SetPosition(InitialMousePositionX, InitialMousePositionY);
        }
    }
}
