#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public class FlipTexture2D : IDisposable
    {
        public Texture2D Texture
        {
            get { return textures[index]; }
        }

        Texture2D[] textures;

        int index;

        public FlipTexture2D(GraphicsDevice graphicsDeivce, int width, int height,
            bool mipMap, SurfaceFormat format, int numberTextures)
        {
            textures = new Texture2D[numberTextures];
            for (int i = 0; i < textures.Length; ++i)
                textures[i] = new Texture2D(graphicsDeivce, width, height, mipMap, format);
        }

        public FlipTexture2D(GraphicsDevice graphicsDeivce, int width, int height,
            bool mipMap, SurfaceFormat format)
            : this(graphicsDeivce, width, height, mipMap, format, 2)
        {
        }

        public void Flip()
        {
            if (++index >= textures.Length)
                index = 0;
        }

        public void Dispose()
        {
            for (int i = 0; i < textures.Length; ++i)
            {
                if (textures[i] != null)
                {
                    textures[i].Dispose();
                    textures[i] = null;
                }
            }
        }

    }
}
