#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Landscape
{
    public abstract class Partition : IDisposable
    {
        int x;

        int y;

        float width;

        float height;

        float halfWidth;

        float halfHeight;

        Vector2 position;

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        public float Width
        {
            get { return width; }
        }

        public float Height
        {
            get { return height; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        // Don't set a value into this property in subclasses.
        public PartitionLoadState LoadState { get; set; }

        public void Initialize(int x, int y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            halfWidth = width * 0.5f;
            halfHeight = height * 0.5f;
            position.X = x * width;
            position.Y = y * height;
        }

        public float CalculateEyeDistanceSquared(ref Vector3 eyePosition)
        {
            var center = new Vector3
            {
                // X * width + width * 0.5
                X = X * width + halfWidth,
                Y = 0,
                Z = Y * height + halfHeight
            };

            float result;
            Vector3.DistanceSquared(ref eyePosition, ref center, out result);

            return result;
        }

        public virtual void LoadContent() { }

        public virtual void UnloadContent() { }

        public virtual void Draw(GameTime gameTime) { }

        protected virtual void DisposeOverride(bool disposing) { }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Partition()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            UnloadContent();
            DisposeOverride(disposing);

            disposed = true;
        }

        #endregion
    }
}
