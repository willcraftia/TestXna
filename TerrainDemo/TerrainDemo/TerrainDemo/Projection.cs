﻿#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo
{
    /// <summary>
    /// Projection 行列を管理するクラスです。
    /// </summary>
    public abstract class Projection
    {
        /// <summary>
        /// Projection 行列。
        /// </summary>
        public Matrix Matrix = Matrix.Identity;

        /// <summary>
        /// Matrix プロパティを更新します。
        /// </summary>
        public abstract void Update();
    }
}
