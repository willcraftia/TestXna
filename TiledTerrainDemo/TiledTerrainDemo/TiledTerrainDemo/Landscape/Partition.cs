#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Landscape
{
    public abstract class Partition : IDisposable
    {
        public int X { get; set; }

        public int Y { get; set; }

        // Don't set a value into this property in subclasses.
        public PartitionLoadState LoadState { get; set; }

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
