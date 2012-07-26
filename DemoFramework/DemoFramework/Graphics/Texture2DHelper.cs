#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public static class Texture2DHelper
    {
        /// <summary>
        /// Color.White で塗りつぶされた 1x1 のテクスチャを生成します。
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice。</param>
        /// <returns>Color.White で塗りつぶされた 1x1 のテクスチャ。</returns>
        public static Texture2D CreateFillTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData<Color>(new Color[] { Color.White });
            return texture;
        }
    }
}
